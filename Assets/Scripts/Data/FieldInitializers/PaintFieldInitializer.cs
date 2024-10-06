using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = nameof(FieldInitializer) + "/Paint", fileName = "Paint" + nameof(FieldInitializer))]
public class PaintFieldInitializer : FieldInitializer
{
    [SerializeField]
    Texture2D _texture;

    [SerializeField]
    int _width;

    [SerializeField, OpenFieldEditorButton]
    bool _openFieldEditor;

    protected override void RefreshPixels (float[] pixData, int width) 
    {
        var pixels = _texture.GetPixels();
        for (int i = 0; i < pixData.Length; i++) {
            pixData[i] = pixels[i].r;
        }
    }

    public void SetTexture (Texture2D texture) => _texture = texture; 
}
