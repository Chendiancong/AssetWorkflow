using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace cdc.AssetWorkflow
{
    public class AssetMgrHelper
    {
        public Dictionary<string, string> versions = new Dictionary<string, string>();
        public Dictionary<string, string> localPathRecord = new Dictionary<string, string>();
        public AssetMgrConfig config;
        public HotUpdateProfiler hotUpdateProfiler = new HotUpdateProfiler
        {
            state = HotUpdateState.Initial
        };

        public string AssetSavePath
        {
            get
            {
                #if UNITY_EDITOR
                return Path.Combine(Application.dataPath, "..", "LocalAssets");
                #else
                return Application.persistentDataPath;
                #endif
            }
        }

        private Regex m_fileExtension = new Regex(@"\.[^\.]+$");

        public void LoadSetting()
        {
            string settingPath = GetLocalLoadPath("Setting.json");
            string jsonString;
            using (StreamReader reader = new StreamReader(settingPath))
            {
                jsonString = reader.ReadToEnd();
            }
            AssetMgrConfig setting = JsonUtility.FromJson<AssetMgrConfig>(jsonString);
            config = setting;
        }


        public void PrepareFileMap()
        {

        }

        public async ValueTask HotUpdate()
        {
            HotUpdateProfiler profiler = hotUpdateProfiler;
            profiler.ResetAndSetState(HotUpdateState.LoadLocalVersion);
            Dictionary<string, string> oldVersions = new Dictionary<string, string>();
            Dictionary<string, string> newVersions = new Dictionary<string, string>();
            LoadFileToVersion(oldVersions);
            await Task.Delay(100);
            if (!config.enablePatch)
            {
                profiler.SetState(HotUpdateState.Success);
                return;
            }

            string reqUrl = config.serverUrl.TrimEnd('/', '\\');
            var differentBundles = new List<(string name, string version)>();
            {
                // Update Version.txt
                profiler.ResetAndSetState(HotUpdateState.UpdateVersion);
                ConvertBytesToVersion(
                    newVersions,
                    await Network.DownloadToMemoryAsync($"{reqUrl}/Version.txt")
                );

                // Compare Version
                foreach (var kv in versions)
                {
                    if (!oldVersions.ContainsKey(kv.Key) || !kv.Value.StrEquals(oldVersions[kv.Key]))
                        differentBundles.Add((name: kv.Key, version: kv.Value));
                }
            }

            {
                profiler.ResetAndSetState(HotUpdateState.UpdateAsset);
                if (differentBundles.Count > 0)
                {
                    var downloadedVersions = new Dictionary<string, string>();
                    LoadFileToVersion(downloadedVersions, "Version_downloaded.txt");
                    if (!Directory.Exists(AssetSavePath))
                        Directory.CreateDirectory(AssetSavePath);
                    foreach ((string name, string version) bundle in differentBundles)
                    {
                        if (!downloadedVersions.ContainsKey(bundle.name))
                        {
                            downloadedVersions[bundle.name] = bundle.version;
                            string bundleName = ConvertBundleName(bundle.name);
                            string filePath = GetLocalSavePath(bundleName);
                            await DownloadToFileAsync($"{reqUrl}/{bundleName}", filePath);
                            localPathRecord[bundle.name] = filePath;
                        }
                    }
                }
            }

            {
                profiler.ResetAndSetState(HotUpdateState.LoadFileMap);
            }

        }

        private void LoadFileToVersion(Dictionary<string, string> outVersions, string fileName = "Version.txt")
        {
            string versionPath = GetLocalLoadPath(fileName);
            FileSystem.ReadFileLineByLine(
                versionPath,
                line => {
                    line = line.Trim();
                    if (string.IsNullOrEmpty(line))
                        return;
                    string[] sections = line.Split(':');
                    if (sections.Length != 2)
                        return;
                    outVersions[sections[0]] = sections[1];
                }
            );
        }

        private void ConvertBytesToVersion(Dictionary<string, string> outVersions, byte[] bytes)
        {
            string content = Encoding.UTF8.GetString(bytes);
            Debug.Log($"version content:{content}");
            foreach (string oneLine in content.Split('\n'))
            {
                string line = oneLine.Trim();
                if (string.IsNullOrEmpty(line))
                    continue;
                string[] sections = line.Split(':');
                if (sections.Length != 2)
                    return;
                outVersions[sections[0]] = sections[1];
            }
        }

        /// <summary>
        /// 获取动态资源的本地加载目录
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <returns></returns>
        public string GetLocalLoadPath(string fileName)
        {
            string localPath;
            if (localPathRecord.TryGetValue(fileName, out localPath))
                return localPath;
            InternalGetLocalLoadPath(fileName, ref localPath);
            localPathRecord[fileName] = localPath;
            return localPath;
        }

        private void InternalGetLocalLoadPath(string fileName, ref string localPath)
        {
            localPath = Path.Combine(
                AssetSavePath,
                fileName
            );
            if (File.Exists(localPath))
                return;
            localPath = Path.Combine(
                Application.streamingAssetsPath,
                fileName
            );
        }

        /// <summary>
        /// 获取动态资源的本地储存目录
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <returns></returns>
        public string GetLocalSavePath(string fileName)
        {
            fileName = Path.Combine(
                AssetSavePath,
                fileName
            );
            return fileName;
        }

        private Dictionary<string, TaskCompletionSource<bool>> m_pendingReqs = new Dictionary<string, TaskCompletionSource<bool>>();

        private void OnWebRequestComplete(AsyncOperation operation)
        {
            UnityWebRequest request = (operation as UnityWebRequestAsyncOperation).webRequest;
            TaskCompletionSource<bool> tsc;
            if (m_pendingReqs.TryGetValue(request.url, out tsc))
                m_pendingReqs.Remove(request.url);
            if (request.result == UnityWebRequest.Result.Success)
                tsc?.SetResult(true);
            else
                tsc?.SetResult(false);
        }

        public async ValueTask DownloadAsync(string url, DownloadHandler downloadHandler = null)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.downloadHandler = downloadHandler;
                TaskCompletionSource<bool> tsc = new TaskCompletionSource<bool>();
                m_pendingReqs[url] = tsc;
                UnityWebRequestAsyncOperation operation = request.SendWebRequest();
                operation.completed += OnWebRequestComplete;
                bool isOk = await tsc.Task;
                if (!isOk)
                    throw new Exception($"Download {url} failed!");
            }
        }

        public ValueTask DownloadToFileAsync(string url, string savePath)
        {
            return DownloadAsync(url, new DownloadHandlerFile(savePath));
        }

        private string ConvertBundleName(string originName)
        {
            return $"{originName.Substring(0, 2)}/{originName}";
        }

        public struct DownloadPending
        {
            public TaskCompletionSource<bool> tsc;
            public UnityWebRequest request;

            public double CalculatedTotalByte
            {
                get
                {
                    if (request == null)
                        return 0f;
                    float progress = request.downloadProgress;
                    float downloaded = request.downloadedBytes;
                    if (progress.AlmostEquals(0f))
                        return 0f;
                    return ((double)progress)/((double)downloaded);
                }
            }
        }

        public struct HotUpdateProfiler
        {
            public HotUpdateState state;
            public float downloadProgress;
            public ulong downloadedBytes;
            public float downloadSpeed;

            public void Reset()
            {
                downloadProgress = 0f;
                downloadedBytes = 0;
                downloadSpeed = 0f;
            }

            public void SetState(HotUpdateState state) => this.state = state;

            public void ResetAndSetState(HotUpdateState state)
            {
                Reset();
                SetState(state);
            }
        }

        public enum HotUpdateState
        {
            Initial,
            LoadLocalVersion,
            LoadLocalVersionFail,
            UpdateVersion,
            UpdateVersionFail,
            UpdateAsset,
            UpdateAssetFail,
            LoadFileMap,
            LoadFileMapFail,
            Success,
        }
    }
}