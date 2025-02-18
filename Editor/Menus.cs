using UnityEditor;
using UnityEngine;

namespace cdc.AssetWorkflow.Editor
{
    public static class Menus
    {
        [MenuItem("GameObject/Asset Workflow/Gameplay/Prefab")]
        public static void CreatePrefab()
        {
            GameObject go = new GameObject("PrefabLoader");
            go.AddComponent<cdc.AssetWorkflow.Prefab>();
        }

        [MenuItem("GameObject/Asset Workflow/UI/Image")]
        public static void CreateImage()
        {
            GameObject go = new GameObject("Image");
            go.AddComponent<cdc.AssetWorkflow.Image>();
        }

        [MenuItem("GameObject/Asset Workflow/UI/RawImage")]
        public static void CreateRawImage()
        {
            GameObject go = new GameObject("RawImage");
            go.AddComponent<cdc.AssetWorkflow.RawImage>();
        }

        [MenuItem("Asset Workflow/Operation")]
        public static void ShowOperationWindow()
        {
            OperationWindow.ShowWindow();
        }

        [MenuItem("Assets/Asset Workflow/Folder Configure", true)]
        public static bool ValidateAssetFolderConfigure() => AssetFolderConfigure.ValidateConfigureFolder();

        [MenuItem("Assets/Asset Workflow/Folder Configure")]
        public static void OpenAssetFolderConfigure() => AssetFolderConfigure.OpenWindow();
    }
}