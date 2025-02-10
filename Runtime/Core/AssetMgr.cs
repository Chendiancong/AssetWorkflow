using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace cdc.AssetWorkflow
{
    internal class AssetMgr : IAssetManager
    {
        private AssetMgrHelper m_helper;
        private Dictionary<string, IBaseAsset> m_cachedAssets;
        private AssetBundleManifest m_manifest;

        public AssetMgr()
        {
            m_helper = new AssetMgrHelper();
            m_cachedAssets = new Dictionary<string, IBaseAsset>();
        }

        public async ValueTask Initial()
        {
            m_helper.LoadSetting();
            await m_helper.HotUpdate();
            await m_helper.PrepareAssetMap();
            m_manifest = await LoadManifest();
        }

        public async ValueTask LoadBundleDependencies(string bundleName)
        {
            if (bundleName.StrEquals(m_helper.config.manifestName))
                return;
            foreach (string dep in m_manifest.GetDirectDependencies(bundleName))
            {
                var bundle = MakeAssetBundle(dep) as ManagedAssetBundle;
                Debugger.Log($"prepare dep bundle {bundle.name}");
                await bundle.Get();
                bundle.AddRef();
            }
        }

        #region IAssetLoader
        private Regex m_resourcesPrefix = new Regex(@"^Assets/Resources");
        public IAssetHandle MakeAsset(string assetName)
        {
            assetName = $"Assets/{assetName}";
            if (m_resourcesPrefix.IsMatch(assetName))
                return MakeAssetFromResources(assetName).CastToHandle<IAssetHandle>();
            else
                return MakeAssetFromBundle(assetName).CastToHandle<IAssetHandle>();

        }

        private IBaseAsset MakeAssetFromResources(string assetName)
        {
            IBaseAsset asset;
            if (m_cachedAssets.TryGetValue(assetName, out asset))
                return asset;
            else
            {
                asset = new ManagedResourceAsset(m_resourcesPrefix.Replace(assetName, ""));
                m_cachedAssets[assetName] = asset;
                return asset;
            }
        }

        private IBaseAsset MakeAssetFromBundle(string assetName)
        {
            string bundlePath;
            if (!m_helper.AssetNameToBundleName(assetName, out bundlePath))
                throw new Exception($"missing asset bundle for {assetName}");
            var bundle = MakeAssetBundle(bundlePath) as ManagedAssetBundle;
            return bundle.GetAsset(assetName);
        }

        private IBaseAsset MakeAssetBundle(string bundleName)
        {
            IBaseAsset asset;
            if (!m_cachedAssets.TryGetValue(bundleName, out asset))
            {
                asset = new ManagedAssetBundle(
                    bundleName,
                    m_helper.GetLocalLoadPath(bundleName),
                    this
                );
                m_cachedAssets[bundleName] = asset;
            }
            return asset;
        }

        public void DebugInfo(bool flag)
        {
            Debugger.enabled = flag;
        }

        #endregion

        private async ValueTask<AssetBundleManifest> LoadManifest()
        {
            string manifestName = m_helper.config.manifestName;
            string manifestPath = m_helper.GetLocalLoadPath(manifestName);
            ManagedAssetBundle bundleAsset = new ManagedAssetBundle(
                manifestName,
                manifestPath,
                this
            );
            Debugger.Log($"manifest bundle is {manifestName}, located in {manifestPath}");
            m_cachedAssets[$"manifest->{manifestName}"] = bundleAsset;
            ManagedAsset asset = bundleAsset.GetAsset("AssetBundleManifest");
            asset.AddRef();
            return await asset.Cast<AssetBundleManifest>();
        }
    }
}