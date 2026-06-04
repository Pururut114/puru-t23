#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEngine;
using UnityEditor;

namespace Trigger2to3
{
    [CustomEditor(typeof(T23_ConditionalTrigger))]
    internal class T23_ConditionalTriggerEditor : T23_TriggerEditorBase
    {
        T23_ConditionalTrigger body;

        public enum CompParameterType
        {
            Constant = 0,
            PropertyBox = 1,
            DifferenceFromBefore = 2
        }

        private string[] CompOperator_a = { "Equal (=)", "Not Equal (!=)", "Greater (>)", "Less (<)", "Greater or Equal (>=)", "Less or Equal (<=)" };
        private string[] CompOperator_b = { "Equal (=)", "Not Equal (!=)" };

        protected override void DrawFields()
        {
            body = target as T23_ConditionalTrigger;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("passive"));
            if (!body.passive)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("allowContinuity"));
            }

            GUILayout.Space(10);
            EditorGUILayout.LabelField("База", EditorStyles.boldLabel);
            serializedObject.FindProperty("basePropertyBox").objectReferenceValue = EditorGUILayout.ObjectField("PropertyBox", body.basePropertyBox, typeof(T23_PropertyBox), true);

            if (body.basePropertyBox)
            {
                GUILayout.Space(10);
                EditorGUILayout.LabelField("Сравнение", EditorStyles.boldLabel);
                serializedObject.FindProperty("compOperator").intValue = EditorGUILayout.Popup("Оператор", body.compOperator, (body.basePropertyBox.valueType == 1 || body.basePropertyBox.valueType == 2) ? CompOperator_a : CompOperator_b);
                serializedObject.FindProperty("compParameterType").intValue = (int)(CompParameterType)EditorGUILayout.EnumPopup("Тип параметра", (CompParameterType)body.compParameterType);
                if (body.compParameterType == 0)
                {
                    switch (body.basePropertyBox.valueType)
                    {
                        case 0:
                            serializedObject.FindProperty("comp_b").boolValue = EditorGUILayout.Toggle("Значение", body.comp_b);
                            break;
                        case 1:
                            serializedObject.FindProperty("comp_i").intValue = EditorGUILayout.IntField("Значение", body.comp_i);
                            break;
                        case 2:
                            serializedObject.FindProperty("comp_f").floatValue = EditorGUILayout.FloatField("Значение", body.comp_f);
                            break;
                        case 4:
                            serializedObject.FindProperty("comp_s").stringValue = EditorGUILayout.TextField("Значение", body.comp_s);
                            break;
                    }
                }
                if (body.compParameterType == 1)
                {
                    serializedObject.FindProperty("compPropertyBox").objectReferenceValue = EditorGUILayout.ObjectField("PropertyBox", body.compPropertyBox, typeof(T23_PropertyBox), true);
                    if (body.compPropertyBox)
                    {
                        if (body.compPropertyBox.valueType != body.basePropertyBox.valueType)
                        {
                            EditorGUILayout.HelpBox("Тип значения PropertyBox несовместим", MessageType.Error);
                        }
                    }
                }
            }
        }
    }
}
#endif
