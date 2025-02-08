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
                // Update Version
                profiler.ResetAndSetState(HotUpdateState.UpdateVersion);
                ConvertBytesToVersion(
                    curVersions,
                    await Network.DownloadToMemoryAsync($"{reqUrl}/Version")
                );

                // Compare Version
                foreach (var kv in curVersions)
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
                    string tempVerFileName = "Version_downloaded";
                    // 保证从储存目录加载临时版本文件
                    localPathRecord[tempVerFileName] = GetLocalSavePath(tempVerFileName);
                    LoadFileToVersion(downloadedVersions, tempVerFileName);
                    if (!Directory.Exists(AssetSavePath))
                        Directory.CreateDirectory(AssetSavePath);
                    using (StreamWriter writer = new StreamWriter(GetLocalSavePath(tempVerFileName), true))
                    {
                        foreach ((string name, string version) bundle in differentBundles)
                        {
                            if (!downloadedVersions.ContainsKey(bundle.name))
                            {
                                string bundleName = ConvertToPathIfBundleName(bundle.name);
                                string filePath = GetLocalSavePath(bundleName);
                                string url = $"{reqUrl}/{bundleName}";
                                Debug.Log($"start download {url}");
                                await Network.DownloadToFileAsync(url, filePath);
                                downloadedVersions[bundle.name] = bundle.version;
                                localPathRecord[bundle.name] = filePath;
                                // 持续记录已经下载的文件，支持端点续传
                                writer.WriteLine($"{bundle.name}:{bundle.version}");
                            }
                        }
                    }

                    // 完全更新之后删除的版本文件，并生成正式的文件
                    File.Delete(GetLocalSavePath(tempVerFileName));
                    SaveVersionToFile(curVersions);
                }
            }

            {
                profiler.ResetAndSetState(HotUpdateState.LoadFileMap);
            }

            profiler.ResetAndSetState(HotUpdateState.Success);
            Debug.Log("HotUpdate finished");
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

        private Regex m_bundleNameSuffix = new Regex(@"\.ab$", RegexOptions.IgnoreCase);
        private string ConvertToPathIfBundleName(string originName)
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