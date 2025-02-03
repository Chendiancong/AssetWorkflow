namespace cdc.BundleWorkFlow
{
    public interface IAssetBundleManager
    {
        bool AssetPathToBundleName(string assetPath, out string output);
        void SetAssetPathPair(string assetPath, string bundleName);
        void SetBundleVersion(string bundleName, string bundleVersion);
        bool CompareVersion(string bundleName, string intputVersion);
    }
}