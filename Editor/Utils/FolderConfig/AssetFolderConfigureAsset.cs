using UnityEngine;

namespace cdc.AssetWorkflow.Editor
{
    internal interface IFolderConfigure
    {
        AssetBundlePackingLevel PackingLevel { get; }
    }

    internal class AssetFolderConfigureAsset : ScriptableObject, IFolderConfigure
    {
        [Tooltip("No:改目录下的资源不参与构建\nNormal:该目录下的资源统一进行构建\nSingle:该目录下的资源单独进行构建")]
        public AssetBundlePackingLevel packingLevel = AssetBundlePackingLevel.Normal;

        public static readonly IFolderConfigure defaultConfig = new Default();

        public AssetBundlePackingLevel PackingLevel => packingLevel;

        public class Default : IFolderConfigure
        {
            public AssetBundlePackingLevel PackingLevel => AssetBundlePackingLevel.Normal;
        }
    }

    internal enum AssetBundlePackingLevel
    {
        No,
        Normal,
        Single,
    }
}