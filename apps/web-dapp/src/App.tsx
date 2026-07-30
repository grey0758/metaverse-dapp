import { useState } from "react";

const apiUrl = import.meta.env.VITE_API_URL ?? "http://127.0.0.1:8788";
const chainCaip2 = import.meta.env.VITE_CHAIN_CAIP2?.trim();

type Status = { tone: "quiet" | "good" | "warn"; text: string };

export function App() {
  const [status, setStatus] = useState<Status>({
    tone: "quiet",
    text: "Guest play is ready. Wallet connection is optional.",
  });
  const [account, setAccount] = useState<string>();

  async function createGuestSession() {
    setStatus({ tone: "quiet", text: "Creating guest session…" });
    const response = await fetch(`${apiUrl}/v1/auth/guest`, {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({ displayName: "Web Guest" }),
    });
    if (!response.ok) {
      setStatus({ tone: "warn", text: "Guest API is not reachable." });
      return;
    }
    setStatus({ tone: "good", text: "Guest session created. No wallet required." });
  }

  async function connectWallet() {
    if (!window.ethereum) {
      setStatus({ tone: "warn", text: "No EIP-1193 wallet was detected." });
      return;
    }
    if (!chainCaip2) {
      setStatus({
        tone: "warn",
        text: "Wallet login is intentionally disabled until a chain is selected.",
      });
      return;
    }

    const accounts = (await window.ethereum.request({
      method: "eth_requestAccounts",
    })) as string[];
    const selected = accounts[0];
    if (!selected) return;
    setAccount(selected);

    const challengeResponse = await fetch(`${apiUrl}/v1/auth/wallet/challenge`, {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({ address: selected }),
    });
    if (!challengeResponse.ok) {
      setStatus({ tone: "warn", text: "Wallet challenge is not configured." });
      return;
    }
    const challenge = (await challengeResponse.json()) as {
      nonce: string;
      message: string;
    };
    const signature = (await window.ethereum.request({
      method: "personal_sign",
      params: [challenge.message, selected],
    })) as string;
    const verifyResponse = await fetch(`${apiUrl}/v1/auth/wallet/verify`, {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({ nonce: challenge.nonce, signature }),
    });
    setStatus(
      verifyResponse.ok
        ? { tone: "good", text: "Wallet signature verified for this session." }
        : { tone: "warn", text: "Signature verification failed." },
    );
  }

  return (
    <main>
      <header>
        <div className="mark">F</div>
        <div>
          <p className="eyebrow">FEATHERFALL PROTOCOL</p>
          <h1>Social deduction,<br />with the chain kept in its lane.</h1>
        </div>
      </header>

      <section className="grid">
        <article className="hero-card">
          <span className="chip">GAME FIRST</span>
          <h2>Enter the lobby as a guest.</h2>
          <p>
            Live movement, roles, voice, and match authority stay off-chain.
            Wallets belong in identity and collectibles—not the controls.
          </p>
          <button onClick={createGuestSession}>Start guest session</button>
        </article>

        <article className="wallet-card">
          <span className="chip outline">OPTIONAL DAPP</span>
          <h2>Connect only when you choose.</h2>
          <p className="account">
            {account ? `${account.slice(0, 8)}…${account.slice(-6)}` : "No wallet connected"}
          </p>
          <button className="secondary" onClick={connectWallet}>
            Connect and sign in
          </button>
          <small>{chainCaip2 || "No production chain selected"}</small>
        </article>
      </section>

      <aside className={`status ${status.tone}`}>{status.text}</aside>

      <footer>
        <span>Authoritative multiplayer</span>
        <span>Optional ownership</span>
        <span>Mobile-store safe boundary</span>
      </footer>
    </main>
  );
}
