using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

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

        private NetworkManager networkManager;

        private void Awake()
        {
            Application.runInBackground = true;
            networkManager = GetComponent<NetworkManager>();
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

        private static void OnClientConnected(ulong clientId)
        {
            Debug.Log($"NGO client connected: {clientId}");
        }

        private static void OnClientDisconnected(ulong clientId)
        {
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
    }
}
