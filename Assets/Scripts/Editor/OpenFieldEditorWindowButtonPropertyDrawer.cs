using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(OpenFieldEditorButtonAttribute))]
public class OpenFieldEditorWindowButtonPropertyDrawer : PropertyDrawer
{
    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
    {
        if (GUI.Button(position, "Open Field Editor")) {
            var window = EditorWindow.GetWindow<FieldEditorWindow>("Field Editor");
            var target = property.serializedObject;
            var widthProp = target.FindProperty("_width");
            window.Target = target.targetObject as PaintFieldInitializer;
            window.Width = widthProp.intValue;
            window.InitTexture();
        }
    }
}
