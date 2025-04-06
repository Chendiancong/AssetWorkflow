using System.IO;
using UnityEditor;
using UnityEditor.EditorTools;
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
        /// 资源服务器地址
        /// </summary>
        [Tooltip("资源服务器地址")]
        public string serverUrl = "http://localhost";
        /// <summary>
        /// 是否使用自定义输出目录
        /// 自定义目录可以是绝对目录也可以是相对目录，相对目录以项目目录为起始目录
        /// 否则使用默认输出目录（Application.streamingAssetsPath），
        /// </summary>
        [Tooltip("是否使用自定义的输出目录\n自定义目录可以是绝对目录也可以是相对目录，相对目录以项目目录为起始目录\n否则使用默认的输出目录(Application.streamingAssetsPath)")]
        public bool useCustomOutputPath = false;
        /// <summary>
        /// 自定义输出目录
        /// </summary>
        [Tooltip("自定义输出目录")]
        public string customOutputPath = "Assets/StreamingAssets";
        /// <summary>
        /// 是否使用自定义的构建平台
        /// 默认为当前激活的平台
        /// </summary>
        [Tooltip("是否使用自定义的构建平台\n默认为当前激活的平台")]
        public bool useCustomBuildTarget = false;
        /// <summary>
        /// 自定义的构建平台
        /// </summary>
        [Tooltip("自定义的构建平台\n选择NoTarget则为当前平台")]
        public BuildTarget buildTarget = BuildTarget.NoTarget;
        /// <summary>
        /// 当前需要OperationWindow需要执行哪一项操作
        /// </summary>
        public OperationType operationType = OperationType.NormalBuild;
        /// <summary>
        /// asset bundle的输出目录(系统本地目录)
        /// </summary>
        public string OutputPath => !enablePatch || !useCustomOutputPath ?
            Application.streamingAssetsPath :
            customOutputPath;
        /// <summary>
        /// 使用内置的资源服务器进行测试
        /// </summary>
        [Tooltip("是否使用内置的资源服务器进行测试")]
        public bool useInternalAssetServer = false;
        /// <summary>
        /// 内置资源服务器的根目录
        /// </summary>
        [Tooltip("内置资源服务器的根目录")]
        public string testServerRoot = "";

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

        public AssetMgrConfig CreateConfig(ref BuilderCommand cmd)
        {
            var config = new AssetMgrConfig();
            config.enablePatch = enablePatch;
            config.bundleRootPath = rootPath;
            config.manifestName = Path.GetFileName(EditorFileSystem.GetOutputPath(cmd.GetTrulyBuildTarget()));
            config.serverUrl = serverUrl;
            return config;
        }

        public enum CompressType
        {
            No = 1,
            ChunkedCompress,
            FullCompress
        }

        public enum OperationType
        {
            [InspectorName("选择需要执行的操作")]
            NoSelect,
            [InspectorName("增量构建")]
            NormalBuild,
            [InspectorName("重新构建")]
            ForceBuild,
            [InspectorName("清理构建目录")]
            Clean,
        }
    }
}