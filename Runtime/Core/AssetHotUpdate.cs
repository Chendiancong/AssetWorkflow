using System.Threading.Tasks;

namespace cdc.AssetWorkflow
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