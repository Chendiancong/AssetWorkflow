using UnityEditor;
using UnityEngine;

namespace cdc.BundleWorkFlow.Editor
{
    public class OperationWindow : EditorWindow
    {
        private GUIContent m_settingLabel = new GUIContent("Settings");
        private GUIContent m_rootPathContent = new GUIContent("Root Path");
        private GUIContent m_compressTypeContent = new GUIContent("Compress Type");
        private GUIContent m_buildContent = new GUIContent("Build");
        private GUIContent m_cleanContent = new GUIContent("Clean");
        private SerializedObject m_settingObj;

        [MenuItem("Bundle Workflow/Operation", priority = 100)]
        public static void ShowWindow()
        {
            OperationWindow window = GetWindow<OperationWindow>("Bundle Workflow");
            window.minSize = new Vector2(300, 400);
        }

        private void OnGUI()
        {
            if (m_settingObj == null)
                m_settingObj = new SerializedObject(BuildSettingAsset.Instance);
            m_settingObj.Update();

            EditorGUILayout.LabelField(m_settingLabel);
            EditorGUILayout.PropertyField(m_settingObj.FindProperty("rootPath"), m_rootPathContent);
            EditorGUILayout.PropertyField(m_settingObj.FindProperty("compressType"), m_compressTypeContent);
            m_settingObj.ApplyModifiedProperties();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(m_buildContent))
                Builder.NormalBuild();
            if (GUILayout.Button(m_cleanContent))
                Builder.Clean();
            EditorGUILayout.EndHorizontal();
        }
    }
}