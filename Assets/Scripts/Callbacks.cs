using System;
using AOT;
using UnityEngine;

public class Callbacks
{
    [MonoPInvokeCallback(typeof(Action<string>))]
    public static void RandomName(string value)
    {
        Debug.Log("[Unity] SignMessageCallback2: " + value);
    }

}