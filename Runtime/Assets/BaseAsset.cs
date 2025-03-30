using System;
using System.Threading.Tasks;

namespace cdc.AssetWorkflow
{
    public enum ManagedAssetState
    {
        Initial,
        Loading,
        Loaded,
        Unloaded,
        Failed
    }

    internal interface IBaseAsset
    {
        AssetImpl CastToAsset<AssetImpl>() where AssetImpl : IBaseAsset;
        HandleImpl CastToHandle<HandleImpl>() where HandleImpl : IBaseHandle;
    }

    internal abstract class BaseAsset<AssetType> : IBaseAsset, IHandle<AssetType>
        where AssetType : UnityEngine.Object
    {
        public AssetType asset;
        public string name;
        public int refCount;
        public ManagedAssetState state;
        protected Func<float> getProgress;

        #region IHandle implemetations
        public virtual bool IsValid => state <= ManagedAssetState.Loaded;

        string IHandle<AssetType>.Name => name;

        ManagedAssetState IHandle<AssetType>.State => state;

        int IHandle<AssetType>.RefCount => refCount;

        public float LoadingProgress
        {
            get
            {
                if (state == ManagedAssetState.Initial)
                    return 0f;
                if (state == ManagedAssetState.Loading)
                    return getProgress?.Invoke() ?? 0f;
                return 1f;
            }
        }
        #endregion

        public BaseAsset()
        {
            refCount = 0;
            state = ManagedAssetState.Initial;
        }

        #region IBaseAsset implementations
        AssetImpl IBaseAsset.CastToAsset<AssetImpl>() => (AssetImpl)(IBaseAsset)this;
        HandleImpl IBaseAsset.CastToHandle<HandleImpl>() => (HandleImpl)(IBaseHandle)this;
        #endregion

        #region IHandle implementaions
        public abstract ValueTask<AssetType> Get();

        public void AddRef(int diff = 1)
        {
            if (!IsValid)
                return;
            if (diff <= 0)
                throw new Exception("Ref diff must >= 1");
            int origin = refCount;
            refCount += diff;
            if (origin == 0 && refCount > 0)
                OnUse();
        }

        public void DecRef(int diff = 1)
        {
            if (!IsValid)
                return;
            if (diff <= 0)
                throw new Exception("Ref diff must >= 1");
            int origin = refCount;
            refCount = Math.Max(0, refCount - diff);
            if (origin > 0 && refCount == 0)
                OnUnuse();
        }
        #endregion

        protected virtual void OnUse() { }

        protected virtual void OnUnuse() { }
    }
}