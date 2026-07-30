using UnityEngine;

namespace MetaverseGame.Config
{
    [CreateAssetMenu(menuName = "Metaverse DApp/Game Environment")]
    public sealed class GameEnvironment : ScriptableObject
    {
        [SerializeField] private string apiBaseUrl = "http://127.0.0.1:8788";
        [SerializeField] private string gameServerUrl = "ws://127.0.0.1:8787";
        [SerializeField] private string chainCaip2 = "";
        [SerializeField] private bool walletRequired;

        public string ApiBaseUrl => apiBaseUrl;
        public string GameServerUrl => gameServerUrl;
        public string ChainCaip2 => chainCaip2;
        public bool WalletRequired => walletRequired;
    }
}
