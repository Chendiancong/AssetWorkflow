using System;
using System.Threading.Tasks;
using UnityEngine;

namespace cdc.AssetWorkflow
{
    internal class ManagedAsset : IAssetHandle
    {
        public ManagedAssetBundle bundle;
        public string assetName;
        public UnityEngine.Object asset;
        public ManagedAssetState state;
        public int refCount;
        private Func<float> m_getProgress;

        public bool IsValid => state <= ManagedAssetState.Loaded;

        public int RefCount => refCount;

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

        public UnityEngine.Object Target => asset;

        public string AssetName => assetName;

        public ManagedAssetState State => state;

        public ManagedAsset(ManagedAssetBundle bundle, string name)
        {
            this.bundle = bundle;
            assetName = name;
            refCount = 0;
        }

        private TaskCompletionSource<ManagedAsset> m_promise;
        private void OnAssetComplete(AsyncOperation operation)
        {
            var req = operation as AssetBundleRequest;
            if (req.isDone)
            {
                asset = req.asset;
                state = ManagedAssetState.Loaded;
                m_getProgress = null;
                m_promise.SetResult(this);
            }
            else
            {
                asset = null;
                state = ManagedAssetState.Failed;
                m_getProgress = null;
                m_promise.SetException(new Exception($"Load {assetName} from bundle {bundle.name}"));
            }
        }

        public ValueTask<ManagedAsset> LoadAsset()
        {
            switch (state)
            {
                case ManagedAssetState.Initial:
                    {
                        m_promise = new TaskCompletionSource<ManagedAsset>();
                        AssetBundleRequest req = bundle.assetBundle.LoadAssetAsync(assetName);
                        req.completed += OnAssetComplete;
                        m_getProgress = () => req.progress;
                        return new ValueTask<ManagedAsset>(m_promise.Task);
                    }
                case ManagedAssetState.Loading:
                    return new ValueTask<ManagedAsset>(m_promise.Task);
                case ManagedAssetState.Loaded:
                    return new ValueTask<ManagedAsset>(this);
                default:
                    throw new Exception($"Error state:{state}");
            }
        }

        ValueTask<IAssetHandle> IAssetHandle.LoadAsset() => throw new NotImplementedException();

        public AssetType Cast<AssetType>()
            where AssetType : UnityEngine.Object
            => asset as AssetType;

        public void AddRef(int diff = 1)
        {
            throw new NotImplementedException();
        }

        public void DecRef(int diff = 1)
        {
            throw new NotImplementedException();
        }
    }
}