using System;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WalletConnect.Web3Modal.WebGl;
using WalletConnect.Web3Modal.WebGl.Wagmi;

public class Main : MonoBehaviour
{
    [SerializeField] private TMP_InputField _messageInputField;
    [SerializeField] private Button _signMessageButton;
    
    [DllImport("__Internal")]
    private static extern void PreloadWeb3Modal(string projectId, string appName, string appLogoUrl);

    [DllImport("__Internal")]
    private static extern void OpenWeb3Modal();

    [DllImport("__Internal")]
    private static extern void WagmiCall(int id, string methodName, string payload, InteropService.ExternalMethodCallback callback);

    private void Awake()
    {
        _messageInputField.onValueChanged.AddListener(OnMessageValueChanged);
        _signMessageButton.interactable = false;
    }

    private void Start()
    {
        var projectId = "bd4997ce3ede37c95770ba10a3804dad";
        var appName = "Web3Modal";
        var appLogoUrl = "https://avatars.githubusercontent.com/u/37784886";

        PreloadWeb3Modal(projectId, appName, appLogoUrl);
    }

    private void OnMessageValueChanged(string value)
    {
        _signMessageButton.interactable = !string.IsNullOrEmpty(value);
    }

    public void OnConnectClicked()
    {
        Debug.Log("OnConnectClicked");
        
        OpenModal();
    }
    
    public void OpenModal()
    {
        OpenWeb3Modal();
    }

    [Serializable]
    private struct WrongType
    {
        public int number;
    }

    [Serializable]
    private struct SignMessagePayload
    {
        public string message;
    }

    public async void OnSignClicked()
    {
        var messageText = _messageInputField.text;
        var param = new SignMessageParameter
        {
            message = messageText
        };

        var wagmi = new WagmiInterop();

        var signature = await wagmi.SignMessageAsync(param);

        Debug.Log($"Signed. Verifying signature ({signature})");

        var account = await wagmi.GetAccountAsync();
        var isValid = await wagmi.VerifyMessageAsync(account.address, messageText, signature);

        Debug.Log($"The signature is valid: {isValid}");
    }

    public async void OnGetAccountClicked()
    {
        var wagmi = new WagmiInterop();
        var account = await wagmi.GetAccountAsync();

        Debug.Log($"{account.chainId}:{account.address}");
    }

    public async void OnSignTypedDataClicked()
    {
        var wagmi = new WagmiInterop();

        // In the real world, would have to create a typed data object
        // and then serialize it to JSON
        const string typedDataJson = @"{
          ""types"": {
            ""Person"": [
              { ""name"": ""name"", ""type"": ""string"" },
              { ""name"": ""wallet"", ""type"": ""address"" }
            ],
            ""Mail"": [
              { ""name"": ""from"", ""type"": ""Person"" },
              { ""name"": ""to"", ""type"": ""Person"" },
              { ""name"": ""contents"", ""type"": ""string"" }
            ]
          },
          ""primaryType"": ""Mail"",
          ""message"": {
            ""from"": {
              ""name"": ""Cow"",
              ""wallet"": ""0xCD2a3d9F938E13CD947Ec05AbC7FE734Df8DD826""
            },
            ""to"": {
              ""name"": ""Bob"",
              ""wallet"": ""0xbBbBBBBbbBBBbbbBbbBbbbbBBbBbbbbBbBbbBBbB""
            },
            ""contents"": ""Hello, Bob!""
          }
        }";

        var signature = await wagmi.SignTypedDataAsync(typedDataJson);

        Debug.Log($"Signed. Verifying signature ({signature})");

        var account = await wagmi.GetAccountAsync();
        var isValid = await wagmi.VerifyTypedDataAsync(typedDataJson, account.address, signature);

        Debug.Log($"The signature is valid: {isValid}");
    }

    public async void OnSwitchChainClicked()
    {
        var polygonChain = new AddEthereumChainParameter
        {
            chainId = "137",
            chainName = "Polygon",
            nativeCurrency = new NativeCurrency
            {
                name = "MATIC",
                symbol = "MATIC",
                decimals = 18
            },
            rpcUrls = new[]
            {
                "https://polygon-rpc.com",
                "https://rpc-mainnet.maticvigil.com",
                "https://rpc-mainnet.matic.network"
            },
            blockExplorerUrls = new[]
            {
                "https://polygonscan.com"
            }
        };

        var wagmi = new WagmiInterop();

        await wagmi.SwitchChainAsync(137, polygonChain);

        Debug.Log($"Chain switched to {polygonChain.chainName}");
    }

    public async void OnReadContractClicked()
    {
        const string contractAddress = "0xb47e3cd837ddf8e4c57f05d70ab865de6e193bbb";
        const string yugaLabsAddress = "0xA858DDc0445d8131daC4d1DE01f834ffcbA52Ef1";
        const string abi = Abi.CryptoPunks;

        var wagmi = new WagmiInterop();

        // Call 'name' function that doesn't take any arguments and returns a string
        var tokenName = await wagmi.ReadContractAsync(contractAddress, abi, "name");
        Debug.Log($"Token name: {tokenName}");

        // Call 'balanceOf' function that takes an address and returns an uint256
        var balance = await wagmi.ReadContractAsync(contractAddress, abi, "balanceOf", new[]
        {
            yugaLabsAddress
        });
        Debug.Log($"Yuga Labs owns: {balance} punks");
    }

    public async void OnSendEtherClicked()
    {
        const string vitalikAddress = "0xd8dA6BF26964aF9D7eEd9e03E53415D37aA96045";
        const string wei = "100000000000000000000"; // 100 ETH

        var wagmi = new WagmiInterop();
        var txHash = await wagmi.SendTransactionAsync(new SendTransactionParameter
        {
            to = vitalikAddress,
            value = wei
        });
        Debug.Log($"Success! Transaction hash: {txHash}");
    }
}