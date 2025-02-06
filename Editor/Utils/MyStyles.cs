using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace cdc.AssetWorkflow
{
    internal static class MyStyles
    {
        private static Dictionary<string, GUIContent> m_contents;

        static MyStyles()
        {
            m_contents = new Dictionary<string, GUIContent>();
        }

        public static GUIContent GetContent(string label)
        {
            GUIContent content;
            if (!m_contents.TryGetValue(label, out content))
            {
                content = new GUIContent(label);
                m_contents[label] = content;
            }
            return content;
        }

        public static bool Toggle(SerializedProperty sprop, GUIContent content)
        {
            bool ret = sprop.boolValue = EditorGUILayout.Toggle(content, sprop.boolValue);
            return ret;
        }
    }
}