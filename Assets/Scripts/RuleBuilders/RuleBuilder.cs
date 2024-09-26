using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

public abstract class RuleBuilder
{
    const int StatesTotal = 512;
    const int NeigborsNum = 9;

    public int[] CreateRule ()
    {
        var result = new int[16];
        for (int i = 0; i < StatesTotal; i++) {
            bool state = GetState(i) > 0 ? true : false;
            SetBit(result, i, state);
        }
        return result;
    }

    protected abstract int GetState (bool[] config);

    private int GetState (int config)
    {
        var cells = new bool[NeigborsNum];
        for (int i = 0; i < cells.Length; i++) {
            bool bit = ((config >> i) & 1) != 0;
            cells[i] = bit;
        }
        return GetState(cells);
    }

    private void SetBit (int[] rule, int idx, bool bit)
    {
        int arrayIdx = idx / 32;
        int bitNum = idx % 32;
        if (bit) {
            rule[arrayIdx] |= 1 << bitNum;
            return;
        }
        rule[arrayIdx] &= ~(1 << bitNum);
    }
}
