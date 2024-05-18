using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class Main : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void PreloadWeb3Modal(string projectId, string appName, string appLogoUrl);

    [DllImport("__Internal")]
    private static extern void OpenWeb3Modal();

    [DllImport("__Internal")]
    private static extern void ConnectWallet(string connectorName);

    private void Start()
    {
        string projectId = "bd4997ce3ede37c95770ba10a3804dad";
        string appName = "Web3Modal";
        string appLogoUrl = "https://avatars.githubusercontent.com/u/37784886";

        PreloadWeb3Modal(projectId, appName, appLogoUrl);
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
    
}