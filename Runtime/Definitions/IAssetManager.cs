using System.Threading.Tasks;

namespace cdc.AssetWorkflow
{
    public interface IAssetManager
    {
        /// <summary>
        /// 管理器初始化，在一个异步的过程里面顺序执行：加载配置->热更新->加载资源字典->加载AssetBundleManifest
        /// </summary>
        /// <returns></returns>
        ValueTask Initial();
        /// <summary>
        /// 获取一个资源句柄，需要注意的是，您只能加载Resources或者是被打包到AssetBundle中的资源。
        /// </summary>
        /// <param name="assetPath">资源在dataPath中的相对位置</param>
        /// <returns></returns>
        IAssetHandle MakeAsset(string assetPath);
        void DebugInfo(bool flag);
    }
}