#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEditor;
using UnityEngine;

namespace Trigger2to3
{
    [CustomEditor(typeof(T23_SetAvatarScaling))]
    internal class T23_SetAvatarScalingEditor : T23_ActionEditorBase
    {
        private static readonly string[] s_ModeOptions = { "Конкретный рост", "Диапазон (player-controlled)" };

        protected override void DrawFields()
        {
            var modeProp = serializedObject.FindProperty("setSpecificHeight");
            int modeIdx = modeProp.boolValue ? 0 : 1;
            int newMode = EditorGUILayout.Popup("Режим", modeIdx, s_ModeOptions);
            if (newMode != modeIdx)
            {
                modeProp.boolValue = newMode == 0;
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            }

            if (modeProp.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("eyeHeight"), new GUIContent("Eye Height (м)"));
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("allowManualScaling"), new GUIContent("Allow Manual Scaling"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("minHeight"), new GUIContent("Min Height (м)"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxHeight"), new GUIContent("Max Height (м)"));
            }
        }
    }
}
#endif
