using UnityEngine;

[CreateAssetMenu(menuName = nameof(FieldInitializer) + "/Noise", fileName = "Noise" + nameof(FieldInitializer))]
public class NoiseFieldInitializer : FieldInitializer
{
    [SerializeField, Range(0, 1)]
    float _dencity;

    protected override void RefreshPixels (float[] pixData, int width) 
    {
        for (int i = 0; i < pixData.Length; i++) {
            var val = Random.value + _dencity * .125f;
            pixData[i] = Mathf.Floor(val);
        }
    }
}
