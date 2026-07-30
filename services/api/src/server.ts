import { buildApp } from "./app.js";

const host = process.env.API_HOST ?? "127.0.0.1";
const port = Number(process.env.API_PORT ?? 8788);
const app = await buildApp();

await app.listen({ host, port });
console.log(`api listening on http://${host}:${port}`);
