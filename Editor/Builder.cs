using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace cdc.BundleWorkFlow.Editor
{
    internal static class Builder
    {
        public static Helper helper = new Helper();

        [MenuItem("Bundle Workflow/Normal Build", priority = 200)]
        public static void NormalBuild()
        {
            BuilderProcessorAttribute.ExecutePhase(BuilderProcessorPhase.PreBuild);
            InternalBuild(false);
            InternalPostBuild();
            BuilderProcessorAttribute.ExecutePhase(BuilderProcessorPhase.PostBuild);
        }

        [MenuItem("Bundle Workflow/Clean", priority = 201)]
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

        [MenuItem("Bundle Workflow/GenerateAssetFileMap", priority = 202)]
        private static void GenerateAssetFileMap()
        {
            string outputPath = helper.OutputPath;
            if (!Directory.Exists(outputPath))
                return;
        }

        [MenuItem("Bundle Workflow/Test", priority = 203)]
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

        private static void InternalPostBuild()
        {
        }

        private static void InternalBuild(bool forceRebuild)
        {
            var platform = EditorUserBuildSettings.activeBuildTarget;
            helper.BeforeBuild();
            helper.CollectAssets(UniPath.BundleRootPath);
            var targetPath = helper.OutputPath;
            if (!Directory.Exists(targetPath))
                Directory.CreateDirectory(targetPath);
            var buildOption = BuildAssetBundleOptions.None;

            BuildSettingAsset.Instance.ConvertCompressType(ref buildOption);
            if (forceRebuild)
                buildOption |= BuildAssetBundleOptions.ForceRebuildAssetBundle;

            BuildPipeline.BuildAssetBundles(
                targetPath,
                new List<AssetBundleBuild>(helper.AllBuilds).ToArray(),
                buildOption,
                platform
            );

            helper.GenerateAssetFileMap();
            helper.GenerateAssetFileVersions();

            AssetDatabase.Refresh();
        }

        public class Helper
        {
            private List<AssetBundleBuild> m_builds = new List<AssetBundleBuild>();
            public string OutputPath => Application.streamingAssetsPath;
            public ReadOnlyCollection<AssetBundleBuild> AllBuilds { get; private set; }

            public Helper()
            {
                AllBuilds = new ReadOnlyCollection<AssetBundleBuild>(m_builds);
            }

            public void BeforeBuild()
            {
                m_builds.Clear();
            }

            public void CollectAssets(string rootPath)
            {
                var assetDic = new Dictionary<string, List<string>>();
                var list = m_builds;
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
                    build.assetBundleName = UniPath
                        .LocalPathToAssetBundleName(kv.Key);
                    build.assetNames = kv.Value
                        .ConvertAll(v => UniPath.LocalPathToDataPath(v))
                        .ToArray();
                    list.Add(build);
                }
            }

            public void GenerateAssetFileMap()
            {
                var dic = new Dictionary<string, string>();
                foreach (AssetBundleBuild build in m_builds)
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
            }

            public void GenerateAssetFileVersions()
            {
                string filePath = Path.Combine(OutputPath, "Version.txt");
                if (File.Exists(filePath))
                    File.Delete(filePath);
                var sb = new StringBuilder();
                UniPath.WalkDirectory(
                    OutputPath,
                    (di, fi) => {
                        using (FileStream stream = File.OpenRead(fi.FullName))
                        {
                            using (MD5 md5 = MD5.Create())
                            {
                                byte[] hashBytes = md5.ComputeHash(stream);
                                string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                                sb.AppendLine($"{fi.Name}:{hash}");
                            }
                        }
                    }
                );

                using (var writer = new StreamWriter(filePath))
                {
                    writer.Write(sb.ToString());
                }
            }
        }
    }
}