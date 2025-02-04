using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace cdc.BundleWorkFlow
{
    internal class ManagedAssetBundle : IAssetBundleManageable
    {
        public string bundlePath;
        public string name;
        public string version;
        public AssetBundle assetBundle;
        public int refCount;
        public State state;
        public Dictionary<string, AssetInfo> loadedAssets;
        private Func<float> m_getProgress;

        public string Name => name;

        public int RefCount => refCount;

        public bool IsValid => state <= State.Loaded;

        public bool IsLoaded => state == State.Loaded;

        public float LoadingProgress
        {
            get
            {
                if (state == State.Initial)
                    return 0f;
                if (state == State.Loading)
                    return m_getProgress?.Invoke() ?? 0f;
                return 1f;
            }
        }

        public ManagedAssetBundle(string bundleName, string bundlePath)
        {
            name = bundleName;
            this.bundlePath = bundlePath;
            refCount = 0;
            state = State.Initial;
            loadedAssets = new Dictionary<string, AssetInfo>();
        }

        private TaskCompletionSource<AssetBundle> m_promise;
        private void OnBundleComplete(AsyncOperation operation)
        {
            var req = operation as AssetBundleCreateRequest;
            if (req.isDone)
            {
                state = State.Loaded;
                assetBundle = req.assetBundle;
                m_getProgress = null;
                m_promise.SetResult(assetBundle);
            }
            else
            {
                state = State.Failed;
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
                case State.Initial:
                    {
                        state = State.Loading;
                        m_promise = new TaskCompletionSource<AssetBundle>();
                        AssetBundleCreateRequest req = AssetBundle.LoadFromFileAsync(bundlePath);
                        req.completed += OnBundleComplete;
                        m_getProgress = () => req.progress;
                        return new ValueTask<AssetBundle>(m_promise.Task);
                    }
                case State.Loading:
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
            state = State.Unloaded;
            assetBundle?.Unload(unloadAllLoadedObjects);
            assetBundle = null;
        }

        public enum State
        {
            Initial,
            Loading,
            Loaded,
            Unloaded,
            Failed
        }

        public class AssetInfo
        {
            public AssetBundle assetBundle;
            public string assetName;
            public UnityEngine.Object asset;
            public State state;
            private Func<float> m_getProgress;

            public bool IsLoaded => state == State.Loaded;
            public float LoadingProgress
            {
                get
                {
                    if (state == State.Initial)
                        return 0f;
                    if (state == State.Loading)
                        return m_getProgress?.Invoke() ?? 0f;
                    return 1f;
                }
            }

            public AssetInfo(AssetBundle bundle, string name)
            {
                assetBundle = bundle;
                assetName = name;
            }

            private TaskCompletionSource<AssetInfo> m_promise;
            private void OnAssetComplete(AsyncOperation operation)
            {
                var req = operation as AssetBundleRequest;
                if (req.isDone)
                {
                    asset = req.asset;
                    state = State.Loaded;
                    m_getProgress = null;
                    m_promise.SetResult(this);
                }
                else
                {
                    asset = null;
                    state = State.Failed;
                    m_getProgress = null;
                    m_promise.SetException(new Exception($"Load {assetName} from bundle {assetBundle.name}"));
                }
            }
            public ValueTask<AssetInfo> LoadAsset()
            {
                if (IsLoaded)
                    return new ValueTask<AssetInfo>(this);
                else
                {
                    switch (state)
                    {
                        case State.Initial:
                            {
                                AssetBundleRequest req = assetBundle.LoadAssetAsync(assetName);
                                req.completed += OnAssetComplete;
                                m_getProgress = () => req.progress;
                                return new ValueTask<AssetInfo>(m_promise.Task);
                            }
                        case State.Loading:
                            return new ValueTask<AssetInfo>(m_promise.Task);
                        default:
                            throw new Exception($"Error state:{state}");
                    }
                }
            }

            public AssetType Cast<AssetType>()
                where AssetType : UnityEngine.Object
                => asset as AssetType;
        }
    }
}