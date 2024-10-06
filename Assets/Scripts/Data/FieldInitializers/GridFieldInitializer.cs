using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = nameof(FieldInitializer) + "/Grid", fileName = "Grid" + nameof(FieldInitializer))]
public class GridFieldInitializer : FieldInitializer
{
    [SerializeField, Range(0, 1)]
    float _area;

    [SerializeField, Min(2)]
    int _dencity = 2;

    protected override void RefreshPixels (float[] pixData, int width)
    {
        float area = 1f - _area;
        int min = (int)(width * width * area / 2);
        int max = pixData.Length - min;
        for (int i = min; i < max; i++) {
            int x = i % width;
            int y = i / width;
            int asdf = (int)(width / 2 * area);
            if (x < asdf || x > width - asdf) continue;
            int hVal = y % _dencity > 0 ? 0 : 1;
            int vVal = x % _dencity > 0 ? 0 : 1;
            int val = hVal * vVal;
            pixData[i] = val;
        }
    }
}
