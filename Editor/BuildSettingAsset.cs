using System.IO;
using UnityEditor;
using UnityEngine;

namespace cdc.AssetWorkflow.Editor
{
    internal class BuildSettingAsset : ScriptableObject
    {
        /// <summary>
        /// 构建asset bundle的资源的根目录，它是相对于Application.dataPath的目录，统一用正斜杠(/)进行分割
        /// </summary>
        [Tooltip("构建asset bundle的资源的根目录，它是相对于Assets的目录，统一用正斜杠(/)进行分割")]
        public string rootPath = "PackAssets";
        /// <summary>
        /// asset bundle的压缩类型，No为不压缩，ChunkedCompress为LZ4，FullCompress为LZMA
        /// </summary>
        [Tooltip("asset bundle的压缩类型，No为不压缩，ChunkedCompress为LZ4，FullCompress为LZMA")]
        public CompressType compressType = CompressType.ChunkedCompress;
        /// <summary>
        /// 是否启用热更新
        /// </summary>
        [Tooltip("是否启用热更新\n不启用热更新的话AssetBundle会固定输出到{Application.dataPath}")]
        public bool enablePatch = true;
        /// <summary>
        /// 是否使用本地服务
        /// </summary>
        [Tooltip("是否使用本地服务")]
        public bool useLocalServer = true;
        /// <summary>
        /// 资源服务器地址
        /// </summary>
        [Tooltip("资源服务器地址")]
        public string serverUrl = "http://localhost";
        [Tooltip("端口id")]
        [Min(1)]
        public int host = 8080;
        /// <summary>
        /// 是否使用默认输出目录（Application.streamingAssetsPath），
        /// 自定义目录可以是绝对目录也可以是相对目录，相对目录以项目目录为起始目录
        /// </summary>
        [Tooltip("是否使用默认的输出目录（Application.streamingAssetsPath）\n自定义目录可以是绝对目录也可以是相对目录，相对目录以项目目录为起始目录")]
        public bool useDefaultOutputPath = false;
        /// <summary>
        /// 自定义输出目录
        /// </summary>
        [Tooltip("自定义输出目录")]
        public string customOutputPath = "Assets/StreamingAssets";
        /// <summary>
        /// asset bundle的输出目录(系统本地目录)
        /// </summary>
        public string OutputPath => useDefaultOutputPath ?
            Application.streamingAssetsPath :
            customOutputPath;

        private static BuildSettingAsset m_instance;
        public static BuildSettingAsset Instance
        {
            get
            {
                if (m_instance == null)
                {
                    
                    string path = "Assets/AssetWorkflow/BuildSettingAsset.asset";
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

        public AssetMgrConfig CreateConfig()
        {
            var config = new AssetMgrConfig();
            config.enablePatch = enablePatch;
            config.bundleRootPath = rootPath;
            config.manifestName = Path.GetFileName(OutputPath);
            config.serverUrl = useLocalServer ? "http://localhost" : serverUrl;
            config.host = host;
            return config;
        }

        public enum CompressType
        {
            No = 1,
            ChunkedCompress,
            FullCompress
        }
    }
}