using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace cdc.BundleWorkFlow
{
    internal class ManagedAssetBundle : IAssetBundleHandle
    {
        public string bundlePath;
        public string name;
        public string version;
        public AssetBundle assetBundle;
        public int refCount;
        public ManagedAssetState state;
        public Dictionary<string, ManagedAsset> loadedAssets;
        private Func<float> m_getProgress;


        public int RefCount => refCount;

        public bool IsValid => state <= ManagedAssetState.Loaded;

        public bool IsLoaded => state == ManagedAssetState.Loaded;

        public float LoadingProgress
        {
            get
            {
                if (state == ManagedAssetState.Initial)
                    return 0f;
                if (state == ManagedAssetState.Loading)
                    return m_getProgress?.Invoke() ?? 0f;
                return 1f;
            }
        }

        public AssetBundle Target => assetBundle;

        public string Path => bundlePath;

        public string BundleName => name;

        public ManagedAssetState State => state;

        public ManagedAssetBundle(string bundleName, string bundlePath)
        {
            name = bundleName;
            this.bundlePath = bundlePath;
            refCount = 0;
            state = ManagedAssetState.Initial;
            loadedAssets = new Dictionary<string, ManagedAsset>();
        }

        private TaskCompletionSource<AssetBundle> m_promise;
        private void OnBundleComplete(AsyncOperation operation)
        {
            var req = operation as AssetBundleCreateRequest;
            if (req.isDone)
            {
                state = ManagedAssetState.Loaded;
                assetBundle = req.assetBundle;
                m_getProgress = null;
                m_promise.SetResult(assetBundle);
            }
            else
            {
                state = ManagedAssetState.Failed;
                assetBundle = null;
                m_getProgress = null;
                m_promise.SetException(new Exception($"Load bundle:{bundlePath} failed!"));
            }
        }

        public ValueTask<AssetBundle> LoadBundle()
        {
            if (IsLoaded)
                return new ValueTask<AssetBundle>(assetBundle);
            switch (state)
            {
                case ManagedAssetState.Initial:
                    {
                        state = ManagedAssetState.Loading;
                        m_promise = new TaskCompletionSource<AssetBundle>();
                        AssetBundleCreateRequest req = AssetBundle.LoadFromFileAsync(bundlePath);
                        req.completed += OnBundleComplete;
                        m_getProgress = () => req.progress;
                        return new ValueTask<AssetBundle>(m_promise.Task);
                    }
                case ManagedAssetState.Loading:
                    return new ValueTask<AssetBundle>(m_promise.Task);
                default:
                    throw new Exception($"Error state {state}");
            }
        }

        public void AddRef(int diff = 1)
        {
            refCount += diff;
        }

        public void DecRef(int diff = 1)
        {
            refCount = Math.Max(0, refCount - diff);
            if (refCount <= 0)
                Unload(true);
        }

        private void Unload(bool unloadAllLoadedObjects)
        {
            state = ManagedAssetState.Unloaded;
            assetBundle?.Unload(unloadAllLoadedObjects);
            assetBundle = null;
        }
    }

    public enum ManagedAssetState
    {
        Initial,
        Loading,
        Loaded,
        Unloaded,
        Failed
    }
}