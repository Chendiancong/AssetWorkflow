using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace cdc.BundleWorkFlow.Editor
{
    public static class Builder
    {
        public static Helper helper = new Helper();

        [MenuItem("Bundle Workflow/Normal Build", priority = 200)]
        public static void NormalBuild()
        {
            BuilderProcessorAttribute.ExecutePhase(BuilderProcessorPhase.PreBuild);
            InternalBuild(false);
            BuilderProcessorAttribute.ExecutePhase(BuilderProcessorPhase.PostBuild);
        }

        [MenuItem("Bundle Workflow/Force Build", priority = 201)]
        public static void ForceBuild()
        {
            BuilderProcessorAttribute.ExecutePhase(BuilderProcessorPhase.PreBuild);
            InternalBuild(true);
            BuilderProcessorAttribute.ExecutePhase(BuilderProcessorPhase.PostBuild);
        }

        [MenuItem("Bundle Workflow/Clean", priority = 202)]
        public static void Clean()
        {
            BuilderProcessorAttribute.ExecutePhase(BuilderProcessorPhase.PreClean);
            string targetPath = helper.OutputPath;
            if (Directory.Exists(targetPath))
            {
                Directory.Delete(targetPath, true);
                string metaFile = $"{targetPath}.meta";
                if (File.Exists(metaFile))
                    File.Delete(metaFile);
                AssetDatabase.Refresh();
            }
            BuilderProcessorAttribute.ExecutePhase(BuilderProcessorPhase.PostClean);
        }

        [MenuItem("Bundle Workflow/GenerateAssetFileMap", priority = 203)]
        private static void GenerateAssetFileMap()
        {
            var list = new List<AssetBundleBuild>();
            helper.CollectAssets(UniPath.BundleRootPath, list);
            helper.GenerateAssetFileMap(list);
            AssetDatabase.Refresh();
        }

        [MenuItem("Bundle Workflow/GenerateAssetFileVersions", priority = 204)]
        private static void GenerateAssetFileVersions()
        {
            helper.GenerateAssetFileVersions();
            AssetDatabase.Refresh();
        }

        [MenuItem("Bundle Workflow/GenerateSettingFile", priority = 205)]
        private static void GenerateSettingFile()
        {
            helper.GenerateSettingFile();
            AssetDatabase.Refresh();
        }

        [MenuItem("Bundle Workflow/Test", priority = 206)]
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

        private static void InternalBuild(bool forceRebuild)
        {
            var platform = EditorUserBuildSettings.activeBuildTarget;
            var list = new List<AssetBundleBuild>();
            helper.CollectAssets(UniPath.BundleRootPath, list);
            var targetPath = helper.OutputPath;
            if (!Directory.Exists(targetPath))
                Directory.CreateDirectory(targetPath);
            var buildOption = BuildAssetBundleOptions.None;

            BuildSettingAsset.Instance.ConvertCompressType(ref buildOption);
            if (forceRebuild)
                buildOption |= BuildAssetBundleOptions.ForceRebuildAssetBundle;

            BuildPipeline.BuildAssetBundles(
                targetPath,
                list.ToArray(),
                buildOption,
                platform
            );

            helper.GenerateAssetFileMap(list);
            helper.GenerateAssetFileVersions();
            helper.GenerateSettingFile();

            AssetDatabase.Refresh();
        }

        public class Helper
        {
            public string OutputPath => Application.streamingAssetsPath;
            public ReadOnlyCollection<AssetBundleBuild> AllBuilds { get; private set; }

            public void CollectAssets(string rootPath, List<AssetBundleBuild> list)
            {
                var assetDic = new Dictionary<string, List<string>>();
                list.Clear();
                UniPath.WalkDataPath(
                    rootPath,
                    (dInfo, fInfo) =>
                    {
                        if (fInfo.Name.EndsWith(".meta"))
                            return;
                        List<string> assetNames;
                        if (!assetDic.TryGetValue(dInfo.FullName, out assetNames))
                        {
                            assetNames = new List<string>();
                            assetDic[dInfo.FullName] = assetNames;
                        }
                        assetNames.Add(fInfo.FullName);
                    }
                );

                foreach (KeyValuePair<string, List<string>> kv in assetDic)
                {
                    var build = new AssetBundleBuild();
                    string bundleName = UniPath.LocalPathToAssetBundleName(kv.Key);
                    string origin = bundleName;
                    Crypto.FromStringToMD5(ref bundleName);
                    build.assetBundleName = bundleName.Substring(0, 2) + "/" + bundleName;
                    build.assetNames = kv.Value
                        .ConvertAll(v => UniPath.LocalPathToDataPath(v))
                        .ToArray();
                    list.Add(build);
                }

                Debug.Log("Collect assets ok.");
            }

            public void GenerateAssetFileMap(List<AssetBundleBuild> list)
            {
                var dic = new Dictionary<string, string>();
                foreach (AssetBundleBuild build in list)
                {
                    foreach (string assetName in build.assetNames)
                        dic[assetName] = build.assetBundleName;
                }

                var sb = new StringBuilder();
                foreach (KeyValuePair<string, string> kv in dic)
                    sb.AppendLine($"{kv.Key}:{kv.Value}");

                string filePath = Path.Combine(OutputPath, "AssetMap.txt");
                using (var writer = new StreamWriter(filePath))
                {
                    writer.Write(sb.ToString());
                }

                Debug.Log("Mapping file generated successfully.");
            }

            public void GenerateAssetFileVersions()
            {
                string filePath = Path.Combine(OutputPath, "Version.txt");
                if (File.Exists(filePath))
                    File.Delete(filePath);
                var sb = new StringBuilder();
                var ignoreEnd = new Regex(@"\.(meta|manifest)$", RegexOptions.IgnoreCase);
                UniPath.WalkDirectory(
                    OutputPath,
                    (di, fi) => {
                        // 不需要计算meta文件和manifest文件
                        if (ignoreEnd.IsMatch(fi.Name))
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

            public void GenerateSettingFile()
            {
                var setting = new
                {
                    bundleRootPath = $"Assets/{UniPath.BundleRootPath}",
                };
                string jsonString = JsonUtility.ToJson(setting, true);
                string filePath = Path.Combine(UniPath.BundleOutputPath, "Setting.json");
                if (File.Exists(filePath))
                    File.Delete(filePath);
                using (var writer = new StreamWriter(filePath))
                {
                    writer.Write(jsonString);
                }
                Debug.Log("Setting file generated successfully");
            }
        }
    }
}