using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = nameof(FieldInitializer) + "/your", fileName = "Your" + nameof(FieldInitializer))]
public class YourFieldInitializer : FieldInitializer
{
    protected override void RefreshPixels (float[] pixData, int width) => throw new System.NotImplementedException();
}
