using System.Threading.Tasks;
using UnityEngine;

namespace cdc.AssetWorkflow
{
    public interface IBaseHandle { }
    public interface IHandle<AssetType> : IBaseHandle
        where AssetType : UnityEngine.Object
    {
        /// <summary>
        /// 资源是否有效
        /// </summary>
        bool IsValid { get; }
        /// <summary>
        /// 资源名称
        /// </summary>
        string Name { get; }
        /// <summary>
        /// 资源状态
        /// </summary>
        ManagedAssetState State { get; }
        /// <summary>
        /// 加载进度
        /// </summary>
        float LoadingProgress { get; }
        /// <summary>
        /// 引用计数
        /// </summary>
        int RefCount { get; }

        /// <summary>
        /// 增加资源的引用计数
        /// </summary>
        void AddRef(int diff = 1);
        /// <summary>
        /// 减少资源的引用计数
        /// </summary>
        void DecRef(int diff = 1);
        /// <summary>
        /// 获取资源对象
        /// </summary>
        /// <returns></returns>
        ValueTask<AssetType> Get();
    }

    public interface IAssetHandle : IHandle<UnityEngine.Object>
    {
        /// <summary>
        /// 获取特定类型的资源对象
        /// </summary>
        /// <typeparam name="AssetType"></typeparam>
        ValueTask<AssetType> Cast<AssetType>()
            where AssetType : UnityEngine.Object;
    }

    public interface IAssetBundleHandle : IHandle<AssetBundle>
    {
        /// <summary>
        /// AssetBundle的加载路径
        /// </summary>
        string Path { get; }
        /// <summary>
        /// 从bundle中获取资源
        /// </summary>
        IAssetHandle GetAsset(string assetName);
    }
}