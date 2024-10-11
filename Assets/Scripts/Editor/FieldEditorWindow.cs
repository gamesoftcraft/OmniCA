using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameSoftCraft
{
    public class FieldEditorWindow : EditorWindow
    {
        public int Width = 512;

        Texture2D _texture;
        GUISkin _skin;

        string _folderPath;
        string _fileName;

        public PaintFieldInitializer Target { get; set; }

        private void OnEnable ()
        {
            _skin = Resources.Load<GUISkin>("GUISkins/RuleWindowGUISkin");
            _folderPath = "Assets/Resources";
            _folderPath = Path.Combine(_folderPath, "CustomFields");
            _fileName = "Untitled";
        }

        public void InitTexture ()
        {
            _texture = new Texture2D(Width, Width, TextureFormat.RGBA32, false);
            _texture.filterMode = FilterMode.Point;
            _texture.wrapMode = TextureWrapMode.Clamp;
            _texture.anisoLevel = 0;
            _texture.Apply();

            ResetTexture();
        }

        private void OnGUI ()
        {
            GUILayout.Label(_texture, GUILayout.Width(Width), GUILayout.Height(Width));

            var dRect = GUILayoutUtility.GetLastRect();
            var evt = Event.current;
            var mousePos = evt.mousePosition;
            var isClick = evt.type == EventType.MouseDrag;
            isClick |= evt.type == EventType.MouseDown;
            isClick &= evt.button == 0;
            if (isClick && dRect.Contains(mousePos)) {
                var mouseInRect = new Vector2Int(
                   (int)(mousePos.x - dRect.x),
                   Width - (int)(mousePos.y - dRect.y)
                );
                _texture.SetPixel(mouseInRect.x, mouseInRect.y, Color.white);
                _texture.Apply();
                Repaint();
            }

            GUILayout.BeginHorizontal();
            _fileName = GUILayout.TextField(_fileName, _skin.textField);
            if (GUILayout.Button("Save", _skin.button)) {
                Save();
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Reset", _skin.button)) {
                ResetTexture();
            }
        }

        private void ResetTexture ()
        {
            var pixels = new Color32[Width * Width];
            for (int i = 0; i < pixels.Length; i++) {
                pixels[i].a = 0xff;
            }

            _texture.SetPixels32(pixels);
            _texture.Apply();
        }

        private void Save ()
        {
            byte[] bytes = _texture.EncodeToPNG();
            string filePath = Path.Combine(_folderPath, _fileName + ".asset");
            AssetDatabase.CreateAsset(_texture, filePath);
            Target.SetTexture(_texture);

            Debug.Log("Field saved to " + filePath);
        }
    }
}
