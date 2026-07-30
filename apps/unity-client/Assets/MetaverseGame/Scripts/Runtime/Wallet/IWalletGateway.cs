using System.Threading.Tasks;

namespace MetaverseGame.Wallet
{
    public interface IWalletGateway
    {
        bool IsAvailable { get; }
        string AccountCaip10 { get; }
        Task ConnectAsync();
        Task<string> SignMessageAsync(string message);
        Task DisconnectAsync();
    }
}
