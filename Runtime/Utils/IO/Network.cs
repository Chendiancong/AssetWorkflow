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

        public static async ValueTask DownloadAsyncKeepingCheck<T>(string url, T downloadHandler = default, Action<T> onSection = default, Action<T> onComplete = default)
            where T : DownloadHandler
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.downloadHandler = downloadHandler;
                var pending = Pending.Create();
                m_pendings[request.GetHashCode()] = pending;
                UnityWebRequestAsyncOperation operation = request.SendWebRequest();
                operation.completed += OnRequestComplete;
                do
                {
                    onSection?.Invoke(request.downloadHandler as T);
                    if (!request.isDone)
                        await Task.Delay(500);
                } while (false);

                bool isOk = await pending.tsc.Task;
                if (!isOk)
                    throw new Exception($"Download {url} failed: {request.error}!");
                onComplete?.Invoke(request.downloadHandler as T);
            }
        }

        public static ValueTask DownloadToFileAsync(string url, string savePath)
        {
            return DownloadAsync(url, new DownloadHandlerFile(savePath));
        }

        // public static async ValueTask DownloadToFileAsync2(string url, string savePath)
        // {
        //     using (UnityWebRequest request = UnityWebRequest.Get(url))
        //     {
        //         var downloadHandler = new DownloadHandlerFile(savePath);
        //         request.downloadHandler = downloadHandler;

        //         var pending = Pending.Create();
        //         m_pendings[request.GetHashCode()] = pending;

        //         // Variables for tracking progress
        //         long totalBytes = 0;
        //         long downloadedBytes = 0;
        //         DateTime startTime = DateTime.Now;
        //         DateTime lastUpdateTime = DateTime.Now;

        //         // Setup progress tracking
        //         request.SendWebRequest();
        //         while (!request.isDone)
        //         {
        //             if (totalBytes == 0 && request.downloadedBytes > 0)
        //             {
        //                 totalBytes = request.downloadedBytes;
        //             }

        //             downloadedBytes = request.downloadedBytes;

        //             // Calculate download speed (bytes per second)
        //             TimeSpan elapsed = DateTime.Now - startTime;
        //             double downloadSpeed = elapsed.TotalSeconds > 0 ? downloadedBytes / elapsed.TotalSeconds : 0;

        //             // Update stats every second
        //             if ((DateTime.Now - lastUpdateTime).TotalSeconds >= 1)
        //             {
        //                 lastUpdateTime = DateTime.Now;
        //                 Debug.Log($"Download progress: {downloadedBytes}/{totalBytes} bytes " +
        //                         $"({(totalBytes > 0 ? (float)downloadedBytes/totalBytes*100 : 0):F1}%) " +
        //                         $"Speed: {FormatBytes(downloadSpeed)}/s");
        //             }

        //             await Task.Yield();
        //         }

        //         // Final update
        //         TimeSpan totalElapsed = DateTime.Now - startTime;
        //         double averageSpeed = totalElapsed.TotalSeconds > 0 ? downloadedBytes / totalElapsed.TotalSeconds : 0;
        //         Debug.Log($"Download completed: {downloadedBytes} bytes in {totalElapsed.TotalSeconds:F2}s " +
        //                 $"(Average speed: {FormatBytes(averageSpeed)}/s)");

        //         if (request.result != UnityWebRequest.Result.Success)
        //         {
        //             throw new Exception($"Download {url} failed: {request.error}");
        //         }
        //     }
        // }

        // private static string FormatBytes(double bytes)
        // {
        //     string[] suffixes = { "B", "KB", "MB", "GB" };
        //     int suffixIndex = 0;
        //     while (bytes >= 1024 && suffixIndex < suffixes.Length - 1)
        //     {
        //         bytes /= 1024;
        //         suffixIndex++;
        //     }
        //     return $"{bytes:F2} {suffixes[suffixIndex]}";
        // }

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
