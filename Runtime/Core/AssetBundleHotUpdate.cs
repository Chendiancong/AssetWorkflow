using System.Threading.Tasks;

namespace cdc.BundleWorkFlow
{
    public struct AssetBundleHotUpdate
    {
        public void Initital(IAssetBundleManager mgr)
        {

        }

        public async Task HotUpdate(IAssetBundleManager mgr)
        {
            await Task.Delay(1000);
        }
    }
}