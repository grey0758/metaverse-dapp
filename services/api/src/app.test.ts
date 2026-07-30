import { afterEach, describe, expect, it } from "vitest";
import { buildApp } from "./app.js";

describe("account API", () => {
  afterEach(() => {
    delete process.env.CHAIN_CAIP2;
    delete process.env.API_PUBLIC_DOMAIN;
  });

  it("creates a guest session without wallet configuration", async () => {
    const app = await buildApp();
    const response = await app.inject({
      method: "POST",
      url: "/v1/auth/guest",
      payload: { displayName: "Grey Goose" },
    });
    expect(response.statusCode).toBe(200);
    expect(response.json().subject).toMatch(/^guest:/);
    await app.close();
  });

  it("does not invent a chain for wallet authentication", async () => {
    const app = await buildApp();
    const response = await app.inject({
      method: "POST",
      url: "/v1/auth/wallet/challenge",
      payload: { address: "0x0000000000000000000000000000000000000001" },
    });
    expect(response.statusCode).toBe(503);
    expect(response.json()).toEqual({ error: "wallet_auth_not_configured" });
    await app.close();
  });
});
