using System;
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

        public static bool ToggleField(SerializedProperty sprop, GUIContent content = null, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(sprop, content ?? null, options);
            // bool hasTooltip = !string.IsNullOrEmpty(sprop.tooltip);
            // if (hasTooltip && GUILayout.Button("?", GUILayout.Width(20)))
            //     EditorUtility.DisplayDialog("Tooltip", sprop.tooltip, "OK");
            EditorGUILayout.EndHorizontal();
            return sprop.boolValue;
        }

        public static T EnumField<T>(SerializedProperty sprop, GUIContent content = null, params GUILayoutOption[] options)
            where T : Enum
        {
            EditorGUILayout.PropertyField(sprop, content ?? null, options);
            int flag = sprop.enumValueFlag;
            if (Enum.IsDefined(typeof(T), flag))
                return (T)(ValueType)flag;
            else
                throw new Exception("error flag");
        }
    }
}