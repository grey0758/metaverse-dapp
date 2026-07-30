import { randomUUID } from "node:crypto";
import { createServer } from "node:http";
import {
  parseClientMessage,
  protocolVersion,
  type ServerMessage,
} from "@metaverse/protocol";
import { WebSocket, WebSocketServer } from "ws";
import { GameRoom } from "./room.js";

type ConnectionState = {
  playerId?: string;
  playerName?: string;
  roomCode?: string;
};

const host = process.env.GAME_HOST ?? "127.0.0.1";
const port = Number(process.env.GAME_PORT ?? 8787);
const tickRate = 20;
const rooms = new Map<string, GameRoom>();
const connections = new Map<WebSocket, ConnectionState>();
const playerSockets = new Map<string, WebSocket>();

const httpServer = createServer((request, response) => {
  if (request.url === "/health") {
    response.writeHead(200, { "content-type": "application/json" });
    response.end(JSON.stringify({ ok: true, rooms: rooms.size }));
    return;
  }
  response.writeHead(404).end();
});

const socketServer = new WebSocketServer({ server: httpServer });

function send(socket: WebSocket, message: ServerMessage): void {
  if (socket.readyState === WebSocket.OPEN) {
    socket.send(JSON.stringify(message));
  }
}

function sendError(
  socket: WebSocket,
  requestId: string,
  code: string,
  message = code,
): void {
  send(socket, {
    v: protocolVersion,
    type: "error",
    requestId,
    code,
    message,
  });
}

function broadcastRoom(room: GameRoom, requestId = "server"): void {
  const payload: ServerMessage = {
    v: protocolVersion,
    type: "room_state",
    requestId,
    roomCode: room.code,
    phase: room.phase,
    players: room.publicPlayers(),
  };
  for (const player of room.players.values()) {
    const socket = playerSockets.get(player.id);
    if (socket) send(socket, payload);
  }
}

socketServer.on("connection", (socket) => {
  connections.set(socket, {});

  socket.on("message", (raw) => {
    let requestId = "invalid";
    try {
      const parsedJson: unknown = JSON.parse(raw.toString());
      if (
        parsedJson &&
        typeof parsedJson === "object" &&
        "requestId" in parsedJson &&
        typeof parsedJson.requestId === "string"
      ) {
        requestId = parsedJson.requestId;
      }
      const message = parseClientMessage(parsedJson);
      const state = connections.get(socket)!;

      if (message.type === "guest_auth") {
        if (state.playerId) {
          sendError(socket, message.requestId, "already_authenticated");
          return;
        }
        state.playerId = randomUUID();
        state.playerName = message.name;
        playerSockets.set(state.playerId, socket);
        send(socket, {
          v: protocolVersion,
          type: "auth_ok",
          requestId: message.requestId,
          playerId: state.playerId,
        });
        return;
      }

      if (!state.playerId || !state.playerName) {
        sendError(socket, message.requestId, "authentication_required");
        return;
      }

      if (message.type === "join_room") {
        if (state.roomCode) {
          sendError(socket, message.requestId, "already_in_room");
          return;
        }
        const room =
          rooms.get(message.roomCode) ?? new GameRoom(message.roomCode);
        room.join(state.playerId, state.playerName);
        rooms.set(room.code, room);
        state.roomCode = room.code;
        broadcastRoom(room, message.requestId);
        return;
      }

      if (message.type === "ping") {
        send(socket, {
          v: protocolVersion,
          type: "pong",
          requestId: message.requestId,
          sentAt: message.sentAt,
          serverAt: Date.now(),
        });
        return;
      }

      const room = state.roomCode ? rooms.get(state.roomCode) : undefined;
      if (!room) {
        sendError(socket, message.requestId, "room_required");
        return;
      }

      if (message.type === "ready") {
        room.setReady(state.playerId, message.ready);
        broadcastRoom(room, message.requestId);
        if (room.canStart()) {
          const roles = room.start();
          const startedAt = Date.now();
          for (const [playerId, role] of roles) {
            const playerSocket = playerSockets.get(playerId);
            if (playerSocket) {
              send(playerSocket, {
                v: protocolVersion,
                type: "match_started",
                requestId: "server",
                role,
                startedAt,
              });
            }
          }
          broadcastRoom(room);
        }
        return;
      }

      if (message.type === "input") {
        room.setInput(state.playerId, message.sequence, message.x, message.z);
      }
    } catch (error) {
      const code =
        error instanceof Error && /^[a-z_]+$/.test(error.message)
          ? error.message
          : "invalid_message";
      sendError(socket, requestId, code);
    }
  });

  socket.on("close", () => {
    const state = connections.get(socket);
    connections.delete(socket);
    if (!state?.playerId) return;
    playerSockets.delete(state.playerId);
    if (!state.roomCode) return;
    const room = rooms.get(state.roomCode);
    if (!room) return;
    room.leave(state.playerId);
    if (room.players.size === 0) {
      rooms.delete(room.code);
    } else {
      broadcastRoom(room);
    }
  });
});

setInterval(() => {
  for (const room of rooms.values()) {
    if (room.phase !== "playing") continue;
    const snapshot: ServerMessage = {
      v: protocolVersion,
      type: "snapshot",
      requestId: "server",
      tick: room.tickNumber + 1,
      players: room.tick(1 / tickRate),
    };
    for (const player of room.players.values()) {
      const socket = playerSockets.get(player.id);
      if (socket) send(socket, snapshot);
    }
  }
}, 1000 / tickRate).unref();

httpServer.listen(port, host, () => {
  console.log(`game-server listening on ws://${host}:${port}`);
});
