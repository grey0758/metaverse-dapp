import type { PlayerPublicState } from "@metaverse/protocol";

export type Role = "goose" | "duck";
export type RoomPhase = "lobby" | "playing" | "finished";

type PlayerState = PlayerPublicState & {
  role?: Role;
  lastSequence: number;
  pendingX: number;
  pendingZ: number;
};

export class GameRoom {
  readonly players = new Map<string, PlayerState>();
  phase: RoomPhase = "lobby";
  tickNumber = 0;

  constructor(
    readonly code: string,
    private readonly capacity = 12,
    private readonly speed = 4,
  ) {}

  join(id: string, name: string): void {
    if (this.phase !== "lobby") {
      throw new Error("match_already_started");
    }
    if (!this.players.has(id) && this.players.size >= this.capacity) {
      throw new Error("room_full");
    }
    this.players.set(id, {
      id,
      name,
      ready: false,
      x: 0,
      z: 0,
      lastSequence: -1,
      pendingX: 0,
      pendingZ: 0,
    });
  }

  leave(id: string): void {
    this.players.delete(id);
  }

  setReady(id: string, ready: boolean): void {
    const player = this.requirePlayer(id);
    if (this.phase !== "lobby") {
      throw new Error("match_already_started");
    }
    player.ready = ready;
  }

  canStart(): boolean {
    return (
      this.phase === "lobby" &&
      this.players.size >= 2 &&
      [...this.players.values()].every((player) => player.ready)
    );
  }

  start(random: () => number = Math.random): Map<string, Role> {
    if (!this.canStart()) {
      throw new Error("room_not_ready");
    }

    const ids = [...this.players.keys()];
    for (let index = ids.length - 1; index > 0; index -= 1) {
      const swapIndex = Math.floor(random() * (index + 1));
      [ids[index], ids[swapIndex]] = [ids[swapIndex]!, ids[index]!];
    }

    const duckCount = Math.max(1, Math.floor(ids.length / 5));
    const roles = new Map<string, Role>();
    ids.forEach((id, index) => {
      const role: Role = index < duckCount ? "duck" : "goose";
      this.requirePlayer(id).role = role;
      roles.set(id, role);
    });
    this.phase = "playing";
    return roles;
  }

  setInput(id: string, sequence: number, x: number, z: number): void {
    const player = this.requirePlayer(id);
    if (this.phase !== "playing" || sequence <= player.lastSequence) {
      return;
    }
    const magnitude = Math.hypot(x, z);
    const scale = magnitude > 1 ? 1 / magnitude : 1;
    player.pendingX = x * scale;
    player.pendingZ = z * scale;
    player.lastSequence = sequence;
  }

  tick(deltaSeconds: number): PlayerPublicState[] {
    this.tickNumber += 1;
    if (this.phase === "playing") {
      for (const player of this.players.values()) {
        player.x += player.pendingX * this.speed * deltaSeconds;
        player.z += player.pendingZ * this.speed * deltaSeconds;
      }
    }
    return this.publicPlayers();
  }

  publicPlayers(): PlayerPublicState[] {
    return [...this.players.values()].map(({ id, name, ready, x, z }) => ({
      id,
      name,
      ready,
      x: Number(x.toFixed(3)),
      z: Number(z.toFixed(3)),
    }));
  }

  roleFor(id: string): Role | undefined {
    return this.requirePlayer(id).role;
  }

  private requirePlayer(id: string): PlayerState {
    const player = this.players.get(id);
    if (!player) {
      throw new Error("player_not_in_room");
    }
    return player;
  }
}
