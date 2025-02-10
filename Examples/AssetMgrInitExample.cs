using cdc.AssetWorkflow;
using UnityEngine;

public class AssetMgrInitExample : MonoBehaviour
{
    private IAssetManager m_assetMgr;
    public async void Doit()
    {
        if (m_assetMgr is null)
        {
            m_assetMgr = Facade.AssetMgr;
            await m_assetMgr.Initial();
        }
        var handle = m_assetMgr.MakeAsset("PackAssets/Sub/ManyCube.prefab");
        var prefab = await handle.Cast<GameObject>();
        Instantiate(prefab);
    }
}
