using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace cdc.AssetWorkflow
{
    internal static class Network
    {
        private static Dictionary<int, Pending> m_pendings = new Dictionary<int, Pending>();

        public static async ValueTask DownloadAsync<T>(string url, T downloadHandler = default, Action<T> onComplete = default)
            where T : DownloadHandler
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.downloadHandler = downloadHandler;
                var pending = Pending.Create();
                m_pendings[request.GetHashCode()] = pending;
                UnityWebRequestAsyncOperation operation = request.SendWebRequest();
                operation.completed += OnRequestComplete;
                bool isOk = await pending.tsc.Task;
                if (!isOk)
                    throw new Exception($"Download {url} failed!");
                onComplete?.Invoke(request.downloadHandler as T);
            }
        }

        public static ValueTask DownloadToFileAsync(string url, string savePath)
        {
            return DownloadAsync(url, new DownloadHandlerFile(savePath));
        }

        public static async ValueTask<byte[]> DownloadToMemoryAsync(string url)
        {
            byte[] result = default;
            await DownloadAsync(
                url, new DownloadHandlerBuffer(),
                handler => result = handler.data
            );
            return result;
        }

        private static void OnRequestComplete(AsyncOperation operation)
        {
            UnityWebRequest request = (operation as UnityWebRequestAsyncOperation).webRequest;
            int key = request.GetHashCode();
            Pending pending;
            if (m_pendings.TryGetValue(key, out pending))
                m_pendings.Remove(key);
            if (request.result == UnityWebRequest.Result.Success)
                pending.tsc?.SetResult(true);
            else
                pending.tsc?.SetResult(false);
        }

        private struct Pending
        {
            public TaskCompletionSource<bool> tsc;
            public UnityWebRequest request;

            public bool IsValid => tsc != null && request != null;

            public static Pending Create()
            {
                return new Pending
                {
                    tsc = new TaskCompletionSource<bool>(),
                };
            }
        }
    }
}