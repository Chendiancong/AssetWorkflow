using System;
using System.Text.RegularExpressions;

namespace cdc.AssetWorkflow
{
    [Serializable]
    public class AssetRef
    {
        private static Regex m_assetPathPrefix = new Regex(@"^Assets/");

        [UnityEngine.SerializeField]
        private string m_assetPath;

        public string AssetPath => m_assetPathPrefix.Replace(m_assetPath, "");
        public IAssetHandle Handle => Facade.AssetMgr.MakeAsset(AssetPath);
    }
}