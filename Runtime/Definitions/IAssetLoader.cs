using System;
using System.Threading.Tasks;

namespace cdc.AssetWorkflow
{
    public interface IAssetLoader
    {
        /// <summary>
        /// 加载Resources中的资源
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="onLoaded"></param>
        /// <typeparam name="AssetType"></typeparam>
        void LoadResource<AssetType>(string assetPath, Action<Exception, AssetType> onLoaded)
            where AssetType : UnityEngine.Object;

        /// <summary>
        /// 加载Resource中的资源
        /// </summary>
        /// <typeparam name="AssetType"></typeparam>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        ValueTask<AssetType> LoadResource<AssetType>(string assetPath)
            where AssetType : UnityEngine.Object;
        /// <summary>
        /// 加载AssetBundle中的资源
        /// </summary>
        /// <param name="assetPath">Resources或asset bundle rootPath的相对位置</param>
        /// <param name="onLoaded">加载完成回调</param>
        /// <typeparam name="AssetType"></typeparam>
        void LoadBundleAsset<AssetType>(string assetPath, Action<Exception, AssetType> onLoaded)
            where AssetType : UnityEngine.Object;

        /// <summary>
        /// 加载Resources或AssetBundle中的资源
        /// </summary>
        /// <param name="assetPath">Resources或asset bundle rootPath的位置</param>
        /// <typeparam name="AssetType"></typeparam>
        /// <returns></returns>
        ValueTask<AssetType> LoadBundleAsset<AssetType>(string assetPath)
            where AssetType : UnityEngine.Object;
    }
}