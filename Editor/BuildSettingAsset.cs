using UnityEditor;
using UnityEngine;

namespace cdc.BundleWorkFlow
{
    internal class BuildSettingAsset : ScriptableObject
    {
        public string rootPath = "PackAssets";
        public CompressType compressType = CompressType.ChunkedCompress;

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