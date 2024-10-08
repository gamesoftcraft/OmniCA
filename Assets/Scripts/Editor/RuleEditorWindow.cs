using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Linq;
using System.Threading.Tasks;

public class RuleEditorWindow : EditorWindow
{
    const string PrefsKeyInstanceID = "RuleEditorWindow.InstanceID";
    const string PrefsKeyPropertyPath = "RuleEditorWindow.PropertyPath";
    const int StatesTotal = 512;

    Material _mat;
    RenderTexture _rt;
    Vector2 _scrollPos;
    GUISkin _skin;

    GUIStyle _labelHeaderStyles;
    SerializedProperty _ruleProperty;
    RuleBuilder[] _ruleSetters;

    string _folderPath;
    string _filePath;
    string _fileName;
    string[] _allFileNames;
    string[] _allRuleSettersTypeNames;
    int _fileIndex;
    int _ruleSetterIndex;
    int _searchMask;
    bool _isReady;

    public void SetReady () => _isReady = true;

    private void OnEnable ()
    {
        Initialize();
    }

    private async void Initialize ()
    {
        while (!_isReady) await Task.Yield();

        _skin = Resources.Load<GUISkin>("GUISkins/RuleWindowGUISkin");
        _labelHeaderStyles = _skin.FindStyle("label header");

        InitDisplay();

        if (_ruleProperty == null) {
            SetRuleProperty();
        }

        _folderPath = Application.streamingAssetsPath;
        _allFileNames = FindAllFilenames();
        _fileName = _allFileNames[0];
        _filePath = Path.Combine(_folderPath, _fileName + ".json");
        _allRuleSettersTypeNames = FindAllRuleSetters();
    }

    private void SetRuleProperty ()
    {
        int instId = EditorPrefs.GetInt(PrefsKeyInstanceID);
        string propPath = EditorPrefs.GetString(PrefsKeyPropertyPath);
        var targetObject = EditorUtility.InstanceIDToObject(instId);
        var serializedObject = new SerializedObject(targetObject);
        _ruleProperty = serializedObject.FindProperty(propPath);
    }

    private void InitDisplay ()
    {
        var shader = Shader.Find("Unlit/StateDisplay");
        if (shader != null && _mat == null) {
            _mat = new Material(shader);
        }
        if (_rt == null) {
            _rt = new RenderTexture(64, 64, 16);
        }
    }

    private void OnDisable ()
    {
        SaveRule(Path.Combine(_folderPath, "Latest.json"));

        if (_rt != null) {
            _rt.Release();
            DestroyImmediate(_rt);
        }

        if (_mat != null) {
            DestroyImmediate(_mat);
        }
    }

    private void OnGUI ()
    {
        if (_ruleProperty == null) return;

        void LeftSection ()
        {
            /// Left section
            GUILayout.Space(32);
            GUILayout.BeginVertical(GUILayout.Width(180));

            GUILayout.Label("File:", _labelHeaderStyles);

            if (GUILayout.Button("New Rule", _skin.button)) {
                RefreshRuleArray(new int[16]);
            }
            if (GUILayout.Button("Randomize", _skin.button)) {
                SetRandomRule();
            }
            if (GUILayout.Button("Load Rule", _skin.button)) {
                LoadRule();
            }
            if (GUILayout.Button("Save Rule", _skin.button)) {
                SaveRule(_filePath);
            }
            GUILayout.BeginHorizontal();
            string newFileName = GUILayout.TextField(_fileName, _skin.textField);
            int newFileIndex = EditorGUILayout.Popup(_fileIndex, _allFileNames, _skin.customStyles[5], GUILayout.Width(20));
            if (newFileIndex != _fileIndex) {
                _fileIndex = newFileIndex;
                newFileName = _allFileNames[_fileIndex];
            }
            if (newFileName != _fileName) {
                _filePath = Path.Combine(_folderPath, newFileName + ".json");
                _fileName = newFileName;
            }
            GUILayout.EndHorizontal();

            /// Display neigbourhood combinations that fit in the window
            GUILayout.Label("Table:", _labelHeaderStyles);

            int scrollY = (int)(1f / (StatesTotal * 74.6f) * StatesTotal * _scrollPos.y);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Width(180));
            for (int i = 0; i < StatesTotal; i++) {
                DisplayElement(i);
            }
            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        void RightSection ()
        {
            GUILayout.BeginVertical(GUILayout.Width(286f));
            
            DisplaySearch();

            /// Display rule setter scripts controls
            GUILayout.BeginVertical(_labelHeaderStyles);
            GUILayout.Label("Script:", _labelHeaderStyles);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Run")) {
                var rule = _ruleSetters[_ruleSetterIndex].CreateRule();
                for (int i = 0; i < rule.Length; i++) {
                    _ruleProperty.GetArrayElementAtIndex(i).intValue = rule[i];
                }
            }
            _ruleSetterIndex = EditorGUILayout.Popup(
                _ruleSetterIndex, _allRuleSettersTypeNames, EditorStyles.popup
                );
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            /// Display rule binary representations
            GUILayout.Label("Binary:", _labelHeaderStyles); //EditorStyles.boldLabel
            string binary = "";
            for (int i = 0; i < StatesTotal; i++) {
                binary += GetBit(i) ? 1 : 0;
            }
            GUILayout.Label(binary, _skin.label);

            /// Display rule integer representations
            GUILayout.Label("Integers:", _labelHeaderStyles);
            for (int i = 0; i < _ruleProperty.arraySize; i++) {
                var ruleVal = _ruleProperty.GetArrayElementAtIndex(i).intValue;
                GUILayout.Label(i + ":\t" + ruleVal.ToString(), _skin.label);
            }
            GUILayout.EndVertical();
        }

        if (_mat == null || _rt == null) InitDisplay();

        GUILayout.BeginHorizontal();
        LeftSection();
        RightSection();
        GUILayout.EndHorizontal();
    }

    private void DisplayElement (int idx)
    {
        _mat.SetInteger("_Mask", idx);

        RenderTexture.active = _rt;
        GL.Clear(true, true, Color.clear);
        Graphics.Blit(null, _rt, _mat);
        RenderTexture.active = null;

        GUILayout.BeginHorizontal(_skin.customStyles[3]);
        GUILayout.Label(idx.ToString(), GUILayout.Width(25));
        GUILayout.Label(_rt, GUILayout.Width(64), GUILayout.Height(64));
        GUILayout.Space(8f);

        SetBitSerializable(idx, GUILayout.Toggle(GetBit(idx), ""));
        
        GUILayout.EndHorizontal();
    }

    private void DisplaySearch ()
    {
        void Refresh ()
        {
            var dRect = GUILayoutUtility.GetLastRect();
            var evt = Event.current;
            var mousePos = evt.mousePosition;

            var isClick = evt.type == EventType.MouseDown && evt.button == 0;
            if (isClick && dRect.Contains(mousePos)) {
                var mouseInRect = new Vector2(
                   mousePos.x - dRect.x, mousePos.y - dRect.y
                );
                int gridClickX = (int)(mouseInRect.x / dRect.width * 3);
                int gridClickY = (int)((1 - mouseInRect.y / dRect.height) * 3);
                int clickId = 3 * gridClickY + gridClickX;

                /// toggle bit in _searchMask at clickId
                _searchMask ^= (1 << clickId);    
                Repaint();
            }
        }

        GUILayout.Label("Search:", _labelHeaderStyles);

        _mat.SetInteger("_Mask", _searchMask);

        RenderTexture.active = _rt;
        GL.Clear(true, true, Color.clear);
        Graphics.Blit(null, _rt, _mat);
        RenderTexture.active = null;

        GUILayout.BeginHorizontal(_skin.customStyles[3]);
        GUILayout.Label(_searchMask.ToString(), GUILayout.Width(25));
        GUILayout.Label(_rt, GUILayout.Width(64), GUILayout.Height(64));
        
        Refresh();
        
        GUILayout.Space(8f);
        GUILayout.BeginVertical();
        GUILayout.Label("State", EditorStyles.largeLabel);
        SetBitSerializable(_searchMask, GUILayout.Toggle(GetBit(_searchMask), ""));
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    public void SetEditorPrefs (int targetObjId, string propPath)
    {
        EditorPrefs.SetInt(PrefsKeyInstanceID, targetObjId);
        EditorPrefs.SetString(PrefsKeyPropertyPath, propPath);
    }

    private void SaveRule (string path)
    {
        string json = JsonUtility.ToJson(
            new RuleInfo(GetRuleArray())
            );
        File.WriteAllText(path, json);
        Debug.Log("Rule saved to " + path);
    }

    private void LoadRule ()
    {
        if (File.Exists(_filePath)) {
            string json = File.ReadAllText(_filePath);
            var ruleData = JsonUtility.FromJson<RuleInfo>(json);
            RefreshRuleArray(ruleData.ruleArray);

            Debug.Log("Rule loaded from " + _filePath);
        }
        else {
            Debug.LogWarning("Rule file not found at " + _filePath);
        }
    }

    private void SetRandomRule ()
    {
        System.Random rng = new System.Random();
        for (int i = 0; i < _ruleProperty.arraySize; i++) {
            _ruleProperty.GetArrayElementAtIndex(i).intValue = rng.Next(int.MinValue, int.MaxValue);
        }
    }

    private bool GetBit (int idx)
    {
        int arrayIdx = idx / 32;
        int bitIdx = idx % 32;
        var ruleElement = _ruleProperty.GetArrayElementAtIndex(arrayIdx);
        bool bit = ((ruleElement.intValue >> bitIdx) & 1) != 0;

        return bit;
    }

    private void SetBitSerializable (int idx, bool bit)
    {
        int arrayIdx = idx / 32;
        int bitNum = idx % 32;
        var ruleElement = _ruleProperty.GetArrayElementAtIndex(arrayIdx);
        if (bit) {
            ruleElement.intValue |= 1 << bitNum;
            return;
        }
        ruleElement.intValue &= ~(1 << bitNum);
        Apply();
    }

    private int[] GetRuleArray ()
    {
        var result = new int[_ruleProperty.arraySize];
        for (int i = 0; i < result.Length; i++) {
            result[i] = _ruleProperty.GetArrayElementAtIndex(i).intValue;
        }
        return result;
    }

    private void RefreshRuleArray (int[] rule)
    {
        for (int i = 0; i < _ruleProperty.arraySize; i++) {
            _ruleProperty.GetArrayElementAtIndex(i).intValue = rule[i];
        }
        Apply();
    }

    private void Apply ()
    {
        _ruleProperty.serializedObject.ApplyModifiedProperties();
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
            _fileIndex = 0;
            if (fileNames.Length > 0) {
                _fileName = fileNames[_fileIndex];
            }

            return fileNames;
        }
        else {
            Debug.LogError($"Directory not found: {_folderPath}");
            return new string[0];
        }
    }

    private string[] FindAllRuleSetters ()
    {
        var types = (
            from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
            from type in domainAssembly.GetTypes()
            where typeof(RuleBuilder).IsAssignableFrom(type)
            select type
            ).ToArray();

        types = types.Where(t => !t.IsAbstract).ToArray();

        _ruleSetters = new RuleBuilder[types.Count()];
        for ( int i = 0; i < types.Count(); i++ ) {
            _ruleSetters[i] = Activator.CreateInstance(types[i]) as RuleBuilder;
        }

        return types.Select(t => t.Name[..t.Name.IndexOf(nameof(RuleBuilder))]).ToArray();
    }
}
