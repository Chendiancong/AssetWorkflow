using System.Threading.Tasks;

namespace cdc.BundleWorkFlow
{
    public struct AssetHotUpdate
    {
        public void Initital(IAssetManager mgr)
        {

        }

        public async Task Execute(IAssetManager mgr)
        {
            await Task.Delay(1000);
        }
    }
}