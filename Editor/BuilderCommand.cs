using UnityEditor;

namespace cdc.AssetWorkflow
{
    public struct BuilderCommand
    {
        public BuildTarget buildTarget;
        public BuildAssetBundleOptions options;

        public BuildTarget GetTrulyBuildTarget()
        {
            return buildTarget == BuildTarget.NoTarget ?
                EditorUserBuildSettings.activeBuildTarget :
                buildTarget;
        }
    }
}