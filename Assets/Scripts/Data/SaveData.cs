 using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ResourceSaveData
{
    public string ID;
    public float hp;
    public int dayDead;
}

[System.Serializable]
public class SaveData
{
    public Vector3 playerPosition;
    public int playerGold;

    public List<InventorySaveData> inventorySaveData;

    public List<string> openedChestIDs = new List<string>();
    public float savedCurrentTime;
    public int savedDay;

    // Lưu nông trại
    public List<FarmTileData> farmTileSaveData = new List<FarmTileData>();

    // Lưu object được sinh lại sau khi phá hủy
    public List<ResourceSaveData> resourceSaveData = new List<ResourceSaveData>(); 
}