using System;
using System.Threading.Tasks;
using MetaverseGame.Config;
using MetaverseGame.Gameplay;
using MetaverseGame.Networking;
using UnityEngine;

namespace MetaverseGame.Bootstrap
{
    [RequireComponent(typeof(GameSocketClient))]
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private GameEnvironment environment;
        [SerializeField] private PlayerMotor localPlayer;
        [SerializeField] private string displayName = "Unity Guest";
        [SerializeField] private string developmentRoom = "DUCK42";
        [SerializeField, Min(1f)] private float inputRate = 20f;

        private GameSocketClient client;
        private string playerId;
        private int inputSequence;
        private float nextInputAt;

        private async void Start()
        {
            client = GetComponent<GameSocketClient>();
            client.MessageReceived += OnMessage;
            if (environment == null)
            {
                environment = Resources.Load<GameEnvironment>("GameEnvironment");
            }
            if (environment == null)
            {
                Debug.LogError("GameEnvironment asset is missing.");
                return;
            }

            try
            {
                await client.ConnectAsync(environment.GameServerUrl);
                await client.SendAsync(new ClientMessage
                {
                    type = "guest_auth",
                    requestId = Guid.NewGuid().ToString("N"),
                    name = displayName,
                });
            }
            catch (Exception exception)
            {
                Debug.LogError($"Unable to connect to game server: {exception.Message}");
            }
        }

        private async void Update()
        {
            if (string.IsNullOrEmpty(playerId) || localPlayer == null || Time.time < nextInputAt)
            {
                return;
            }
            nextInputAt = Time.time + 1f / inputRate;
            Vector2 input = localPlayer.CurrentInput;
            try
            {
                await client.SendAsync(new ClientMessage
                {
                    type = "input",
                    requestId = Guid.NewGuid().ToString("N"),
                    sequence = ++inputSequence,
                    x = input.x,
                    z = input.y,
                });
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Movement input was not sent: {exception.Message}");
            }
        }

        private async void OnMessage(ServerMessage message)
        {
            if (message.type == "auth_ok")
            {
                playerId = message.playerId;
                await client.SendAsync(new ClientMessage
                {
                    type = "join_room",
                    requestId = Guid.NewGuid().ToString("N"),
                    roomCode = RoomCode.Normalize(developmentRoom),
                });
                return;
            }

            if (message.type == "match_started")
            {
                Debug.Log($"Private role assigned: {message.role}");
            }
            else if (message.type == "error")
            {
                Debug.LogWarning($"Server error [{message.code}]: {message.message}");
            }
        }

        public async Task SetReadyAsync(bool ready)
        {
            await client.SendAsync(new ClientMessage
            {
                type = "ready",
                requestId = Guid.NewGuid().ToString("N"),
                ready = ready,
            });
        }

        private void OnDestroy()
        {
            if (client != null)
            {
                client.MessageReceived -= OnMessage;
            }
        }
    }
}
