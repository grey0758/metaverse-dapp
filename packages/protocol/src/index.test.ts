import { describe, expect, it } from "vitest";
import { parseClientMessage } from "./index.js";

describe("client protocol", () => {
  it("accepts normalized room codes", () => {
    expect(
      parseClientMessage({
        v: 1,
        type: "join_room",
        requestId: "r1",
        roomCode: "DUCK42",
      }),
    ).toMatchObject({ roomCode: "DUCK42" });
  });

  it("rejects movement outside the normalized input range", () => {
    expect(() =>
      parseClientMessage({
        v: 1,
        type: "input",
        requestId: "r2",
        sequence: 1,
        x: 2,
        z: 0,
      }),
    ).toThrow();
  });
});
