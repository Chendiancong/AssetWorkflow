using System;
using System.Threading.Tasks;

namespace cdc.BundleWorkFlow
{
    public class AssetMgr : IAssetLoader, IAssetBundleManager
    {
        private AssetBundleHotUpdate m_hotUpdate;

        public AssetMgr()
        {
            m_hotUpdate = new AssetBundleHotUpdate();
        }

        public void Init()
        {
            m_hotUpdate.Initital(this);
        }

        public Task HotUpdate() => m_hotUpdate.HotUpdate(this);

        public void Load<AssetType>(string assetPath, Action<Exception, AssetType> onLoaded) where AssetType : UnityEngine.Object
        {
            throw new NotImplementedException();
        }

        public Task<AssetType> Load<AssetType>(string assetPath) where AssetType : UnityEngine.Object
        {
            throw new NotImplementedException();
        }

        bool IAssetBundleManager.AssetPathToBundleName(string assetPath, out string output)
        {
            throw new NotImplementedException();
        }

        void IAssetBundleManager.SetAssetPathPair(string assetPath, string bundleName)
        {
            throw new NotImplementedException();
        }

        void IAssetBundleManager.SetBundleVersion(string bundleName, string bundleVersion)
        {
            throw new NotImplementedException();
        }

        bool IAssetBundleManager.CompareVersion(string bundleName, string intputVersion)
        {
            throw new NotImplementedException();
        }
    }
}