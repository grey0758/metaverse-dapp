import { describe, expect, it } from "vitest";
import { GameRoom } from "./room.js";

describe("GameRoom", () => {
  it("starts only when every player is ready and keeps roles private", () => {
    const room = new GameRoom("TEST");
    room.join("a", "Alice");
    room.join("b", "Bob");
    room.setReady("a", true);
    expect(room.canStart()).toBe(false);
    room.setReady("b", true);

    const roles = room.start(() => 0.5);
    expect([...roles.values()].filter((role) => role === "duck")).toHaveLength(1);
    expect(room.publicPlayers()).not.toHaveProperty("role");
  });

  it("normalizes diagonal movement and ignores replayed input", () => {
    const room = new GameRoom("TEST", 12, 4);
    room.join("a", "Alice");
    room.join("b", "Bob");
    room.setReady("a", true);
    room.setReady("b", true);
    room.start(() => 0.5);
    room.setInput("a", 1, 1, 1);
    const first = room.tick(1).find((player) => player.id === "a")!;
    expect(Math.hypot(first.x, first.z)).toBeCloseTo(4, 2);

    room.setInput("a", 1, -1, 0);
    const second = room.tick(1).find((player) => player.id === "a")!;
    expect(second.x).toBeGreaterThan(first.x);
  });
});
