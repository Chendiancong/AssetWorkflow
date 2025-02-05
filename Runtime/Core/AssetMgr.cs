using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace cdc.AssetWorkflow
{
    public class AssetMgr : IAssetLoader, IAssetManager
    {
        private AssetHotUpdate m_hotUpdate;
        private Dictionary<string, string> m_persistentPaths;
        private Dictionary<string, string> m_assetPath2Bundles;
        private Dictionary<string, ManagedAssetBundle> m_bundles;

        public AssetMgr()
        {
            m_hotUpdate = new AssetHotUpdate();
            m_assetPath2Bundles = new Dictionary<string, string>();
            m_bundles = new Dictionary<string, ManagedAssetBundle>();
        }

        public void Init()
        {
            m_hotUpdate.Initital(this);
        }

        public Task HotUpdate() => m_hotUpdate.Execute(this);

        public void LoadResource<AssetType>(string assetPath, Action<Exception, AssetType> onLoaded)
            where AssetType : UnityEngine.Object
        {
            throw new NotImplementedException();
        }

        public ValueTask<AssetType> LoadResource<AssetType>(string assetPath)
            where AssetType : UnityEngine.Object
        {
            throw new NotImplementedException();
        }

        public void LoadBundleAsset<AssetType>(string assetPath, Action<Exception, AssetType> onLoaded) where AssetType : UnityEngine.Object
        {
            throw new NotImplementedException();
        }

        public ValueTask<AssetType> LoadBundleAsset<AssetType>(string assetPath) where AssetType : UnityEngine.Object
        {
            throw new NotImplementedException();
        }

        bool IAssetManager.AssetPathToBundleName(string assetPath, out string output)
        {
            throw new NotImplementedException();
        }

        bool IAssetManager.GetBundlePath(string bundleName, out string bundlePath)
        {
            if (!m_persistentPaths.TryGetValue(bundleName, out bundlePath))
            {
                bundlePath = Path.Combine(
                    Application.streamingAssetsPath,
                    bundleName.Substring(0, 2),
                    bundleName
                );
            }

            return true;
        }

        void IAssetManager.SetBundlePath(string bundleName, string bundlePath)
        {
            m_persistentPaths[bundleName] = bundlePath;
        }

        void IAssetManager.SetAssetPathPair(string assetPath, string bundleName)
        {
            throw new NotImplementedException();
        }

        void IAssetManager.SetBundleVersion(string bundleName, string bundleVersion)
        {
            throw new NotImplementedException();
        }

        bool IAssetManager.CompareVersion(string bundleName, string intputVersion)
        {
            throw new NotImplementedException();
        }
    }
}