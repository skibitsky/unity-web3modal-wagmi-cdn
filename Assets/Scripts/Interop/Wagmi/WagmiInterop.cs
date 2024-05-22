using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace WalletConnect.Web3Modal.WebGl.Wagmi
{
    public class WagmiInterop : InteropService
    {
        [DllImport("__Internal")]
        private static extern void WagmiCall(int id, string methodName, string payload, ExternalMethodCallback callback);

        public WagmiInterop() : base(WagmiCall)
        {
        }

        public Task<string> SignMessageAsync(SignMessageParameter parameter)
        {
            return InteropCallAsync<SignMessageParameter, string>(WagmiMethods.SignMessage, parameter);
        }

        public Task<GetAccountReturnType> GetAccountAsync()
        {
            return InteropCallAsync<object, GetAccountReturnType>(WagmiMethods.GetAccount, null);
        }
    }
}