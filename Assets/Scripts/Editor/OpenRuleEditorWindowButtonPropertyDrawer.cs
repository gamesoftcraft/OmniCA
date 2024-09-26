using System.Collections;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;


[CustomPropertyDrawer(typeof(OpenRuleEditorButtonAttribute))]
public class OpenRuleEditorWindowButtonPropertyDrawer : PropertyDrawer
{
    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
    {
        if (GUI.Button(position, "Open Rule Editor")) {
            var window = EditorWindow.GetWindow<RuleEditorWindow>("Rule Editor");
            var target = property.serializedObject;
            var ruleProp = target.FindProperty("_rule");

            window.SetEditorPrefs(target.targetObject.GetInstanceID(), ruleProp.propertyPath);
        }
    }
}
