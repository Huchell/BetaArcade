using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MinMax {

    [SerializeField] float _min = 0;
    [SerializeField] float _max = 1;

    public float Max
    {
        get
        {
            return _max;
        }
        set
        {
            _max = value;
        }
    }
    public float Min
    {
        get
        {
            return _min;
        }
        set
        {
            _min = value;
        }
    }

    public MinMax() { }
    public MinMax(float min, float max)
    {
        Min = min;
        Max = max;
    }

    public float Clamp(float value)
    {
        if (value >= Max)
            return Max;
        else if (value <= Min)
            return Min;
        else
            return value;
    }

    public float Map(float value, float min, float max)
    {
        return (((value - Min) * (max - min)) / (Max - Min)) + min;
    }
}
