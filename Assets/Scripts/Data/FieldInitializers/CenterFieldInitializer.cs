using UnityEngine;

namespace GameSoftCraft
{
    [CreateAssetMenu(menuName = nameof(FieldInitializer) + "/Center", fileName = "Center" + nameof(FieldInitializer))]
    public class CenterFieldInitializer : FieldInitializer
    {
        protected override void RefreshPixels (float[] pixData, int width)
        {
            pixData[pixData.Length - width * (width / 2) + width / 2] = 1f;
        }
    }
}
