using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CAManager.RuleData))]
public class RuleDataPropertyDrawer : PropertyDrawer
{
    string _fileName;
    string _folderPath;
    string[] _allFileNames;

    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
    {
        var target = property.serializedObject.targetObject;
        var amountProp = property.FindPropertyRelative("<Steps>k__BackingField");
        var fileIdxProp = property.FindPropertyRelative("_fileIndex");
        _folderPath = Application.streamingAssetsPath;
        if (_allFileNames == null || _allFileNames.Length == 0 ) {
            var allFileNames = FindAllFilenames();
            _allFileNames = new string[allFileNames.Length + 1];
            Array.Copy(allFileNames, 0, _allFileNames, 1, allFileNames.Length);
            _allFileNames[0] = "Use Latest";
        }
        GUILayout.BeginHorizontal();
        int newFileIndex = EditorGUILayout.Popup(fileIdxProp.intValue, _allFileNames);
        amountProp.intValue = EditorGUILayout.IntField(amountProp.intValue);
        GUILayout.EndHorizontal();

        if (newFileIndex != fileIdxProp.intValue) {
            var nameProp = property.FindPropertyRelative("<Name>k__BackingField");
            var ruleProp = property.FindPropertyRelative("<Rule>k__BackingField");
            var fileName = newFileIndex != 0 ? _allFileNames[newFileIndex] : "Latest";
            nameProp.stringValue = fileName;
            fileIdxProp.intValue = newFileIndex;
            
            _fileName = fileName;

            LoadRule(ruleProp, fileName);
        }
    }

    private string[] FindAllFilenames ()
    {
        if (Directory.Exists(_folderPath)) {
            string[] files = Directory.GetFiles(_folderPath, "*.json");
            string[] fileNames = new string[files.Length];
            /// Extract file names
            for (int i = 0; i < files.Length; i++) {
                fileNames[i] = Path.GetFileName(files[i]).Split('.')[0];
            }
            /// Set the initial selection to the first file if available
            if (fileNames.Length > 0) {
                _fileName = fileNames[0];
            }

            return fileNames;
        }
        else {
            Debug.LogError($"Directory not found: {_folderPath}");
            return new string[0];
        }
    }

    private void LoadRule (SerializedProperty ruleProp, string fileName)
    {
        var folderPath = Application.streamingAssetsPath;
        var path = Path.Combine(folderPath, fileName + ".json");
        if (File.Exists(path)) {
            string json = File.ReadAllText(path);
            var ruleData = JsonUtility.FromJson<RuleInfo>(json);
            var ruleSize = ruleData.ruleArray.Length;
            if (ruleProp.arraySize == 0) {
                ruleProp.arraySize = ruleSize;
            }
            for (int i = 0; i < ruleSize; i++) {
                var chunk = ruleProp.GetArrayElementAtIndex(i);
                chunk.intValue = ruleData.ruleArray[i];
            }

            Debug.Log("Rule loaded from " + path);
            
            return;
        }

        Debug.LogWarning("Rule file not found at " + path);
    }
}
