using System.Threading.Tasks;
using UnityEngine;

namespace cdc.BundleWorkFlow
{
    public interface IBaseHandle
    {
        bool IsValid { get; }
        int RefCount { get; }
        float LoadingProgress { get; }
        void AddRef(int diff = 1);
        void DecRef(int diff = 1);
    }

    public interface IAssetBundleHandle : IBaseHandle
    {
        AssetBundle Target { get; }
        string Path { get; }
        string BundleName { get; }
        ManagedAssetState State { get; }

        ValueTask<AssetBundle> LoadBundle();
    }

    public interface IAssetHandle : IBaseHandle
    {
        Object Target { get; }
        string AssetName { get; }
        ManagedAssetState State { get; }

        ValueTask<IAssetHandle> LoadAsset();
        AssetType Cast<AssetType>()
            where AssetType : Object;
    }
}