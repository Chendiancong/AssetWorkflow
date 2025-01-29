using UnityEngine;
using UnityEditor;

namespace cdc.BundleWorkFlow
{
    internal static class BuildSetting
    {
        /// <summary>
        /// 构建assetbundle的资源根目录
        /// </summary>
        public static string RootPath => BuildSettingAsset.Instance.rootPath;
    }
}