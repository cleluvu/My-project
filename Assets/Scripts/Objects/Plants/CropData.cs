using System;
using UnityEngine;

public class CropData
{
    public bool isTilled;
    public string plantType;
    public int currentGrowState;
    public float growthTime;

    [NonSerialized] public GameObject plantGameObject;

    public CropData()
    {
        isTilled = true;
        plantType = "";
        currentGrowState = 0;
        growthTime = 0f;
        plantGameObject = null;
    }
}
