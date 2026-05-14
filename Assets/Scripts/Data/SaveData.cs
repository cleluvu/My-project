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
public class EntitySaveData
{
    public string ID;
    public Vector3 position;
    public int daysSinceLastDrop;
    public int daysFedSinceLastDrop;
    public bool isHungry;
}

[System.Serializable]
public class ChestSaveData
{
    public string chestID;
    public bool isOpened;
}

[System.Serializable]
public class ShopInstanceData
{
    public string shopID;
    public List<ShopItemData> stock = new();
}

[System.Serializable]
public class ShopItemData
{
    public int itemID;
    public int quantity;
}

[System.Serializable]
public class AgentSaveData
{
    public string employeeID;
    public int remainingDays;
    public Vector3 position;
}


[System.Serializable]
public class SaveData
{
    public Vector3 playerPosition;

    // Lưu kho đồ
    public List<InventorySaveData> inventorySaveData;

    // Lưu hotbar
    public List<InventorySaveData> hotbarSaveData;

    public List<string> openedChestIDs = new List<string>();
    public float savedCurrentTime;
    public int savedDay;

    // Lưu trạng thái rương kho báu
    public List<ChestSaveData> chestSaveDatas;

    // Lưu nông trại
    public List<FarmTileData> farmTileSaveData = new List<FarmTileData>();

    // Lưu object được sinh lại sau khi phá hủy
    public List<ResourceSaveData> resourceSaveData = new List<ResourceSaveData>(); 

    // Lưu các thực thể trong game
    public List<EntitySaveData> entitySaveData = new List<EntitySaveData>();

    // Lưu shop
    public int playerGold;
    public List<ShopInstanceData> shopStates = new ();

    // Lưu các nhân viên của nông trại
    public List<AgentSaveData> hiredAgentsData = new List<AgentSaveData>();
}
