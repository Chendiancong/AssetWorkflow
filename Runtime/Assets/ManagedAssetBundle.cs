using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace cdc.AssetWorkflow
{
    internal class ManagedAssetBundle : BaseAsset<AssetBundle>, IAssetBundleHandle
    {
        public string bundlePath;
        public string version;
        public Dictionary<string, ManagedAsset> loadedAssets;

        string IAssetBundleHandle.Path => bundlePath;

        public ManagedAssetBundle(string bundleName, string bundlePath) : base()
        {
            name = bundleName;
            this.bundlePath = bundlePath;
            loadedAssets = new Dictionary<string, ManagedAsset>();
        }

        private TaskCompletionSource<AssetBundle> m_promise;
        private void OnBundleComplete(AsyncOperation operation)
        {
            var req = operation as AssetBundleCreateRequest;
            if (req.isDone)
            {
                state = ManagedAssetState.Loaded;
                asset = req.assetBundle;
                getProgress = null;
                m_promise.SetResult(asset);
            }
            else
            {
                state = ManagedAssetState.Failed;
                asset = null;
                getProgress = null;
                m_promise.SetException(new Exception($"Load bundle:{bundlePath} failed!"));
            }
        }

        public override ValueTask<AssetBundle> Get()
        {
            switch (state)
            {
                case ManagedAssetState.Initial:
                    {
                        state = ManagedAssetState.Loading;
                        m_promise = new TaskCompletionSource<AssetBundle>();
                        AssetBundleCreateRequest req = AssetBundle.LoadFromFileAsync(bundlePath);
                        req.completed += OnBundleComplete;
                        getProgress = () => req.progress;
                        return new ValueTask<AssetBundle>(m_promise.Task);
                    }
                case ManagedAssetState.Loading:
                    return new ValueTask<AssetBundle>(m_promise.Task);
                case ManagedAssetState.Loaded:
                    return new ValueTask<AssetBundle>(asset);
                default:
                    throw new Exception($"Error state {state}");
            }
        }

        public ManagedAsset GetAsset(string assetName)
        {
            ManagedAsset asset;
            if (!loadedAssets.TryGetValue(assetName, out asset))
                asset = new ManagedAsset(this, assetName);
            return asset;
        }

        IAssetHandle IAssetBundleHandle.GetAsset(string assetName) =>
            GetAsset(assetName);

        protected override void OnUnuse()
        {
            Unload(true);
        }

        private void Unload(bool unloadAllLoadedObjects)
        {
            state = ManagedAssetState.Unloaded;
            asset?.Unload(unloadAllLoadedObjects);
            asset = null;
            m_promise = null;
        }
    }
}