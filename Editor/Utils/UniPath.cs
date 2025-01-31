using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace cdc.BundleWorkFlow.Editor
{
    internal static class UniPath
    {
        /// <summary>
        /// 构建assetbundle的资源根目录
        /// </summary>
        public static string BundleRootPath => BuildSettingAsset.Instance.rootPath;

        private static Regex m_dataPathReg = new Regex($@"^\b{Application.dataPath}\b[\\\/]?");
        /// <summary>
        /// 将本地路径转换为项目路径，即Assets/...，且统一以反斜杠位分隔符
        /// </summary>
        public static string LocalPathToDataPath(string localPath)
        {
            localPath = localPath.Replace('\\', '/');
            return m_dataPathReg
                .Replace(localPath, "Assets/");
        }

        /// <summary>
        /// 将本地路径转换为对应的asset bundle名称
        /// </summary>
        public static string LocalPathToAssetBundleName(string localPath)
        {
            localPath = localPath.Replace('\\', '/');
            return m_dataPathReg
                .Replace(localPath, "")
                .Replace('/', '_');
        }

        /// <summary>
        /// 递归检查给定目录中的每个文件
        /// </summary>
        /// <param name="dataPath">相对于Application.dataPath的目录</param>
        /// <param name="action">每个文件的处理回调</param>
        public static void WalkDataPath(string dataPath, Action<DirectoryInfo, FileInfo> action)
        {
            string dirPath = Path.Combine(Application.dataPath, dataPath);
            if (Directory.Exists(dirPath))
            {
                DirectoryInfo di = new DirectoryInfo(dirPath);
                foreach (FileInfo fi in di.EnumerateFiles())
                    action(di, fi);
                foreach (DirectoryInfo _di in di.EnumerateDirectories())
                    WalkDataPath(Path.Combine(dataPath, _di.Name), action);
            }
        }

        /// <summary>
        /// 递归检查给定目录的每个文件
        /// </summary>
        /// <param name="path">本地目录，可以是绝对目录，也可以是项目的相对目录</param>
        /// <param name="action">每个文件的回调</param>
        public static void WalkDirectory(string path, Action<DirectoryInfo, FileInfo> action)
        {
            if (Directory.Exists(path))
            {
                var di = new DirectoryInfo(path);
                foreach (FileInfo fi in di.EnumerateFiles())
                    action(di, fi);
                foreach (DirectoryInfo _di in di.EnumerateDirectories())
                    WalkDirectory(_di.FullName, action);
            }
        }
    }
}