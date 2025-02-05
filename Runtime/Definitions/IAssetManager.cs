namespace cdc.AssetWorkflow
{
    public interface IAssetManager
    {
        bool AssetPathToBundleName(string assetPath, out string output);
        bool GetBundlePath(string bundleName, out string bundlePath);
        void SetBundlePath(string bundleName, string bundlePath);
        void SetAssetPathPair(string assetPath, string bundleName);
        void SetBundleVersion(string bundleName, string bundleVersion);
        bool CompareVersion(string bundleName, string intputVersion);
    }
}