using System;
using System.Threading.Tasks;

namespace cdc.BundleWorkFlow
{
    public interface IAssetLoader
    {
        /// <summary>
        /// 加载Resources或AssetBundle中的资源
        /// </summary>
        /// <param name="assetPath">Resources或asset bundle rootPath的相对位置</param>
        /// <param name="onLoaded">加载完成回调</param>
        /// <typeparam name="AssetType"></typeparam>
        void Load<AssetType>(string assetPath, Action<Exception, AssetType> onLoaded)
            where AssetType : UnityEngine.Object;

        /// <summary>
        /// 加载Resources或AssetBundle中的资源
        /// </summary>
        /// <param name="assetPath">Resources或asset bundle rootPath的位置</param>
        /// <typeparam name="AssetType"></typeparam>
        /// <returns></returns>
        Task<AssetType> Load<AssetType>(string assetPath)
            where AssetType : UnityEngine.Object;
    }
}