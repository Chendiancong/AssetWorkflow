using cdc.AssetWorkflow;
using UnityEngine;

public class AssetMgrInitExample : MonoBehaviour
{
    private AssetMgr m_assetMgr;
    public async void Doit()
    {
        if (m_assetMgr is null)
        {
            m_assetMgr = new AssetMgr();
            m_assetMgr.Init();
            await m_assetMgr.HotUpdate();
        }
    }
}
