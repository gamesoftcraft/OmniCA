using UnityEngine;

namespace GameSoftCraft
{
    public abstract class FieldInitializer : ScriptableObject
    {
        protected abstract void RefreshPixels (float[] pixData, int width);

        public void Apply (RenderTexture target)
        {
            var tex = new Texture2D(target.width, target.height, TextureFormat.RFloat, false);
            float[] pixData = new float[target.width * target.height];

            RefreshPixels(pixData, target.width);

            tex.SetPixelData(pixData, 0, 0);
            tex.Apply();
            Graphics.Blit(tex, target);

            if (Application.isPlaying) {
                Destroy(tex);
            }
            else {
                DestroyImmediate(tex);
            }
        }
    }
}
