using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AOT;
using UnityEngine;

namespace WalletConnect.Web3Modal.WebGl
{
    public class InteropService
    {
        private static readonly Dictionary<int, PendingInteropCall> PendingInteropCalls = new();

        private readonly ExternalMethod _externalMethod;

        public InteropService(ExternalMethod externalMethod)
        {
            _externalMethod = externalMethod;
        }

        public async Task<TRes> InteropCallAsync<TReq, TRes>(string methodName, TReq requestParameter, CancellationToken cancellationToken = default)
        {
            // TODO: implement cancellation

            var tcs = new TaskCompletionSource<object>();

            var id = Guid.NewGuid().GetHashCode();

            var pendingInteropCall = new PendingInteropCall(typeof(TRes), tcs);
            PendingInteropCalls.Add(id, pendingInteropCall);

            string paramStr = null;
            if (requestParameter != null)
            {
                paramStr = JsonUtility.ToJson(requestParameter);
            }

            _externalMethod(id, methodName, paramStr, TcsCallback);

            var result = await tcs.Task;
            return (TRes)result;
        }

        [MonoPInvokeCallback(typeof(ExternalMethodCallback))]
        public static void TcsCallback(int id, string responseData, string responseError = null)
        {
            if (!PendingInteropCalls.TryGetValue(id, out var pendingCall))
            {
                Debug.LogError("No pending call found for id: " + id);
                return;
            }

            if (!string.IsNullOrEmpty(responseError))
            {
                try
                {
                    var error = JsonUtility.FromJson<InteropCallError>(responseError);
                    if (error != null)
                    {
                        pendingCall.TaskCompletionSource.SetException(new InteropException(error.message));
                        PendingInteropCalls.Remove(id);
                        return;
                    }
                }
                catch (Exception)
                {
                    pendingCall.TaskCompletionSource.SetException(new FormatException($"Unable to parse error response: {responseError}"));
                    PendingInteropCalls.Remove(id);
                    return;
                }
            }

            object res = null;
            if (pendingCall.ResType == typeof(string))
            {
                res = responseData;
            }
            else if (pendingCall.ResType == typeof(int) && int.TryParse(responseData, out var intResult))
            {
                res = intResult;
            }
            else if (pendingCall.ResType == typeof(float) && float.TryParse(responseData, out var floatResult))
            {
                res = floatResult;
            }
            else if (pendingCall.ResType == typeof(double) && double.TryParse(responseData, out var doubleResult))
            {
                res = doubleResult;
            }
            else if (pendingCall.ResType == typeof(bool) && bool.TryParse(responseData, out var boolResult))
            {
                res = boolResult;
            }
            else if (pendingCall.ResType == typeof(char) && char.TryParse(responseData, out var charResult))
            {
                res = charResult;
            }
            else if (pendingCall.ResType != typeof(void))
            {
                try
                {
                    res = JsonUtility.FromJson(responseData, pendingCall.ResType);
                }
                catch (Exception e)
                {
                    pendingCall.TaskCompletionSource.SetException(e);
                    PendingInteropCalls.Remove(id);
                    return;
                }
            }

            pendingCall.TaskCompletionSource.SetResult(res);
            PendingInteropCalls.Remove(id);
        }

        public delegate void ExternalMethod(int id, string methodName, string parameter, ExternalMethodCallback callback);

        public delegate void ExternalMethodCallback(int id, string responseData, string responseError = null);

        private readonly struct PendingInteropCall
        {
            public readonly Type ResType;
            public readonly TaskCompletionSource<object> TaskCompletionSource;

            public PendingInteropCall(Type resType, TaskCompletionSource<object> taskCompletionSource)
            {
                ResType = resType;
                TaskCompletionSource = taskCompletionSource;
            }
        }
    }
}