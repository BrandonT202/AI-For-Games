using UnityEngine;
using System.Collections;

public class FloatCondition : Condition
{
    private float minValue;
    private float maxValue;
    public float targetValue { get; set; }  // a pointer to the test data

    public FloatCondition(float min, float max)
    {
        minValue = min;
        maxValue = max;
    }

    public override bool Test()
    {
        return minValue <= targetValue && targetValue <= maxValue;
    }
}
