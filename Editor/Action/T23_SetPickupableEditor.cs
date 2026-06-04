#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEditor;
using UnityEngine;

namespace Trigger2to3
{
    [CustomEditor(typeof(T23_SetPickupable))]
    internal class T23_SetPickupableEditor : T23_ActionEditorBase
    {
        protected override void DrawFields()
        {
            DrawRecieversList();

            var pickupableProp = serializedObject.FindProperty("pickupable");
            EditorGUILayout.PropertyField(pickupableProp, new GUIContent("Pickupable"));

            if (!pickupableProp.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("dropIfDisabling"), new GUIContent("Drop If Disabling"));
            }
        }
    }
}
#endif
