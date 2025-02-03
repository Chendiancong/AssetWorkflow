using UnityEditor;
using UnityEngine;

namespace cdc.BundleWorkFlow.Editor
{
    internal class BuildSettingAsset : ScriptableObject
    {
        [Tooltip("构建asset bundle的资源的根目录，它是相对于Assets的目录，统一用正斜杠(/)进行分割")]
        /// <summary>
        /// 构建asset bundle的资源的根目录，它是相对于Assets的目录，统一用正斜杠(/)进行分割
        /// </summary>
        public string rootPath = "PackAssets";
        [Tooltip("asset bundle的压缩类型，No为不压缩，ChunkedCompress为LZ4，FullCompress为LZMA")]
        /// <summary>
        /// asset bundle的压缩类型，No为不压缩，ChunkedCompress为LZ4，FullCompress为LZMA
        /// </summary>
        public CompressType compressType = CompressType.ChunkedCompress;
        /// <summary>
        /// asset bundle的输出目录(系统本地目录)
        /// </summary>
        public string OutputPath => Application.streamingAssetsPath;

        private static BuildSettingAsset m_instance;
        public static BuildSettingAsset Instance
        {
            get
            {
                if (m_instance == null)
                {
                    string path = "Assets/bundle-workflow/BuildSettingAsset.asset";
                    m_instance = AssetDatabase.LoadAssetAtPath<BuildSettingAsset>(path);;
                    if (m_instance == null)
                    {
                        m_instance = CreateInstance<BuildSettingAsset>();
                        AssetDatabase.CreateAsset(m_instance, path);
                        AssetDatabase.SaveAssets();
                    }
                }
                return m_instance;
            }
        }

        public void ConvertCompressType(ref BuildAssetBundleOptions origin)
        {
            switch (compressType)
            {
                case CompressType.No:
                    origin |= BuildAssetBundleOptions.UncompressedAssetBundle;
                    break;
                case CompressType.ChunkedCompress:
                    origin |= BuildAssetBundleOptions.ChunkBasedCompression;
                    break;
                case CompressType.FullCompress:
                    break;
            }
        }

        public enum CompressType
        {
            No = 1,
            ChunkedCompress,
            FullCompress
        }
    }
}