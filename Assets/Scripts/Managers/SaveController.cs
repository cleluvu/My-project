using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections;

public class SaveController : MonoBehaviour
{
    public static SaveController Instance;
    private string savePath;
    private InventoryController inventoryController;
    private HotbarController hotbarController;
    private FarmingController farmingController;
    private Chest[] chests;
    private ShopNPC[] shops;

    void Awake()
    {
        if (Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        savePath = Application.persistentDataPath + "/savegame.json";
        inventoryController = FindAnyObjectByType<InventoryController>();
        hotbarController = FindAnyObjectByType<HotbarController>();
        farmingController = FindAnyObjectByType<FarmingController>();
        chests = FindObjectsByType<Chest>(FindObjectsSortMode.None);
        shops = FindObjectsByType<ShopNPC>(FindObjectsSortMode.None);
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();

        // 1. Lưu vị trí Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) data.playerPosition = player.transform.position;

        // 2. Lưu thời gian
        DayAndNight dayNight = Object.FindFirstObjectByType<DayAndNight>();
        if (dayNight != null)
        {
            data.savedCurrentTime = dayNight.currentTime;
            data.savedDay = dayNight.day;
        }

        // 3. Lưu trạng thái rương kho báu
        List<ChestSaveData> chestSaveDatas = GetChestsState();
        if(chestSaveDatas != null) data.chestSaveDatas = chestSaveDatas;

        // 4. Lưu kho đồ chính
        if (inventoryController != null)
        {
            data.inventorySaveData = inventoryController.GetInventoryItem();
        }
        else
        {
            Debug.LogWarning("Không tìm thấy InventoryController để lưu!");
        }

        // 5. Lưu thanh hotbar
        if (hotbarController != null)
        {
            data.hotbarSaveData = hotbarController.GetHotbarItem();
        }
        else
        {
            Debug.LogWarning("Không tìm thấy hotbarController để lưu!");
        }

        // 6. Lưu nông trại
        if(farmingController != null)
        {
            data.farmTileSaveData = farmingController.GetFarmSaveData();
        }

        // 7. Lưu vật phẩm môi trường (đá, gỗ...)
        CollectedObject[] resources = FindObjectsByType<CollectedObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        data.resourceSaveData = new List<ResourceSaveData>();
        foreach(CollectedObject res in resources)
        {
            data.resourceSaveData.Add(res.GetSaveData());
        }

        // 8. Lưu các thực thể (Thú nuôi, quái...)
        Entity[] entities = FindObjectsByType<Entity>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        data.entitySaveData = new List<EntitySaveData>();
        foreach(Entity e in entities)
        {
            data.entitySaveData.Add(e.GetSaveData());
        }

        // 9. Lưu tiền vàng và trạng thái shop
        if (CurrencyController.Instance != null)
        {
            data.playerGold = CurrencyController.Instance.GetGold();
        }
        data.shopStates = GetShopStates();

        // 10. Lưu nhân viên làm thuê
        if (HireManager.Instance != null)
        {
            data.hiredAgentsData = HireManager.Instance.GetSaveData();
        }

        // 11. Lưu tiến độ Quest
        if (QuestController.Instance != null && QuestController.Instance.activateQuests != null)
        {
            data.questProgressData = QuestController.Instance.activateQuests;
        }

        // Chuyển thành JSON và lưu xuống ổ cứng
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        
        Debug.Log("Đã Save toàn bộ dữ liệu game thành công!");
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath)) return;

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // 1. Load vị trí Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) player.transform.position = data.playerPosition;

        // 2. Load thời gian trong game
        DayAndNight dayNight = Object.FindFirstObjectByType<DayAndNight>();
        if (dayNight != null)
        {
            dayNight.currentTime = data.savedCurrentTime;
            dayNight.day = data.savedDay;
        }

        // 3. Load trạng thái rương kho báu
        LoadChestState(data.chestSaveDatas);

        // 4. Load kho đồ chính 
        if (inventoryController != null)
        {
            inventoryController.SetInventoryItem(data.inventorySaveData);
        }

        // 5. Load thanh hotbar
        if (hotbarController != null)
        {
            hotbarController.SetHotbarItem(data.hotbarSaveData);
        }

        // 6. Load nông trại
        if(farmingController != null && data.farmTileSaveData != null)
        {
            farmingController.RestoreFarmData(data.farmTileSaveData);
        }

        // 7. Load vật phẩm môi trường tái sinh
        CollectedObject[] resources = FindObjectsByType<CollectedObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if(data.resourceSaveData != null)
        {
            foreach(CollectedObject obj in resources)
            {
                ResourceSaveData resData = data.resourceSaveData.Find(r => r.ID == obj.ID);
                if(resData != null) obj.RestoreData(resData);
            }
        }

        // 8. Load các thực thể (Thú nuôi, quái...)
        Entity[] entities = FindObjectsByType<Entity>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if(data.entitySaveData != null)
        {
            foreach(Entity e in entities)
            {
                EntitySaveData eData = data.entitySaveData.Find(x => x.ID == e.ID);
                if(eData != null) e.RestoreData(eData);
            }
        }

        // 9. Load tiền vàng và cửa hàng
        LoadShopStates(data.shopStates);
        if (CurrencyController.Instance != null)
        {
            CurrencyController.Instance.SetGold(data.playerGold);
        }

        // 10. Load nhân viên làm thuê 
        if (HireManager.Instance != null && data.hiredAgentsData != null && data.hiredAgentsData.Count > 0)
        {
            DockStation dock = Object.FindFirstObjectByType<DockStation>();
            HireManager.Instance.RestoreData(data.hiredAgentsData, dock);
        }

        // 11. Load tiến độ Quest
        if (QuestController.Instance != null && data.questProgressData != null)
        {
            QuestController.Instance.LoadQuestProgress(data.questProgressData);
        }

        Debug.Log("Đã tải dữ liệu (Load Game) thành công!");
    }

    private List<ShopInstanceData> GetShopStates()
    {
        List<ShopInstanceData> shopStates = new List<ShopInstanceData>();
        if (shops == null) return shopStates;

        foreach(var shop in shops)
        {
            ShopInstanceData shopData = new ShopInstanceData { shopID = shop.shopID, stock = new List<ShopItemData>() };
            foreach(var stockItem in shop.GetCurrentStock())
            {
                shopData.stock.Add(new ShopItemData { itemID = stockItem.itemID, quantity = stockItem.quantity });
            }
            shopStates.Add(shopData);
        }
        return shopStates;
    }

    private List<ChestSaveData> GetChestsState()
    {
        List<ChestSaveData> chestStates = new List<ChestSaveData>();
        if (chests == null) return chestStates;

        foreach(Chest chest in chests)
        {
            chestStates.Add(new ChestSaveData { chestID = chest.ChestID, isOpened = chest.IsOpened });
        }
        return chestStates;
    }

    private void LoadShopStates(List<ShopInstanceData> shopStates)
    {
        if(shopStates == null || shops == null) return;

        foreach(var shop in shops)
        {
            ShopInstanceData shopData = shopStates.FirstOrDefault(s => s.shopID == shop.shopID);
            if(shopData != null)
            {
                List<ShopNPC.ShopStockItem> loadedStock = new List<ShopNPC.ShopStockItem>();
                foreach(var itemData in shopData.stock)
                {
                    loadedStock.Add(new ShopNPC.ShopStockItem { itemID = itemData.itemID, quantity = itemData.quantity });
                }
                shop.SetStock(loadedStock);
            }
        }
    }

    private void LoadChestState(List<ChestSaveData> chestStates)
    {
        if (chestStates == null || chests == null) return;

        foreach(Chest chest in chests)
        {
            ChestSaveData chestSaveData = chestStates.FirstOrDefault(c => c.chestID == chest.ChestID);
            if(chestSaveData != null) chest.SetOpened(chestSaveData.isOpened);
        }
    }

    public void NewGame()
    {
        if (File.Exists(savePath)) File.Delete(savePath);
        
        if (CurrencyController.Instance != null) CurrencyController.Instance.SetGold(100); 

        DayAndNight dayNight = Object.FindFirstObjectByType<DayAndNight>();
        if (dayNight != null) { dayNight.day = 1; dayNight.currentTime = 0f; }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) player.transform.position = new Vector3(0, 0, 0); 

        if (inventoryController != null) inventoryController.SetInventoryItem(new List<InventorySaveData>());
        if (hotbarController != null) hotbarController.SetHotbarItem(new List<InventorySaveData>());

        // Làm trống tiến độ nhiệm vụ khi bắt đầu game mới hoàn toàn
        if (QuestController.Instance != null)
        {
            QuestController.Instance.LoadQuestProgress(new List<QuestProgress>());
        }

        SaveGame();
        Debug.Log("Đã khởi tạo dữ liệu cho Game Mới!");
    }

    public void StartNewGame() { StartCoroutine(NewGameRoutine()); }
    private IEnumerator NewGameRoutine()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("SampleScene");
        while (!asyncLoad.isDone) yield return null;
        yield return new WaitForEndOfFrame(); 
        FindAllReferences();
        NewGame();
    }

    public void StartContinueGame() { StartCoroutine(ContinueGameRoutine()); }
    private IEnumerator ContinueGameRoutine()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("SampleScene");
        while (!asyncLoad.isDone) yield return null;
        yield return new WaitForEndOfFrame(); 
        FindAllReferences();
        LoadGame();
    }

    private void FindAllReferences()
    {
        inventoryController = FindAnyObjectByType<InventoryController>(FindObjectsInactive.Include);
        hotbarController = FindAnyObjectByType<HotbarController>(FindObjectsInactive.Include);
        farmingController = FindAnyObjectByType<FarmingController>(FindObjectsInactive.Include);
        chests = FindObjectsByType<Chest>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        shops = FindObjectsByType<ShopNPC>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }
}