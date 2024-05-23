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
        var param = new SignMessageParameter
        {
            message = _messageInputField.text
        };

        var wagmi = new WagmiInterop();

        var result = await wagmi.SignMessageAsync(param);

        Debug.Log($"Result: {result}");
    }

    public async void OnGetAccountClicked()
    {
        var wagmi = new WagmiInterop();
        var account = await wagmi.GetAccountAsync();

        Debug.Log($"{account.chainId}:{account.address}");
    }

    public void OnSignMessage(string signature)
    {
        Debug.Log($"[Unity] OnSignMessage: {signature}");
    }
    
}