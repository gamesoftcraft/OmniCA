using System;
using System.Collections;
using UnityEngine;

public class CAManager : MonoBehaviour
{
    [Serializable]
    public struct RuleData
    {
        [field: SerializeField]
        public string Name { get; private set; }

        [field: SerializeField]
        public int Steps { get; private set; }

        [field: SerializeField, HideInInspector]
        public int[] Rule { get; set; }

        [SerializeField]
        int _fileIndex;
    }

    [Serializable]
    public struct RuleChain 
    {
        public enum Sequence { Order, Repeat, Random }

        [field: SerializeField]
        public Sequence Queue { get; private set; }

        [field: SerializeField]
        public RuleData[] Rules { get; private set; }

        public int StepNum { get; set; }

        public int Index { get; set; }

        public int Count => Rules.Length;
    }

    [SerializeField, HideInInspector]
    ComputeShader _program;

    [SerializeField, HideInInspector]
    RenderTexture _simulatuionField;

    [SerializeField]
    FieldInitializer _fieldInitializer;

    [SerializeField, Range(8, 2048)]
    int _resolution = 256;

    [SerializeField, HideInInspector]
    int[] _rule;

    [SerializeField]
    RuleChain _ruleChain;

    [SerializeField, OpenRuleEditorButton]
    bool _openRuleEditor;

    [SerializeField, Min(0)]
    float _delay;

    [SerializeField]
    bool _pause, _step;

    [SerializeField]
    bool _restart;

    int _cSInitId, _cSStepId;

    int _cSTargetId, _csRuleId, _csWidthId;

    private void OnValidate ()
    {
        if (_rule.Length == 0) {
            _rule = new int[16];
        }
        if (_restart) {
            _restart = false;
            Initialize();
        }
        if (_step) {
            _step = false;
            Step();
        }
        if (_restart) {
            _restart = false;
            Initialize();
        }
        if (_simulatuionField == null) {
            _simulatuionField = Resources.Load<RenderTexture>("SimFields/SimulationField");
        }
        if (_program == null) {
            _program = Resources.Load<ComputeShader>("GPU/SimulationCS");
        }
        if (!Application.isPlaying && _resolution != _simulatuionField.width) {
            _resolution = GetNextPow2(_resolution);
            _simulatuionField.Release();
            _simulatuionField.width = _resolution;
            _simulatuionField.height = _resolution;
            _simulatuionField.Create();
        }
    }

    private void Start ()
    {
        Initialize();
        StartCoroutine(UpdateDelayed());
        
    }

    private void Initialize ()
    {
        _cSInitId = _program.FindKernel("CSInit");
        _cSStepId = _program.FindKernel("CSStep");
        _cSTargetId = Shader.PropertyToID("Target");
        _csRuleId = Shader.PropertyToID("Rule");
        _csWidthId = Shader.PropertyToID("Resolution");

        if (_fieldInitializer == null) {
            _program.SetTexture(_cSInitId, _cSTargetId, _simulatuionField);
            _program.Dispatch(_cSInitId, _simulatuionField.width / 8, _simulatuionField.height / 8, 1);
            return;
        }

        _fieldInitializer.Apply(_simulatuionField);
        _ruleChain.Index = 0;
    }

    private void Step ()
    {
        if (_ruleChain.Count > 0) {
            var _mustSkip = _ruleChain.Queue == RuleChain.Sequence.Order;
            _mustSkip &= _ruleChain.Index >= _ruleChain.Count - 1;
            _mustSkip &= _ruleChain.Rules[_ruleChain.Index].Steps != 0;
            _mustSkip &= _ruleChain.StepNum >= _ruleChain.Rules[_ruleChain.Index].Steps;

            if (_mustSkip) return;
        }
        
        var ruleBuff = new ComputeBuffer(16, 32);
        var sourceTex = CopyTargetRT();
        int[] rule;

        void UpdateRuleIndex ()
        {
            if (_ruleChain.Queue == RuleChain.Sequence.Random) {
                _ruleChain.Index = UnityEngine.Random.Range(0, _ruleChain.Count);
                _ruleChain.StepNum = 0;
            }
            else {
                _ruleChain.Index++; 
                if (_ruleChain.Index >= _ruleChain.Count) {
                    if (_ruleChain.Queue == RuleChain.Sequence.Order) {
                        _ruleChain.Index = _ruleChain.Count - 1;
                    }
                    if (_ruleChain.Queue == RuleChain.Sequence.Repeat) {
                        _ruleChain.Index = 0;
                        _ruleChain.StepNum = 0;
                    }
                }
            }
        }

        if (_ruleChain.Rules.Length > 0) {
            /// Handle steps limit 
            var ruleData = _ruleChain.Rules[_ruleChain.Index];
            if (ruleData.Steps > 0) {
                if (_ruleChain.StepNum < ruleData.Steps) {
                    _ruleChain.StepNum++;
                }
                /// Max StepNum reached
                else if (_ruleChain.Index < _ruleChain.Count) {
                    _ruleChain.StepNum = 0;
                    UpdateRuleIndex();
                }
            }

            rule = _ruleChain.Rules[_ruleChain.Index].Rule;
        }
        /// RuleChain not set in the inspector
        else {
            rule = _rule;
        }

        ruleBuff.SetData(rule);

        _program.SetTexture(_cSStepId, "Source", sourceTex);
        _program.SetTexture(_cSStepId, _cSTargetId, _simulatuionField);
        _program.SetInt(_csWidthId, _simulatuionField.width);
        _program.SetBuffer(_cSStepId, _csRuleId, ruleBuff);
        _program.Dispatch(_cSStepId, _simulatuionField.width / 16, _simulatuionField.height / 16, 1);
        
        ruleBuff.Dispose();
    }

    private IEnumerator UpdateDelayed ()
    {
        yield return new WaitForSeconds(_delay);
        float t = 0;
        while (t < _delay) {
            if (!_pause) {
                t += Time.deltaTime;
            }
            yield return 1;
        }
        Step();
        StartCoroutine(UpdateDelayed());
    }

    private Color[] TexToArray ()
    {
        var copy = CopyTargetRT();
        var result = new Color[_simulatuionField.width * _simulatuionField.height];
        var pixels = copy.GetPixels();
        for (int i = 0; i < pixels.Length; i++) {
            result[i] = pixels[i];
        }

        return result;
    }

    private Texture2D CopyTargetRT ()
    {
        var tex = new Texture2D(_simulatuionField.width, _simulatuionField.height, TextureFormat.RFloat, false);
        RenderTexture.active = _simulatuionField;
        tex.ReadPixels(new Rect(0, 0, _simulatuionField.width, _simulatuionField.height), 0, 0);
        tex.Apply();
        RenderTexture.active = null;

        return tex;
    }

    private int GetNextPow2 (int v)
    {
        v--;
        v |= v >> 1;
        v |= v >> 2;
        v |= v >> 4;
        v |= v >> 8;
        v |= v >> 16;
        v++;

        return v;
    }
}
