using System;
using System.Threading.Tasks;

namespace cdc.AssetWorkflow
{
    public interface IAssetLoader
    {

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="assetPath">资源在dataPath中的相对位置</param>
        /// <param name="onLoaded">加载回调，当发生异常时首个参数不为空</param>
        void LoadAsset(string assetPath, Action<Exception, IAssetHandle> onLoaded);

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="assetPath">资源在dataPath中的相对位置</param>
        /// <returns></returns>
        ValueTask<IAssetHandle> LoadAsset(string assetPath);
    }
}