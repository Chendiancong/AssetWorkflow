using cdc.AssetWorkflow;
using UnityEngine;

public class AssetMgrInitExample : MonoBehaviour
{
    private async void Start()
    {
        var mgr = new AssetMgr();
        await mgr.Init();
    }
}
