using System;

namespace WalletConnect.Web3Modal.WebGl.Wagmi
{
    [Serializable]
    public class SignMessageParameter
    {
        public string message;
    }

    [Serializable]
    public class GetAccountReturnType
    {
        public string address;
        public int chainId;
        public bool isConnecting;
        public bool isReconnecting;
        public bool isConnected;
        public bool isDisconnected;
        public string status;
    }
}