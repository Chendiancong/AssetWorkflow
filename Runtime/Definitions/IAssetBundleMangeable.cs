using System.Threading.Tasks;
using UnityEngine;

namespace cdc.BundleWorkFlow
{
    public interface IAssetBundleManageable
    {
        string Name { get; }
        int RefCount { get; }
        float LoadingProgress { get; }
        bool IsValid { get; }
        bool IsLoaded { get; }
        void AddRef(int diff = 1);
        void DecRef(int diff = 1);
        ValueTask<AssetBundle> LoadBundle();
    }
}