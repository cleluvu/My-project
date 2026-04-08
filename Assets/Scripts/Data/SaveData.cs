using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public Vector3 playerPosition;
    public int playerGold;

    public List<InventorySaveData> inventorySaveData = new List<InventorySaveData>();

    public List<string> openedChestIDs = new List<string>();
    public float savedCurrentTime;
    public int savedDay;
}