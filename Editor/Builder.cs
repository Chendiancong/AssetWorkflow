using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace cdc.BundleWorkFlow
{
    internal static class Builder
    {
        public static Helper helper = new Helper();

        [MenuItem("Bundle Workflow/Build", priority = 200)]
        public static void Build()
        {
            InternalBuild(false);
        }

        [MenuItem("Bundle Workflow/Clean", priority = 201)]
        public static void Clean()
        {
            string targetPath = helper.OutputPath;
            if (Directory.Exists(targetPath))
                Directory.Delete(targetPath, true);
        }

        private static void InternalBuild(bool forceRebuild)
        {
            var platform = EditorUserBuildSettings.activeBuildTarget;
            var builds = helper.CollectAssets(BuildSettingAsset.Instance.rootPath);
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
        }

        public class Helper
        {
            public string OutputPath => Application.streamingAssetsPath;
            public List<AssetBundleBuild> CollectAssets(string rootPath)
            {
                var list = new List<AssetBundleBuild>();
                WalkDirectory(rootPath, list);
                return list;
            }

            private void WalkDirectory(string dirPath, List<AssetBundleBuild> list)
            {
            }
        }
    }
}