import { z } from "zod";

export const protocolVersion = 1 as const;

const requestBase = {
  v: z.literal(protocolVersion),
  requestId: z.string().min(1).max(80),
};

export const ClientMessageSchema = z.discriminatedUnion("type", [
  z.object({
    ...requestBase,
    type: z.literal("guest_auth"),
    name: z.string().trim().min(1).max(24),
  }),
  z.object({
    ...requestBase,
    type: z.literal("join_room"),
    roomCode: z.string().trim().regex(/^[A-Z0-9]{4,8}$/),
  }),
  z.object({
    ...requestBase,
    type: z.literal("ready"),
    ready: z.boolean(),
  }),
  z.object({
    ...requestBase,
    type: z.literal("input"),
    sequence: z.number().int().nonnegative(),
    x: z.number().finite().min(-1).max(1),
    z: z.number().finite().min(-1).max(1),
  }),
  z.object({
    ...requestBase,
    type: z.literal("ping"),
    sentAt: z.number().int().nonnegative(),
  }),
]);

export type ClientMessage = z.infer<typeof ClientMessageSchema>;

export type PlayerPublicState = {
  id: string;
  name: string;
  ready: boolean;
  x: number;
  z: number;
};

export type ServerMessage =
  | {
      v: typeof protocolVersion;
      type: "auth_ok";
      requestId: string;
      playerId: string;
    }
  | {
      v: typeof protocolVersion;
      type: "room_state";
      requestId: string;
      roomCode: string;
      phase: "lobby" | "playing" | "finished";
      players: PlayerPublicState[];
    }
  | {
      v: typeof protocolVersion;
      type: "match_started";
      requestId: string;
      role: "goose" | "duck";
      startedAt: number;
    }
  | {
      v: typeof protocolVersion;
      type: "snapshot";
      requestId: string;
      tick: number;
      players: PlayerPublicState[];
    }
  | {
      v: typeof protocolVersion;
      type: "pong";
      requestId: string;
      sentAt: number;
      serverAt: number;
    }
  | {
      v: typeof protocolVersion;
      type: "error";
      requestId: string;
      code: string;
      message: string;
    };

export function parseClientMessage(input: unknown): ClientMessage {
  return ClientMessageSchema.parse(input);
}
