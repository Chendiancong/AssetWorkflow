using System;
using System.Threading.Tasks;
using UnityEngine;

namespace cdc.AssetWorkflow
{
    internal class ManagedAsset : BaseAsset<UnityEngine.Object>, IAssetHandle
    {
        public ManagedAssetBundle bundle;

        public ManagedAsset(ManagedAssetBundle bundle, string name) : base()
        {
            this.bundle = bundle;
            this.name = name;
        }

        private TaskCompletionSource<UnityEngine.Object> m_promise;
        private void OnAssetComplete(AsyncOperation operation)
        {
            var req = operation as AssetBundleRequest;
            if (req.asset != null)
            {
                asset = req.asset;
                state = ManagedAssetState.Loaded;
                getProgress = null;
                m_promise.SetResult(asset);
            }
            else
            {
                asset = null;
                state = ManagedAssetState.Failed;
                getProgress = null;
                m_promise.SetException(new Exception($"Load {name} from bundle {bundle.name}"));
            }
        }

        public override ValueTask<UnityEngine.Object> Get()
        {
            return InternalGet(typeof(UnityEngine.Object));
        }

        public async ValueTask<AssetType> Cast<AssetType>()
            where AssetType : UnityEngine.Object
        {
            return await InternalGet(typeof(AssetType)) as AssetType;
        }

        private async ValueTask<UnityEngine.Object> InternalGet(Type type)
        {
            var asset = await InternalLoadAsset(type, await bundle.Get());
            return asset;
        }

        private ValueTask<UnityEngine.Object> InternalLoadAsset(Type type, AssetBundle assetBundle)
        {
            switch (state)
            {
                case ManagedAssetState.Initial:
                    {
                        m_promise = new TaskCompletionSource<UnityEngine.Object>();
                        AssetBundleRequest req = assetBundle.LoadAssetAsync(name, type);
                        req.completed += OnAssetComplete;
                        getProgress = () => req.progress;
                        return new ValueTask<UnityEngine.Object>(m_promise.Task);
                    }
                case ManagedAssetState.Loading:
                    return new ValueTask<UnityEngine.Object>(m_promise.Task);
                case ManagedAssetState.Loaded:
                    return new ValueTask<UnityEngine.Object>(asset);
                default:
                    throw new Exception($"Error state:{state}");
            }
        }

        protected override void OnUse()
        {
            bundle?.AddRef();
        }

        protected override void OnUnuse()
        {
            asset = null;
            state = ManagedAssetState.Unloaded;
            m_promise = null;
            bundle?.DecRef();
        }
    }
}