using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveController : MonoBehaviour
{
    public static SaveController Instance;

    private string savePath;
    private InventoryController inventoryController;

    private FarmingController farmingController;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        savePath = Application.persistentDataPath + "/savegame.json";
        inventoryController = FindAnyObjectByType<InventoryController>();
        farmingController = FindAnyObjectByType<FarmingController>();
    }
    void Start()
    {
        LoadGame();
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();

        // Lưu vị trí Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) data.playerPosition = player.transform.position;

        // Lưu thời gian
        DayAndNight dayNight = Object.FindFirstObjectByType<DayAndNight>();
        if (dayNight != null)
        {
            data.savedCurrentTime = dayNight.currentTime;
            data.savedDay = dayNight.day;
        }

        // Lưu kho đồ
        List<InventorySaveData> inventorySaveData = inventoryController.GetInventoryItem();
        if(inventorySaveData != null)
        {
            data.inventorySaveData = inventorySaveData;
        }

        // Lưu nông trại
        if(farmingController != null)
        {
            data.farmTileSaveData = farmingController.GetFarmSaveData();
        }

        // Lưu mấy object có thể bị phá hủy và spawn lại
        CollectedObject[] resources = FindObjectsByType<CollectedObject>(FindObjectsSortMode.None);
        data.resourceSaveData = new List<ResourceSaveData>();
        foreach(CollectedObject res in resources)
        {
            data.resourceSaveData.Add(res.GetSaveData());
        }

        // Chuyển thành JSON và lưu file
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Đã Save vị trí và thời gian!");
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath)) return;

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // Load vị trí Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) player.transform.position = data.playerPosition;

        // Load thời gian (MỚI)
        DayAndNight dayNight = Object.FindFirstObjectByType<DayAndNight>();
        if (dayNight != null)
        {
            dayNight.currentTime = data.savedCurrentTime;
            dayNight.day = data.savedDay;
        }

        // Load kho đồ
        inventoryController.SetInventoryItem(data.inventorySaveData);

        // Load nông trại
        if(farmingController != null)
        {
            farmingController.RestoreFarmData(data.farmTileSaveData);
        }

        // Load mấy object có thể bị phá hủy và spawn lại
        CollectedObject[] resources = FindObjectsByType<CollectedObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if(data.resourceSaveData != null)
        {
            foreach(CollectedObject obj in resources)
            {
                ResourceSaveData saveData = data.resourceSaveData.Find(r => r.ID == obj.ID);
                if(saveData != null)
                {
                    obj.RestoreData(saveData);
                }
            }
        }

        Debug.Log("Đã Load thành công!");
    }
}