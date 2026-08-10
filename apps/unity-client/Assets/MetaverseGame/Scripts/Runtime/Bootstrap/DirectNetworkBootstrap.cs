using System;
using System.Text;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using MetaverseGame.Gameplay;

namespace MetaverseGame.Bootstrap
{
    [RequireComponent(typeof(NetworkManager), typeof(UnityTransport))]
    public sealed class DirectNetworkBootstrap : MonoBehaviour
    {
        private enum StartMode
        {
            Host,
            Client,
            Server,
            Manual,
        }

        [SerializeField] private StartMode defaultMode = StartMode.Host;
        [SerializeField] private string address = "127.0.0.1";
        [SerializeField] private string listenAddress = "127.0.0.1";
        [SerializeField] private ushort port = 7777;
        [SerializeField] private string displayName = "Unity Guest";

        private NetworkManager networkManager;
        private MatchSessionRegistry sessionRegistry;

        private void Awake()
        {
            Application.runInBackground = true;
            networkManager = GetComponent<NetworkManager>();
            sessionRegistry = GetComponent<MatchSessionRegistry>();
            if (sessionRegistry == null)
            {
                sessionRegistry = gameObject.AddComponent<MatchSessionRegistry>();
            }
            networkManager.NetworkConfig.ConnectionApproval = true;
            networkManager.ConnectionApprovalCallback = ApprovalCheck;
            networkManager.NetworkConfig.TickRate = 30;
            networkManager.OnClientConnectedCallback += OnClientConnected;
            networkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }

        private void Start()
        {
            StartMode mode = ReadMode(Environment.GetCommandLineArgs());
#if UNITY_SERVER
            mode = StartMode.Server;
#endif
            if (mode == StartMode.Manual)
            {
                return;
            }

            string[] arguments = Environment.GetCommandLineArgs();
            string resolvedAddress = ReadArgument(arguments, "-ip", address);
            string resolvedListenAddress = ReadArgument(
                arguments,
                "-listen-ip",
                listenAddress);
            ushort resolvedPort = ReadPort(arguments, port);
            if (mode == StartMode.Host || mode == StartMode.Server)
            {
                sessionRegistry?.ResetForNewMatch();
            }
            if (mode != StartMode.Server)
            {
                string sessionId = ResolveSessionId(arguments);
                string payload = string.Join(
                    SessionTicketSeparator.ToString(),
                    sessionId,
                    string.IsNullOrWhiteSpace(displayName)
                        ? "Unity Guest"
                        : displayName.Trim());
                networkManager.NetworkConfig.ConnectionData =
                    Encoding.UTF8.GetBytes(payload);
            }

            UnityTransport transport = GetComponent<UnityTransport>();
            transport.SetConnectionData(
                true,
                resolvedAddress,
                resolvedPort,
                resolvedListenAddress);

            bool started = mode switch
            {
                StartMode.Host => networkManager.StartHost(),
                StartMode.Client => networkManager.StartClient(),
                StartMode.Server => networkManager.StartServer(),
                _ => false,
            };
            if (!started)
            {
                Debug.LogError($"Unable to start NGO in {mode} mode.");
                return;
            }

            Debug.Log(
                $"NGO {mode} started at {resolvedAddress}:{resolvedPort} " +
                $"(listen {resolvedListenAddress}).");
        }

        private void ApprovalCheck(
            NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            if (!TryReadConnectionTicket(
                    request.Payload,
                    out string sessionId,
                    out string resolvedDisplayName))
            {
                response.Approved = false;
                response.Reason = "session_id_required";
                response.Pending = false;
                return;
            }

            string rejectionReason = string.Empty;
            if (sessionRegistry == null ||
                !sessionRegistry.TryApproveConnection(
                    request.ClientNetworkId,
                    sessionId,
                    resolvedDisplayName,
                    out SessionRecord session,
                    out rejectionReason))
            {
                response.Approved = false;
                response.Reason = string.IsNullOrWhiteSpace(rejectionReason)
                    ? "connection_rejected"
                    : rejectionReason;
                response.Pending = false;
                return;
            }

            response.Approved = true;
            response.CreatePlayerObject = true;
            response.PlayerPrefabHash = null;
            response.Position = NetworkPlayerController.ResolveSpawnPosition(
                session.SpawnIndex);
            response.Rotation = Quaternion.identity;
            response.Pending = false;
        }

        private StartMode ReadMode(string[] arguments)
        {
            string value = ReadArgument(arguments, "-mode", defaultMode.ToString());
            return Enum.TryParse(value, true, out StartMode mode) ? mode : defaultMode;
        }

        private static ushort ReadPort(string[] arguments, ushort fallback)
        {
            string value = ReadArgument(arguments, "-port", fallback.ToString());
            return ushort.TryParse(value, out ushort parsed) ? parsed : fallback;
        }

        private static string ReadArgument(string[] arguments, string name, string fallback)
        {
            for (int index = 0; index < arguments.Length - 1; index++)
            {
                if (string.Equals(arguments[index], name, StringComparison.OrdinalIgnoreCase))
                {
                    return arguments[index + 1];
                }
            }
            return fallback;
        }

        private static string ResolveSessionId(string[] arguments)
        {
            string commandLineSessionId = ReadArgument(
                arguments,
                "-session-id",
                string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(commandLineSessionId))
            {
                PlayerPrefs.SetString(SessionIdPlayerPrefsKey, commandLineSessionId);
                PlayerPrefs.Save();
                return commandLineSessionId;
            }

            string storedSessionId = PlayerPrefs.GetString(
                SessionIdPlayerPrefsKey,
                string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(storedSessionId))
            {
                return storedSessionId;
            }

            string generatedSessionId = Guid.NewGuid().ToString("N");
            PlayerPrefs.SetString(SessionIdPlayerPrefsKey, generatedSessionId);
            PlayerPrefs.Save();
            return generatedSessionId;
        }

        private static bool TryReadConnectionTicket(
            byte[] payload,
            out string sessionId,
            out string resolvedDisplayName)
        {
            sessionId = string.Empty;
            resolvedDisplayName = "Unity Guest";
            if (payload == null || payload.Length == 0)
            {
                return false;
            }

            string raw = Encoding.UTF8.GetString(payload);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            string[] parts = raw.Split(new[] { SessionTicketSeparator }, 2);
            sessionId = parts[0].Trim();
            if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
            {
                resolvedDisplayName = parts[1].Trim();
            }
            return !string.IsNullOrWhiteSpace(sessionId);
        }

        private void OnClientConnected(ulong clientId)
        {
            if (networkManager != null &&
                networkManager.IsServer &&
                sessionRegistry != null &&
                sessionRegistry.TryGetSession(clientId, out SessionRecord session))
            {
                string reconnectText = session.ConnectionCount > 1
                    ? $" reconnects={session.ConnectionCount - 1}"
                    : string.Empty;
                Debug.Log(
                    $"NGO client connected: {clientId} " +
                    $"session={session.SessionId} role={session.Role}{reconnectText}");
                return;
            }

            Debug.Log($"NGO client connected: {clientId}");
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (networkManager != null && networkManager.IsServer)
            {
                if (sessionRegistry != null &&
                    sessionRegistry.TryGetSession(clientId, out SessionRecord session))
                {
                    Debug.Log(
                        $"NGO client disconnected: {clientId} " +
                        $"session={session.SessionId} role={session.Role}");
                }
                else
                {
                    Debug.Log($"NGO client disconnected: {clientId}");
                }

                sessionRegistry?.MarkDisconnected(clientId);
                return;
            }

            Debug.Log($"NGO client disconnected: {clientId}");
        }

        private void OnDestroy()
        {
            if (networkManager == null)
            {
                return;
            }
            networkManager.OnClientConnectedCallback -= OnClientConnected;
            networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        private const char SessionTicketSeparator = '\u001F';
        private const string SessionIdPlayerPrefsKey = "MetaverseDApp.SessionId";
    }
}
