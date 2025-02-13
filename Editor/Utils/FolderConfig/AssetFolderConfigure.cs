using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace cdc.AssetWorkflow.Editor
{
    internal static class AssetFolderConfigure
    {
        public static string FileNameSuffix => "_folderconfig.asset";

        [MenuItem("Assets/Asset Folder Configure", true)]
        private static bool ValidateConfigureFolder()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            Debug.Log($"{path}.StartsWith(Assets/{EditorFileSystem.BundleRootPath}) = {path.StartsWith($"Assets/{EditorFileSystem.BundleRootPath}")}");
            return path.StartsWith($"Assets/{EditorFileSystem.BundleRootPath}");
        }

        [MenuItem("Assets/Asset Folder Configure", false, 20)]
        private static void OpenWindow()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            AssetFolderConfigureWindow.ShowWindow(GetConfig(path));
        }

        private static AssetFolderConfigureAsset GetConfig(string path)
        {
            string configPath = $"{path}/{Path.GetFileName(path)}{FileNameSuffix}";
            var asset = AssetDatabase.LoadAssetAtPath<AssetFolderConfigureAsset>(configPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<AssetFolderConfigureAsset>();
                AssetDatabase.CreateAsset(asset, configPath);
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssetIfDirty(asset);
            }
            return asset;
        }
    }

    internal class AssetFolderConfigureWindow : EditorWindow
    {
        private SerializedObject m_asset;

        public static void ShowWindow(AssetFolderConfigureAsset asset)
        {
            var window = GetWindow<AssetFolderConfigureWindow>("Folder Configuration");
            window.minSize = new Vector2(400, 200);
            window.m_asset = new SerializedObject(asset);
        }

        private void OnGUI()
        {
            EditorGUILayout.PropertyField(m_asset.FindProperty("packingLevel"));
            m_asset.ApplyModifiedProperties();
        }
    }
}