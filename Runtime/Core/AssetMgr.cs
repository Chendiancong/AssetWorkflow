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
        private Dictionary<string, string> m_persistentPaths;
        private Dictionary<string, string> m_assetPath2Bundles;
        private Dictionary<string, IBaseAsset> m_cachedAssets;
        private AssetBundleManifest m_manifest;
        private AssetMgrConfig m_setting;

        public AssetMgr()
        {
            m_helper = new AssetMgrHelper();
            m_assetPath2Bundles = new Dictionary<string, string>();
            m_cachedAssets = new Dictionary<string, IBaseAsset>();
        }

        public async ValueTask Init()
        {
            m_setting = m_helper.LoadSetting(this);
            Debug.Log(ObjectDumper.Dump(m_setting));
            await m_helper.HotUpdate(this);

            // m_manifest = await LoadManifest();
            // Debug.Log(m_manifest);
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

        private ValueTask<IAssetHandle> LoadFromAssetBundle(string assetPath)
        {
            throw new NotImplementedException();
        }

        private ValueTask<IAssetHandle> LoadFromAssetBundle2(string finalPath)
        {
            IBaseAsset asset;
            if (!m_cachedAssets.TryGetValue(finalPath, out asset))
            {

            }
            return new ValueTask<IAssetHandle>(asset.CastToHandle<IAssetHandle>());
        }

        #endregion

        #region IAssetManager
        bool IAssetManager.AssetPathToBundleName(string assetPath, out string output)
        {
            return AssetPathToBundleName(assetPath, out output);
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
        #endregion

        private bool AssetPathToBundleName(string assetPath, out string output)
        {
            return m_assetPath2Bundles.TryGetValue(assetPath, out output);
        }
    }
}