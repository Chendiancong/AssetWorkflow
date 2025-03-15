using UnityEngine.UI;

namespace cdc.AssetWorkflow
{
    public static class Facade
    {
        private static IAssetManager m_assetMgr;

        public static IAssetManager AssetMgr
        {
            get
            {
                if (m_assetMgr == null)
                    m_assetMgr = new AssetMgr();
                return m_assetMgr;
            }
        }
    }
}