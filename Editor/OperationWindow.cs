using System.IO;
using Codice.Client.GameUI.Checkin.DifferencesApplier;
using UnityEditor;
using UnityEngine;

namespace cdc.AssetWorkflow.Editor
{
    public class OperationWindow : EditorWindow
    {
        private GUILayoutOption m_maxWidth_100 = GUILayout.MaxWidth(100);
        private SerializedObject m_settingObj;

        public static void ShowWindow()
        {
            OperationWindow window = GetWindow<OperationWindow>("Asset Workflow Operation");
            window.minSize = new Vector2(300, 400);
        }

        private void OnGUI()
        {
            if (m_settingObj == null)
                m_settingObj = new SerializedObject(BuildSettingAsset.Instance);
            m_settingObj.Update();

            EditorGUILayout.LabelField(MyStyles.GetContent("Settings"));
            EditorGUILayout.PropertyField(m_settingObj.FindProperty("rootPath"));
            EditorGUILayout.PropertyField(m_settingObj.FindProperty("compressType"));

            if (MyStyles.ToggleField(m_settingObj.FindProperty("enablePatch")))
            {
                EditorGUILayout.PropertyField(m_settingObj.FindProperty("serverUrl"));

                if (MyStyles.ToggleField(m_settingObj.FindProperty("useCustomOutputPath")))
                {
                    EditorGUILayout.BeginHorizontal();
                    var prop = m_settingObj.FindProperty("customOutputPath");
                    string input = prop.stringValue;
                    EditorGUILayout.PropertyField(prop, MyStyles.GetContent("Custom Path"));
                    if (GUILayout.Button(MyStyles.GetContent("Select Folder"), m_maxWidth_100))
                    {
                        string selectedPath = EditorUtility.OpenFolderPanel("Select Folder", Path.GetDirectoryName(input), "");
                        if (!string.IsNullOrEmpty(selectedPath) && !input.StrEquals(selectedPath))
                        {
                            input = selectedPath;
                            prop.stringValue = input;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            EnumOperation();

            m_settingObj.ApplyModifiedProperties();
        }

        private void EnumOperation()
        {
            var opType = MyStyles.EnumField<BuildSettingAsset.OperationType>(m_settingObj.FindProperty("operationType"));
            if (opType == BuildSettingAsset.OperationType.NoSelect)
                return;
            else
            {
                BuildTarget buildTarget = BuildTarget.NoTarget;
                EditorGUILayout.BeginHorizontal();
                if (MyStyles.ToggleField(m_settingObj.FindProperty("useCustomBuildTarget"), null))
                    buildTarget = MyStyles.EnumField<BuildTarget>(m_settingObj.FindProperty("buildTarget"));
                EditorGUILayout.EndHorizontal();
                if (GUILayout.Button(MyStyles.GetContent("Doit"), GUILayout.Width(100)))
                {
                    switch (opType)
                    {
                        case BuildSettingAsset.OperationType.NormalBuild:
                            Builder.BuildBundle(new BuilderCommand() {
                                buildTarget = buildTarget,
                            });
                            break;
                        case BuildSettingAsset.OperationType.ForceBuild:
                            Builder.BuildBundle(new BuilderCommand() {
                                buildTarget = buildTarget,
                                options = BuildAssetBundleOptions.ForceRebuildAssetBundle
                            });
                            break;
                        case BuildSettingAsset.OperationType.Clean:
                            Builder.Clean(new BuilderCommand() {
                                buildTarget = buildTarget
                            });
                            break;
                    }
                }
            }
        }
    }
}