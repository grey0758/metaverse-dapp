using System;
using System.Threading.Tasks;

namespace MetaverseGame.Wallet
{
    public sealed class DisabledWalletGateway : IWalletGateway
    {
        public bool IsAvailable => false;
        public string AccountCaip10 => string.Empty;

        public Task ConnectAsync() =>
            Task.FromException(new InvalidOperationException(
                "Wallet support is optional and is not configured in this build."));

        public Task<string> SignMessageAsync(string message) =>
            Task.FromException<string>(new InvalidOperationException(
                "Wallet support is optional and is not configured in this build."));

        public Task DisconnectAsync() => Task.CompletedTask;
    }
}
