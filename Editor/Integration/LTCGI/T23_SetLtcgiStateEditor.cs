#if LTCGI_INCLUDED && UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEditor;
using UnityEngine;

namespace Trigger2to3
{
    [CustomEditor(typeof(T23_SetLtcgiState))]
    internal class T23_SetLtcgiStateEditor : T23_ActionEditorBase
    {
        private static readonly string[] s_ModeOptions = { "Глобальный", "По экранам" };

        protected override void DrawFields()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("adapter"));

            var globalProp = serializedObject.FindProperty("global");
            int modeIdx = globalProp.boolValue ? 0 : 1;
            int newMode = EditorGUILayout.Popup("Режим", modeIdx, s_ModeOptions);
            if (newMode != modeIdx)
            {
                globalProp.boolValue = newMode == 0;
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            }

            DrawToggleOperationField();

            if (!globalProp.boolValue)
            {
                DrawRecieversList("screens");
            }
        }
    }
}
#endif
