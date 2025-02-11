using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace cdc.AssetWorkflow
{
    public class AssetMgrHelper
    {
        public Dictionary<string, string> curVersions = new Dictionary<string, string>();
        public Dictionary<string, string> localPathRecord = new Dictionary<string, string>();
        public Dictionary<string, string> assetMap = new Dictionary<string, string>();
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
                return Path.Combine(Application.dataPath, "..", "Library", "RemoteAssets");
                #elif UNITY_STANDALONE
                return Path.Combine(Application.dataPath, "RemoteAssets");
                #else
                return Application.persistentDataPath;
                #endif
            }
        }

        public void LoadSetting()
        {
            string settingPath = GetLocalLoadPath("Setting.json");
            string jsonString;
            Debugger.Log($"load setting from {settingPath}");
            using (StreamReader reader = new StreamReader(settingPath))
            {
                jsonString = reader.ReadToEnd();
            }
            AssetMgrConfig setting = JsonUtility.FromJson<AssetMgrConfig>(jsonString);
            config = setting;
            Debugger.Log($"ok => {ObjectDumper.Dump(setting)}");
        }

        public async ValueTask HotUpdate()
        {
            Debugger.Log("start hot update");
            HotUpdateProfiler profiler = hotUpdateProfiler;
            profiler.ResetAndSetState(HotUpdateState.LoadLocalVersion);
            Dictionary<string, string> oldVersions = new Dictionary<string, string>();
            LoadFileToVersion(oldVersions);
            await Task.Delay(100);
            if (!config.enablePatch)
            {
                profiler.SetState(HotUpdateState.Success);
                Debugger.Log("hot update finished");
                return;
            }

            string reqUrl = config.serverUrl.TrimEnd('/', '\\');
            string platformDir = PlatformMapping.GetAssetPlatform(Application.platform).ToString();
            var differentAssets = new List<(string name, string version)>();
            {
                // Update Version
                profiler.ResetAndSetState(HotUpdateState.UpdateVersion);
                Debugger.Log($"download ${reqUrl}/{platformDir}/Version");
                ConvertBytesToVersion(
                    curVersions,
                    await Network.DownloadToMemoryAsync($"{reqUrl}/{platformDir}/Version")
                );

                // Compare Version
                foreach (var kv in curVersions)
                {
                    if (!oldVersions.ContainsKey(kv.Key) || !kv.Value.StrEquals(oldVersions[kv.Key]))
                        differentAssets.Add((name: kv.Key, version: kv.Value));
                }
            }

            {
                profiler.ResetAndSetState(HotUpdateState.UpdateAsset);
                if (differentAssets.Count > 0)
                {
                    var downloadedVersions = new Dictionary<string, string>();
                    string tempVerFileName = "Version_downloaded";
                    // 保证从储存目录加载临时版本文件
                    localPathRecord[tempVerFileName] = GetLocalSavePath(tempVerFileName);
                    LoadFileToVersion(downloadedVersions, tempVerFileName);
                    if (!Directory.Exists(AssetSavePath))
                        Directory.CreateDirectory(AssetSavePath);
                    using (StreamWriter writer = new StreamWriter(GetLocalSavePath(tempVerFileName), true))
                    {
                        foreach ((string name, string version) assetInfo in differentAssets)
                        {
                            string assetName = ConvertIfBundleName(assetInfo.name);
                            if (!downloadedVersions.ContainsKey(assetName))
                            {
                                string filePath = GetLocalSavePath(assetName);
                                string url = $"{reqUrl}/{platformDir}/{assetName}";
                                Debugger.Log($"download {url}");
                                await Network.DownloadToFileAsync(url, filePath);
                                downloadedVersions[assetInfo.name] = assetInfo.version;
                                localPathRecord[assetName] = filePath;
                                // 持续记录已经下载的文件，支持端点续传
                                writer.WriteLine($"{assetInfo.name}:{assetInfo.version}");
                            }
                        }
                    }

                    // 完全更新之后删除的版本文件，并生成正式的文件
                    File.Delete(GetLocalSavePath(tempVerFileName));
                    SaveVersionToFile(curVersions);
                }
            }

            {
                // reload Setting.json
                LoadSetting();
            }

            profiler.ResetAndSetState(HotUpdateState.Success);
            Debugger.Log("hot update finished");
        }

        private async void LoadFileToVersion(Dictionary<string, string> outVersions, string fileName = "Version")
        {
            string versionPath = GetLocalLoadPath(fileName);
            await FileSystem.ReadFileLineByLine(
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

        private void SaveVersionToFile(Dictionary<string, string> versions, string fileName = "Version")
        {
            string versionPath = GetLocalSavePath(fileName);
            using (StreamWriter writer = new StreamWriter(versionPath))
            {
                foreach (var kv in versions)
                    writer.WriteLine($"{kv.Key}:{kv.Value}");
            }
        }

        private void ConvertBytesToVersion(Dictionary<string, string> outVersions, byte[] bytes)
        {
            string content = Encoding.UTF8.GetString(bytes);
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

        public async ValueTask PrepareAssetMap()
        {
            string filePath = GetLocalLoadPath("AssetMap");
            await FileSystem.ReadFileLineByLine(
                filePath,
                line =>
                {
                    if (string.IsNullOrEmpty(line))
                        return;
                    string[] sections = line.Split(':');
                    if (sections.Length != 2 || string.IsNullOrEmpty(sections[0]) || string.IsNullOrEmpty(sections[1]))
                        return;
                    assetMap[sections[0]] = sections[1];
                }
            );
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

        /// <summary>
        /// 将资源名转换为对应assetbundle名称
        /// </summary>
        /// <returns>是否找到合适的assetbundle</returns>
        public bool AssetNameToBundleName(string assetName, out string bundleName)
        {
            return assetMap.TryGetValue(assetName, out bundleName);
        }

        private Regex m_bundleNameSuffix = new Regex(@"\.ab$", RegexOptions.IgnoreCase);
        /// <summary>
        /// 转换为目录形式的AssetBundle名称
        /// </summary>
        /// <param name="originName"></param>
        /// <returns></returns>
        public string ConvertIfBundleName(string originName)
        {
            if (m_bundleNameSuffix.IsMatch(originName))
                return $"{originName.Substring(0, 2)}/{originName}";
            else
                return originName;
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

            public void SetState(HotUpdateState state)
            {
                this.state = state;
                Debugger.Log($"hot update {state}");
            }

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
            Success,
        }
    }
}