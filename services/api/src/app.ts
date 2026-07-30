import { randomBytes, randomUUID } from "node:crypto";
import cors from "@fastify/cors";
import Fastify from "fastify";
import { getAddress, verifyMessage, type Hex } from "viem";
import { z } from "zod";

type Challenge = {
  address: `0x${string}`;
  message: string;
  expiresAt: number;
};

const guestSchema = z.object({
  displayName: z.string().trim().min(1).max(24),
});

const challengeSchema = z.object({
  address: z.string().regex(/^0x[a-fA-F0-9]{40}$/),
});

const verifySchema = z.object({
  nonce: z.string().min(16).max(128),
  signature: z.string().regex(/^0x[a-fA-F0-9]+$/),
});

function walletConfig():
  | { chainCaip2: string; domain: string; uri: string }
  | undefined {
  const chainCaip2 = process.env.CHAIN_CAIP2?.trim();
  const domain = process.env.API_PUBLIC_DOMAIN?.trim();
  if (!chainCaip2 || !domain || !/^eip155:\d+$/.test(chainCaip2)) {
    return undefined;
  }
  return { chainCaip2, domain, uri: `https://${domain}` };
}

export async function buildApp() {
  const app = Fastify({ logger: false });
  const challenges = new Map<string, Challenge>();
  const sessions = new Map<string, { subject: string; expiresAt: number }>();

  await app.register(cors, {
    origin: process.env.WEB_ORIGIN ?? "http://127.0.0.1:5173",
  });

  app.get("/health", async () => ({
    ok: true,
    walletAuthConfigured: Boolean(walletConfig()),
  }));

  app.post("/v1/auth/guest", async (request, reply) => {
    const parsed = guestSchema.safeParse(request.body);
    if (!parsed.success) {
      return reply.code(400).send({ error: "invalid_display_name" });
    }
    const sessionToken = randomBytes(32).toString("base64url");
    const subject = `guest:${randomUUID()}`;
    const expiresAt = Date.now() + 12 * 60 * 60 * 1000;
    sessions.set(sessionToken, { subject, expiresAt });
    return { sessionToken, subject, displayName: parsed.data.displayName, expiresAt };
  });

  app.post("/v1/auth/wallet/challenge", async (request, reply) => {
    const config = walletConfig();
    if (!config) {
      return reply.code(503).send({ error: "wallet_auth_not_configured" });
    }
    const parsed = challengeSchema.safeParse(request.body);
    if (!parsed.success) {
      return reply.code(400).send({ error: "invalid_address" });
    }

    const address = getAddress(parsed.data.address);
    const nonce = randomBytes(18).toString("base64url");
    const issuedAt = new Date();
    const expiresAt = issuedAt.getTime() + 5 * 60 * 1000;
    const chainId = config.chainCaip2.split(":")[1]!;
    const message = [
      `${config.domain} wants you to sign in with your Ethereum account:`,
      address,
      "",
      "Sign in to the Metaverse DApp account. This does not submit a transaction.",
      "",
      `URI: ${config.uri}`,
      "Version: 1",
      `Chain ID: ${chainId}`,
      `Nonce: ${nonce}`,
      `Issued At: ${issuedAt.toISOString()}`,
      `Expiration Time: ${new Date(expiresAt).toISOString()}`,
    ].join("\n");
    challenges.set(nonce, { address, message, expiresAt });
    return { nonce, message, expiresAt, account: `${config.chainCaip2}:${address}` };
  });

  app.post("/v1/auth/wallet/verify", async (request, reply) => {
    const parsed = verifySchema.safeParse(request.body);
    if (!parsed.success) {
      return reply.code(400).send({ error: "invalid_verification_payload" });
    }
    const challenge = challenges.get(parsed.data.nonce);
    challenges.delete(parsed.data.nonce);
    if (!challenge || challenge.expiresAt < Date.now()) {
      return reply.code(401).send({ error: "challenge_expired_or_unknown" });
    }

    const valid = await verifyMessage({
      address: challenge.address,
      message: challenge.message,
      signature: parsed.data.signature as Hex,
    });
    if (!valid) {
      return reply.code(401).send({ error: "invalid_signature" });
    }

    const sessionToken = randomBytes(32).toString("base64url");
    const expiresAt = Date.now() + 12 * 60 * 60 * 1000;
    const subject = `wallet:${challenge.address.toLowerCase()}`;
    sessions.set(sessionToken, { subject, expiresAt });
    return { sessionToken, subject, expiresAt, verification: "eoa" };
  });

  return app;
}
