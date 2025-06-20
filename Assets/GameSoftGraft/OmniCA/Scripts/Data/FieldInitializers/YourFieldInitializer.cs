using UnityEngine;

namespace GameSoftCraft
{
    [CreateAssetMenu(menuName = nameof(FieldInitializer) + "/Your", fileName = "Example" + nameof(FieldInitializer))]
    public class YourFieldInitializer : FieldInitializer
    {
        protected override void RefreshPixels (float[] pixData, int width) => throw new System.NotImplementedException();
    }
}
