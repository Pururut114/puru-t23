#if UNITY_EDITOR && !COMPILER_UDONSHARP
using UnityEditor;

namespace Trigger2to3
{
    [CustomEditor(typeof(T23_PickupHaptic))]
    internal class T23_PickupHapticEditor : T23_ActionEditorBase
    {
        protected override void DrawFields()
        {
            DrawRecieversList();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("duration"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("amplitude"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("frequency"));
        }
    }
}
#endif
