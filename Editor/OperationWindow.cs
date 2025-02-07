using UnityEditor;
using UnityEngine;

namespace cdc.AssetWorkflow.Editor
{
    public class OperationWindow : EditorWindow
    {
        private GUILayoutOption m_maxWidth_100 = GUILayout.MaxWidth(100);
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

            EditorGUILayout.LabelField(MyStyles.GetContent("Settings"));
            EditorGUILayout.PropertyField(m_settingObj.FindProperty("rootPath"), MyStyles.GetContent("Root Path"));
            EditorGUILayout.PropertyField(m_settingObj.FindProperty("compressType"), MyStyles.GetContent("Compress Type"));

            if (MyStyles.Toggle(m_settingObj.FindProperty("enablePatch"), MyStyles.GetContent("Enable Patch")))
            {
                EditorGUILayout.PropertyField(m_settingObj.FindProperty("serverUrl"), MyStyles.GetContent("Server Url"));

                if (!MyStyles.Toggle(m_settingObj.FindProperty("useDefaultOutputPath"), MyStyles.GetContent("Use Default OutputPath")))
                {
                    EditorGUILayout.PropertyField(m_settingObj.FindProperty("customOutputPath"), MyStyles.GetContent("OutputPath"));
                }
            }


            m_settingObj.ApplyModifiedProperties();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(MyStyles.GetContent("Build"), m_maxWidth_100))
                Builder.NormalBuild();
            if (GUILayout.Button(MyStyles.GetContent("Clean"), m_maxWidth_100))
                Builder.Clean();
            EditorGUILayout.EndHorizontal();
        }
    }
}