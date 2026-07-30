using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MetaverseGame.Networking
{
    public sealed class GameSocketClient : MonoBehaviour
    {
        private readonly ConcurrentQueue<string> received = new();
        private ClientWebSocket socket;
        private CancellationTokenSource lifetime;

        public event Action<ServerMessage> MessageReceived;
        public bool IsConnected => socket?.State == WebSocketState.Open;

        public async Task ConnectAsync(string url)
        {
            if (IsConnected)
            {
                return;
            }

            lifetime = new CancellationTokenSource();
            socket = new ClientWebSocket();
            await socket.ConnectAsync(new Uri(url), lifetime.Token);
            _ = ReceiveLoopAsync(lifetime.Token);
        }

        public async Task SendAsync(ClientMessage message)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Game socket is not connected.");
            }
            string json = JsonUtility.ToJson(message);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            await socket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                lifetime.Token);
        }

        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            byte[] bytes = new byte[32 * 1024];
            while (!token.IsCancellationRequested && socket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await socket.ReceiveAsync(
                    new ArraySegment<byte>(bytes),
                    token);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }
                if (!result.EndOfMessage)
                {
                    Debug.LogWarning("Ignored an oversized game-server message.");
                    continue;
                }
                received.Enqueue(Encoding.UTF8.GetString(bytes, 0, result.Count));
            }
        }

        private void Update()
        {
            while (received.TryDequeue(out string json))
            {
                try
                {
                    ServerMessage message = JsonUtility.FromJson<ServerMessage>(json);
                    MessageReceived?.Invoke(message);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"Invalid game-server message: {exception.Message}");
                }
            }
        }

        private async void OnDestroy()
        {
            lifetime?.Cancel();
            if (socket == null)
            {
                return;
            }
            try
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "client_shutdown",
                        CancellationToken.None);
                }
            }
            catch (WebSocketException)
            {
                // The authoritative server can already be gone during editor shutdown.
            }
            socket.Dispose();
            lifetime?.Dispose();
        }
    }
}
