using System;
using UnityEngine;

[Serializable]
public enum SoilState
{
    Normal,
    Tilled,
    Watered,
}

[Serializable]
public class FarmTileData
{
    public Vector3Int position;
    public SoilState state = SoilState.Normal;

    public String plantedCropID = "";
    public int currentGrowthStage = 0;
    public bool isCropped = false;
}

[Serializable]
public class FarmSaveData
{
    public FarmTileData[] tiles;
}