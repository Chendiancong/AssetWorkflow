using System;
using System.Collections.Generic;
using System.IO;
using PlasticGui.Diff.Annotate;
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
            // helper.CollectAssets(BuildSetting.RootPath);
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
            var builds = helper.CollectAssets(UniPath.BundleRootPath);
            var targetPath = helper.OutputPath;
            if (!Directory.Exists(targetPath))
                Directory.CreateDirectory(targetPath);
            var buildOption = BuildAssetBundleOptions.None;

            BuildSettingAsset.Instance.ConvertCompressType(ref buildOption);
            if (forceRebuild)
                buildOption |= BuildAssetBundleOptions.ForceRebuildAssetBundle;

            BuildPipeline.BuildAssetBundles(
                targetPath,
                builds.ToArray(),
                buildOption,
                platform
            );
            AssetDatabase.Refresh();
        }

        public class Helper
        {
            public string OutputPath => Application.streamingAssetsPath;
            public List<AssetBundleBuild> CollectAssets(string rootPath)
            {
                var list = new List<AssetBundleBuild>();
                var assetDic = new Dictionary<string, List<string>>();
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
                return list;
            }

            public void GenerateAddressableMap(Dictionary<string, string> dic)
            {
                UniPath.WalkDirectory(
                    OutputPath,
                    (di, fi) => {
                    }
                );
            }
        }
    }
}