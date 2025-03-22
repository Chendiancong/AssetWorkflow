using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace cdc.AssetWorkflow.Editor
{
    public static class Builder
    {
        public static Helper helper = new Helper();

        public static void BuildBundle(BuilderCommand cmd = default)
        {
            BuilderProcessorAttribute.ExecutePhase(BuilderProcessorPhase.PreBuild);
            InternalBuild(ref cmd);
            BuilderProcessorAttribute.ExecutePhase(BuilderProcessorPhase.PostBuild);
        }

        public static void Clean(BuilderCommand option = default)
        {
            BuilderProcessorAttribute.ExecutePhase(BuilderProcessorPhase.PreClean);
            string targetPath = EditorFileSystem.GetOutputPath(option.GetTrulyBuildTarget());
            Debug.Log($"clean target path is {targetPath}");
            EditorFileSystem.CleanDirectory(targetPath);
            AssetDatabase.Refresh();
            BuilderProcessorAttribute.ExecutePhase(BuilderProcessorPhase.PostClean);
        }

        public static void GenerateAssetFileMap(BuilderCommand command = default)
        {
            var list = new List<AssetBundleBuild>();
            helper.CollectAssets(EditorFileSystem.BundleRootPath, list);
            helper.GenerateAssetFileMap(list, ref command);
            helper.GenerateAssetFileVersions(ref command);
            helper.GenerateAssetExtraInfoFile(ref command);
            AssetDatabase.Refresh();
        }

        public static void GenerateAssetFileVersions(BuilderCommand command = default)
        {
            helper.GenerateAssetFileVersions(ref command);
            helper.GenerateAssetExtraInfoFile(ref command);
            AssetDatabase.Refresh();
        }

        public static void GenerateSettingFile(BuilderCommand command = default)
        {
            helper.GenerateSettingFile(ref command);
            helper.GenerateAssetFileVersions(ref command);
            helper.GenerateAssetExtraInfoFile(ref command);
            AssetDatabase.Refresh();
        }

        private static void Test()
        {
            // string input = Path.Combine(Application.dataPath, UniPath.BundleRootPath);
            // Debug.Log(UniPath.LocalPathToDataPath(input));
            // Debug.Log(UniPath.LocalPathToAssetBundleName(input));
            // string input = Path.Combine(Application.streamingAssetsPath, "StreamingAssets");
            // var asset = AssetDatabase.LoadAssetAtPath<AssetBundleManifest>("Assets/StreamingAssets/StreamingAssets");
            // Debug.Log(asset);
            // string path = Path.Combine(helper.OutputPath, Path.GetFileName(helper.OutputPath));
            // AssetBundle bundle = AssetBundle.LoadFromFile(path);
            // if (bundle != null)
            // {
            //     var asset = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            //     Debug.Log(asset);
            // }
            // bundle.Unload(true);
        }

        private static void InternalBuild(ref BuilderCommand command)
        {
            BuildTarget platform = command.GetTrulyBuildTarget();
            var list = new List<AssetBundleBuild>();
            helper.CollectAssets(EditorFileSystem.BundleRootPath, list);
            var targetPath = EditorFileSystem.GetOutputPath(command.GetTrulyBuildTarget());
            if (!Directory.Exists(targetPath))
                Directory.CreateDirectory(targetPath);
            var buildOption = BuildAssetBundleOptions.None;

            BuildSettingAsset.Instance.ConvertCompressType(ref buildOption);
            buildOption |= command.options;

            Debug.Log($"Distribute path is {targetPath}");
            try
            {
                BuildPipeline.BuildAssetBundles(
                    targetPath,
                    list.ToArray(),
                    buildOption,
                    platform
                );

                helper.GenerateAssetFileMap(list, ref command);
                helper.GenerateSettingFile(ref command);
                helper.GenerateAssetFileVersions(ref command);
                helper.GenerateAssetExtraInfoFile(ref command);

                AssetDatabase.Refresh();
            }
            catch { throw; }
        }

        public class Helper
        {
            public ReadOnlyCollection<AssetBundleBuild> AllBuilds { get; private set; }

            public void CollectAssets(string rootPath, List<AssetBundleBuild> list)
            {
                var assetDic = new Dictionary<string, List<string>>();
                var folderConfigs = new Dictionary<string, IFolderConfigure>();

                Func<string, IFolderConfigure> GetConfig = localPath =>
                {
                    string dataPath = EditorFileSystem.LocalPathToDataPath(localPath);
                    IFolderConfigure config;
                    if (!folderConfigs.TryGetValue(dataPath, out config))
                    {
                        string configPath = $"{dataPath}/{Path.GetFileName(dataPath)}{AssetFolderConfigure.FileNameSuffix}";
                        var asset = AssetDatabase.LoadAssetAtPath<AssetFolderConfigureAsset>(configPath);
                        if (asset == null)
                            folderConfigs[dataPath] = AssetFolderConfigureAsset.defaultConfig;
                        else
                            folderConfigs[dataPath] = asset;
                        config = folderConfigs[dataPath];
                    }
                    return config;
                };

                list.Clear();
                EditorFileSystem.WalkDataPath(
                    rootPath,
                    (dInfo, fInfo) =>
                    {
                        if (fInfo.Name.EndsWith(".meta"))
                            return;
                        if (fInfo.Name.EndsWith(AssetFolderConfigure.FileNameSuffix))
                            return;
                        List<string> assetNames;
                        var folderConfig = GetConfig(dInfo.FullName);
                        if (folderConfig.PackingLevel == AssetBundlePackingLevel.No)
                            return;
                        else
                        {
                            string key = null;
                            switch (folderConfig.PackingLevel)
                            {
                                case AssetBundlePackingLevel.Normal:
                                    key = dInfo.FullName;
                                    break;
                                case AssetBundlePackingLevel.Single:
                                    key = fInfo.FullName;
                                    break;
                            }
                            if (!assetDic.TryGetValue(key, out assetNames))
                            {
                                assetNames = new List<string>();
                                assetDic[key] = assetNames;
                            }
                            assetNames.Add(fInfo.FullName);
                        }
                    }
                );

                foreach (KeyValuePair<string, List<string>> kv in assetDic)
                {
                    var build = new AssetBundleBuild();
                    string bundleName = EditorFileSystem.LocalPathToAssetBundleName(kv.Key);
                    string origin = bundleName;
                    Crypto.FromStringToMD5(ref bundleName);
                    build.assetBundleName = bundleName.Substring(0, 2) + "/" + bundleName + ".ab";
                    build.assetNames = kv.Value
                        .ConvertAll(v => EditorFileSystem.LocalPathToDataPath(v))
                        .ToArray();
                    list.Add(build);
                }

                Debug.Log("Collect assets ok.");
            }

            public void GenerateAssetFileMap(List<AssetBundleBuild> list, ref BuilderCommand command)
            {
                var dic = new Dictionary<string, string>();
                foreach (AssetBundleBuild build in list)
                {
                    foreach (string assetName in build.assetNames)
                        dic[assetName] = build.assetBundleName;
                }

                {
                    // manifest file
                    string targetPath = EditorFileSystem.GetOutputPath(command.GetTrulyBuildTarget());
                    string manifestName = Path.GetFileName(targetPath);
                    dic[manifestName] = manifestName;
                }

                var sb = new StringBuilder();
                foreach (KeyValuePair<string, string> kv in dic)
                {
                    sb.AppendLine($"{kv.Key}:{kv.Value}");
                }

                string filePath = Path.Combine(
                    EditorFileSystem.GetOutputPath(command.GetTrulyBuildTarget()),
                    "AssetMap"
                );
                using (var writer = new StreamWriter(filePath))
                {
                    writer.Write(sb.ToString());
                }

                Debug.Log("Mapping file generated successfully.");
            }

            private Regex m_ignoreEnd = new Regex(@"\.(meta|manifest)$", RegexOptions.IgnoreCase);
            public void GenerateAssetFileVersions(ref BuilderCommand command)
            {
                string filePath = Path.Combine(
                    EditorFileSystem.GetOutputPath(command.GetTrulyBuildTarget()),
                    "Version"
                );
                if (File.Exists(filePath))
                    File.Delete(filePath);
                var sb = new StringBuilder();
                EditorFileSystem.WalkDirectory(
                    EditorFileSystem.GetOutputPath(command.GetTrulyBuildTarget()),
                    (di, fi) => {
                        // 不需要计算meta文件和manifest文件
                        if (m_ignoreEnd.IsMatch(fi.Name))
                            return;
                        string md5Hash = null;
                        Crypto.FromFileToMD5(ref md5Hash, fi.FullName);
                        sb.Append($"{fi.Name}:{md5Hash}\n");
                    }
                );

                using (var writer = new StreamWriter(filePath))
                {
                    writer.Write(sb.ToString());
                }
                Debug.Log("Version file generated successfully.");
            }

            public void GenerateAssetExtraInfoFile(ref BuilderCommand command)
            {
                var filePath = Path.Combine(
                    EditorFileSystem.GetOutputPath(command.GetTrulyBuildTarget()),
                    "ExtraInfo"
                );
                if (File.Exists(filePath))
                    File.Delete(filePath);
                var sb = new StringBuilder();
                EditorFileSystem.WalkDirectory(
                    EditorFileSystem.GetOutputPath(command.GetTrulyBuildTarget()),
                    (_, fi) => {
                        // 不需要计算meta文件和manifest文件
                        if (m_ignoreEnd.IsMatch(fi.FullName))
                            return;
                        var extraInfo = new FileExtraInfo()
                        {
                            fileName = fi.Name,
                            size = fi.Length
                        };
                        sb.AppendLine(extraInfo.ToStorageStr());
                    }
                );
                using (var writer = new StreamWriter(filePath))
                {
                    writer.Write(sb.ToString());
                }
                Debug.Log("Extra info file generated successfully!");
            }

            public void GenerateSettingFile(ref BuilderCommand command)
            {
                string settingName = "Setting.json";
                AssetMgrConfig config = BuildSettingAsset.Instance.CreateConfig(ref command);
                config.settingName = settingName;
                string jsonString = JsonConvert.SerializeObject(config, Formatting.Indented);
                Action<string> SaveSetting = (string filePath) =>
                {
                    string dirName = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(dirName))
                        Directory.CreateDirectory(dirName);
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                    using (var writer = new StreamWriter(filePath))
                    {
                        writer.Write(jsonString);
                    }
                };

                string filePath = Path.Combine(
                    EditorFileSystem.GetOutputPath(command.GetTrulyBuildTarget()),
                    settingName
                );
                SaveSetting(filePath);
                string backupPath = Path.Combine(
                    Application.streamingAssetsPath,
                    settingName
                );
                if (!filePath.StrEquals(backupPath))
                    SaveSetting(backupPath);
                Debug.Log("Setting file generate successfully!");
            }
        }
    }
}