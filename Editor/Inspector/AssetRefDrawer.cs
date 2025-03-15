using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace cdc.AssetWorkflow.Editor
{
    [CustomPropertyDrawer(typeof(AssetRef))]
    internal class AssetRefDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var wrapper = new AssetRefWrapper(property);
            var asset = wrapper.GetAsset<UnityEngine.Object>();

            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(position, label, MyStyles.GetContent(""), EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            var newAsset = EditorGUI.ObjectField(position, MyStyles.GetContent("Asset"), asset, typeof(UnityEngine.Object), false);
            if (newAsset != asset)
            {
                asset = newAsset;
                wrapper.AssetPath = AssetDatabase.GetAssetPath(asset);
            }
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            string newPath = EditorGUI.TextField(position, MyStyles.GetContent("AssetPath"), wrapper.AssetPath);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.TextField(position, MyStyles.GetContent("AssetGUID"), wrapper.AssetGUID);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.EndDisabledGroup();

            if (!wrapper.AssetPath.StrEquals(newPath))
                wrapper.AssetPath = newPath;

            if (!IsValidAssetPath(wrapper.AssetPath))
            {
                Rect rect = position;
                Color originColor = GUI.contentColor;
                GUI.contentColor = Color.red;
                GUI.Label(rect, MyStyles.GetContent("Invalid Asset"));
                GUI.contentColor = originColor;
                rect.x += EditorGUIUtility.labelWidth;
                rect.width = 20;
                if (GUI.Button(rect, MyStyles.GetContent("!")))
                {
                    EditorUtility.DisplayDialog(
                        "Invalid Asset!",
                        $"Only support assets inside Assets/Resources or Assets/{EditorFileSystem.BundleRootPath}",
                        "OK"
                    );
                }
            }

            EditorGUI.indentLevel -= 1;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var wrapper = new AssetRefWrapper(property);
            int lines = 4;
            if (!IsValidAssetPath(wrapper.AssetPath))
            {
                lines += 1;
            }

            return lineHeight * lines;
        }

        private bool IsValidAssetPath(string assetPath)
        {
            return assetPath.StartsWith("Assets/Resources") ||
                assetPath.StartsWith($"Assets/{EditorFileSystem.BundleRootPath}");
        }

        private struct AssetRefWrapper
        {
            public SerializedProperty mainProp;
            public SerializedProperty assetPathProp;
            public SerializedProperty assetGUIDProp;

            public string AssetPath
            {
                get => assetPathProp.stringValue;
                set
                {
                    assetPathProp.stringValue = value;
                    assetGUIDProp.stringValue = AssetDatabase.AssetPathToGUID(value);

                    if (!string.IsNullOrEmpty(assetGUIDProp.stringValue))
                    {
                        UnityEngine.Object targetObj = mainProp.serializedObject.targetObject;
                        Type type = targetObj.GetType();
                        var methods = type.GetMethods(BindingFlags.Instance|BindingFlags.DeclaredOnly|BindingFlags.Public|BindingFlags.NonPublic)
                            .Where(m => m.GetCustomAttribute<OnAssetRefPathChangeAttribute>() != null);
                        var args = new object[] { value };
                        foreach (var method in methods)
                            method.Invoke(targetObj, args);
                    }
                }
            }

            public string AssetGUID => assetGUIDProp.stringValue;

            public AssetRefWrapper(SerializedProperty property)
            {
                mainProp = property;
                assetPathProp = property.FindPropertyRelative("m_assetPath");
                assetGUIDProp = property.FindPropertyRelative("m_assetGUID");
                assetPathProp.stringValue = AssetDatabase.GUIDToAssetPath(assetGUIDProp.stringValue);
            }

            public T GetAsset<T>()
                where T : UnityEngine.Object
            {
                return (T)AssetDatabase.LoadAssetAtPath(AssetPath, typeof(T));
            }
        }
    }
}