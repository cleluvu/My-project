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

        // Lưu trạng thái rương kho báu
        List<ChestSaveData> chestSaveDatas = GetChestsState();
        if(chestSaveDatas != null)
        {
            data.chestSaveDatas = chestSaveDatas;
        }

        // Lưu kho đồ
        List<InventorySaveData> inventorySaveData = inventoryController.GetInventoryItem();
        if (inventoryController != null)
        {
            data.inventorySaveData = inventoryController.GetInventoryItem();
        }
        else
        {
            Debug.LogWarning("Không tìm thấy InventoryController để lưu!");
        }

        // Lưu hotbar
        List<InventorySaveData> hotbarSaveData = hotbarController.GetHotbarItem();
        if (inventoryController != null)
        {
            data.hotbarSaveData = hotbarController.GetHotbarItem();
        }
        else
        {
            Debug.LogWarning("Không tìm thấy hotbarController để lưu!");
        }

        // Lưu nông trại
        if(farmingController != null)
        {
            data.farmTileSaveData = farmingController.GetFarmSaveData();
        }

        // Lưu mấy object có thể bị phá hủy và spawn lại
        CollectedObject[] resources = FindObjectsByType<CollectedObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        data.resourceSaveData = new List<ResourceSaveData>();
        foreach(CollectedObject res in resources)
        {
            data.resourceSaveData.Add(res.GetSaveData());
        }

        // Lưu entity trong game
        Entity[] entities = FindObjectsByType<Entity>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        data.entitySaveData = new List<EntitySaveData>();
        foreach(Entity e in entities)
        {
            data.entitySaveData.Add(e.GetSaveData());
        }

        // Lưu tiền vàng và cửa hàng
        int playerGold = CurrencyController.Instance.GetGold();
        List<ShopInstanceData> shopStates = GetShopStates();
        data.playerGold = playerGold;
        data.shopStates = shopStates;

        // Lưu nhân viên làm thuê
        if (HireManager.Instance != null)
        {
            data.hiredAgentsData = HireManager.Instance.GetSaveData();
        }

        // Lưu dân làng vào game
        NPCCoreSystems[] allNPCs = FindObjectsByType<NPCCoreSystems>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        data.npcSaveData = new List<NPCSaveData>();
        foreach(NPCCoreSystems npc in allNPCs)
        {
            data.npcSaveData.Add(npc.GetSaveData());
        }

        // Chuyển thành JSON và lưu file
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Đã Save vị trí và thời gian!");
        Debug.Log("Đường dẫn file save: " + Application.persistentDataPath);
    }

    private List<ShopInstanceData> GetShopStates()
    {
        List<ShopInstanceData> shopStates = new List<ShopInstanceData>();
        foreach(var shop in shops)
        {
            ShopInstanceData shopData = new ShopInstanceData
            {
                shopID = shop.shopID,
                stock = new List<ShopItemData>()
            };

            foreach(var stockItem in shop.GetCurrentStock())
            {
                shopData.stock.Add(new ShopItemData
                {
                    itemID = stockItem.itemID,
                    quantity = stockItem.quantity
                });
            }

            shopStates.Add(shopData);
        }

        return shopStates;
    }

    private List<ChestSaveData> GetChestsState()
    {
        List<ChestSaveData> chestStates = new List<ChestSaveData>();

        foreach(Chest chest in chests)
        {
            ChestSaveData chestSaveData = new ChestSaveData
            {
                chestID = chest.ChestID,
                isOpened = chest.IsOpened
            };

            chestStates.Add(chestSaveData);
        }

        return chestStates;
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

        // Load trạng thái rương kho báu
        LoadChestState(data.chestSaveDatas);

        // Load kho đồ
        inventoryController.SetInventoryItem(data.inventorySaveData);

        // Load hotbar
        hotbarController.SetHotbarItem(data.hotbarSaveData);

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

        // Load các thực thể trong game
        Entity[] entities = FindObjectsByType<Entity>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if(data.entitySaveData != null)
        {
            foreach(Entity e in entities)
            {
                EntitySaveData eData = data.entitySaveData.Find(x => x.ID == e.ID);
                if(eData != null) e.RestoreData(eData);
            }
        }

        // Load tiền vàng và cửa hàng
        LoadShopStates(data.shopStates);
        CurrencyController.Instance.SetGold(data.playerGold);

        // Load nhân viên làm thuê 
        if (HireManager.Instance != null && data.hiredAgentsData != null && data.hiredAgentsData.Count > 0)
        {
            DockStation dock = Object.FindFirstObjectByType<DockStation>();
            HireManager.Instance.RestoreData(data.hiredAgentsData, dock);
        }

        // Load dân làng vào game
        NPCCoreSystems[] allNPCs = FindObjectsByType<NPCCoreSystems>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (data.npcSaveData != null)
        {
            foreach(NPCCoreSystems npc in allNPCs)
            {
                // Khớp data bằng tên (ID)
                NPCSaveData npcData = data.npcSaveData.Find(n => n.npcID == npc.gameObject.name);
                if (npcData != null)
                {
                    npc.RestoreData(npcData);
                }
            }
        }

        Debug.Log("Đã Load thành công!");

        Debug.Log("Đã Load thành công!");
    }

    private void LoadShopStates(List<ShopInstanceData> shopStates)
    {
        if(shopStates == null) return;

        foreach(var shop in shops)
        {
            ShopInstanceData shopData = shopStates.FirstOrDefault(s => s.shopID == shop.shopID);

            if(shopData != null)
            {
                List<ShopNPC.ShopStockItem> loadedStock = new List<ShopNPC.ShopStockItem>();

                foreach(var itemData in shopData.stock)
                {
                    loadedStock.Add(new ShopNPC.ShopStockItem
                    {
                       itemID = itemData.itemID,
                       quantity = itemData.quantity 
                    });
                }

                shop.SetStock(loadedStock);
            }
        }
    }

    private void LoadChestState(List<ChestSaveData> chestStates)
    {
        foreach(Chest chest in chests)
        {
            ChestSaveData chestSaveData = chestStates.FirstOrDefault(c => c.chestID == chest.ChestID);

            if(chestSaveData != null)
            {
                chest.SetOpened(chestSaveData.isOpened);
            }
        }
    }

    public void NewGame()
    {
        // Xóa file save cũ 
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }
        
        // Đặt lại tiền vàng
        if (CurrencyController.Instance != null)
        {
            CurrencyController.Instance.SetGold(100); 
        }

        // Đặt lại thời gian
        DayAndNight dayNight = Object.FindFirstObjectByType<DayAndNight>();
        if (dayNight != null)
        {
            dayNight.day = 1;
            dayNight.currentTime = 0f;
        }

        // Đưa Player về vị trí xuất phát mặc định
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(0, 0, 0); 
        }

        // Khởi tạo rương đồ
        if (inventoryController != null)
        {
            List<InventorySaveData> startItems = new List<InventorySaveData>();
            inventoryController.SetInventoryItem(startItems);
        }
        else
        {
            Debug.Log("Lỗi khởi tạo rương đồ");
        }

        if(hotbarController != null)
        {
            List<InventorySaveData> hotbarDatas = new List<InventorySaveData>();
            hotbarController.SetHotbarItem(hotbarDatas);
        }

        SaveGame();
        
        Debug.Log("Đã khởi tạo Game Mới thành công!");
    }

    // Xử lý new game
    public void StartNewGame()
    {
        StartCoroutine(NewGameRoutine());
    }

    private IEnumerator NewGameRoutine()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("SampleScene");
        while (!asyncLoad.isDone) yield return null;
        
        yield return new WaitForEndOfFrame(); 

        FindAllReferences();
        NewGame();
    }

    // Xử lý continue game
    public void StartContinueGame()
    {
        StartCoroutine(ContinueGameRoutine());
    }

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