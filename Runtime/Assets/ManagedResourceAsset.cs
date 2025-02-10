using System;
using System.Threading.Tasks;
using UnityEngine;

namespace cdc.AssetWorkflow
{
    internal class ManagedResourceAsset : BaseAsset<UnityEngine.Object>, IAssetHandle
    {
        public ManagedResourceAsset(string assetPath)
        {
            name = assetPath;
        }

        private TaskCompletionSource<UnityEngine.Object> m_promise;
        private void OnAssetComplete(AsyncOperation operation)
        {
            var req = operation as ResourceRequest;
            if (req.isDone)
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
                m_promise.SetException(new Exception($"Load resource {name}"));
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

        private ValueTask<UnityEngine.Object> InternalGet(Type type)
        {
            switch (state)
            {
                case ManagedAssetState.Initial:
                    {
                        m_promise = new TaskCompletionSource<UnityEngine.Object>();
                        ResourceRequest req = Resources.LoadAsync(name, type);
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

        protected override void OnUnuse()
        {
            // need unload unused asset
            asset = null;
            state = ManagedAssetState.Unloaded;
            m_promise = null;
        }
    }
}