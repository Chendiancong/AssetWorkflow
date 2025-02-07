using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace cdc.AssetWorkflow
{
    public class AssetMgr : IAssetLoader, IAssetManager
    {
        private AssetMgrHelper m_helper;
        private Dictionary<string, IBaseAsset> m_cachedAssets;
        private AssetBundleManifest m_manifest;

        public AssetMgr()
        {
            m_helper = new AssetMgrHelper();
            m_cachedAssets = new Dictionary<string, IBaseAsset>();
        }

        public void Init()
        {
            m_helper.LoadSetting();
        }

        public async ValueTask HotUpdate()
        {
            await m_helper.HotUpdate();
        }

        #region IAssetLoader
        private Regex m_resourcePath = new Regex(@"^Assets[//]Resources");
        public void LoadAsset(string assetPath, Action<Exception, IAssetHandle> onLoaded)
        {
            string dataPath = $"Asset/{assetPath}";
            if (m_resourcePath.IsMatch(dataPath))
                LoadFromResources(assetPath)
                    .AsTask()
                    .ContinueWith(task => onLoaded(task.Exception, task.Result));

        }

        public ValueTask<IAssetHandle> LoadAsset(string assetPath)
        {
            throw new NotImplementedException();
        }

        private ValueTask<IAssetHandle> LoadFromResources(string assetPath)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IAssetManager
        #endregion
    }
}