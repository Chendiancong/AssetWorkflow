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

            EditorGUI.PropertyField(position, wrapper.assetPathProp, MyStyles.GetContent("AssetPath"));
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

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
            int lines = 3;
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
            public SerializedProperty assetPathProp;

            public string AssetPath
            {
                get => assetPathProp.stringValue;
                set => assetPathProp.stringValue = value;
            }

            public AssetRefWrapper(SerializedProperty property)
            {
                assetPathProp = property.FindPropertyRelative("m_assetPath");
            }

            public T GetAsset<T>()
                where T : UnityEngine.Object
            {
                return (T)AssetDatabase.LoadAssetAtPath(assetPathProp.stringValue, typeof(T));
            }
        }
    }
}