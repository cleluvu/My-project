using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    private ItemDictionary itemDictionary;

    [Header("UI Panels")]
    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount;
    public GameObject[] itemPrefab;

    public static InventoryController Instance { get; private set; }

    public System.Action onInventoryChanged;
    private Dictionary<int, int> itemsCountCache = new Dictionary<int, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        itemDictionary = FindAnyObjectByType<ItemDictionary>();

        // TỰ ĐỘNG TÌM KHUNG HÒM ĐỒ
        if (inventoryPanel == null)
        {
            InventoryPanelTag uiTag = FindAnyObjectByType<InventoryPanelTag>(FindObjectsInactive.Include);
            if (uiTag != null)
            {
                inventoryPanel = uiTag.gameObject;
            }
            else
            {
                Debug.LogError($"[{nameof(InventoryController)}] Không tìm thấy Object nào gắn InventoryPanelTag trong Scene!");
            }
        }

        // TỰ ĐỘNG SINH SLOT TRỐNG KHI KHỞI CHẠY KHÔNG QUA FILE SAVE
        if (inventoryPanel != null && inventoryPanel.transform.childCount == 0)
        {
            for (int i = 0; i < slotCount; i++)
            {
                GameObject newSlot = Instantiate(slotPrefab, inventoryPanel.transform);
                newSlot.transform.localScale = Vector3.one;
            }
            Debug.Log($"[{nameof(InventoryController)}] Phát hiện hòm đồ trống! Đã tự động sinh {slotCount} ô Slot để chạy thử.");
        }

        // Khởi tạo cache số lượng ban đầu khi bắt đầu vào game
        RebuildItemCounts();
    }

    public void RebuildItemCounts()
    {
        itemsCountCache.Clear();

        // Quét vật phẩm trong Inventory Panel
        if (inventoryPanel != null)
        {
            foreach (Transform slotTransform in inventoryPanel.transform)
            {
                Slot slot = slotTransform.GetComponent<Slot>();
                if (slot != null && slot.currentItem != null)
                {
                    Item item = slot.currentItem.GetComponent<Item>();
                    if (item != null)
                    {
                        if (!itemsCountCache.ContainsKey(item.ID)) itemsCountCache[item.ID] = 0;
                        itemsCountCache[item.ID] += item.quantity;
                    }
                }
            }
        }

        // Quét vật phẩm trong Hotbar Panel
        HotbarController hotbar = Object.FindFirstObjectByType<HotbarController>();
        if (hotbar != null && hotbar.hotbarPanel != null)
        {
            foreach (Transform slotTransform in hotbar.hotbarPanel.transform)
            {
                Slot slot = slotTransform.GetComponent<Slot>();
                if (slot != null && slot.currentItem != null)
                {
                    Item item = slot.currentItem.GetComponent<Item>();
                    if (item != null)
                    {
                        if (!itemsCountCache.ContainsKey(item.ID)) itemsCountCache[item.ID] = 0;
                        itemsCountCache[item.ID] += item.quantity;
                    }
                }
            }
        }

        // Kích hoạt sự kiện thông báo cho QuestController cập nhật lại số lượng mục tiêu trên giao diện
        onInventoryChanged?.Invoke();
    }

    public Dictionary<int, int> GetItemCounts() => itemsCountCache;


    public bool AddItem(GameObject groundItemObj)
    {
        Item groundItem = groundItemObj.GetComponent<Item>();
        if (groundItem == null) return false;

        HotbarController hotbar = Object.FindFirstObjectByType<HotbarController>();
        
        // Ưu tiên gom cụm (Stack) vào đồ trùng loại ở Hotbar trước
        if (hotbar != null && TryStackItemInPanel(groundItem, hotbar.hotbarPanel.transform)) 
        {
            RebuildItemCounts();
            return true;
        }
        
        // Ưu tiên gom cụm (Stack) vào đồ trùng loại ở Hòm chính trước
        if (TryStackItemInPanel(groundItem, inventoryPanel.transform)) 
        {
            RebuildItemCounts();
            return true;
        }


        // Nếu không trùng loại, ưu tiên tìm ô trống ở Hòm chính (Inventory) để nhét vào trước
        if (TryAddToEmptySlotInPanel(groundItem, inventoryPanel.transform)) 
        {
            RebuildItemCounts();
            return true;
        }

        // Khi Hòm chính đã đầy sạch ô trống, lúc này mới tràn xuống ô trống của Hotbar
        if (hotbar != null && TryAddToEmptySlotInPanel(groundItem, hotbar.hotbarPanel.transform)) 
        {
            RebuildItemCounts();
            return true;
        }

        return false; 
    }

    // Duyệt tìm ô chứa vật phẩm trùng ID để cộng dồn số lượng vào Stack có sẵn
    private bool TryStackItemInPanel(Item itemToAdd, Transform panelTransform)
    {
        foreach (Transform slotTransform in panelTransform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem != null)
            {
                Item existingItem = slot.currentItem.GetComponent<Item>();
                if (existingItem.ID == itemToAdd.ID) 
                {
                    existingItem.AddToStack(itemToAdd.quantity);
                    return true;
                }
            }
        }
        return false;
    }

    // Duyệt tìm ô trống hoàn toàn đầu tiên để sinh ra một UI Item con mới
    private bool TryAddToEmptySlotInPanel(Item itemToAdd, Transform panelTransform)
    {
        foreach (Transform slotTransform in panelTransform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
            {
                ItemDictionary dict = Object.FindFirstObjectByType<ItemDictionary>();
                if (dict == null) return false;

                GameObject uiPrefab = dict.GetItemPrefab(itemToAdd.ID);
                if (uiPrefab != null)
                {
                    GameObject newUIItem = Instantiate(uiPrefab, slot.transform);
                    Item uiItemScript = newUIItem.GetComponent<Item>();
                    
                    if (uiItemScript != null)
                    {
                        uiItemScript.quantity = itemToAdd.quantity;
                        uiItemScript.UpdateQuantityDisplay();
                        uiItemScript.SnapToSlot();
                    }
                    
                    slot.currentItem = newUIItem;
                    return true;
                }
            }
        }
        return false;
    }

    // Trừ bớt số lượng vật phẩm trong kho đồ chính.
    public void RemoveItem(int itemID, int amount)
    {
        foreach(Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if(slot != null && slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if(item != null && item.ID == itemID)
                {
                    item.RemoveFromStack(amount);

                    if(item.quantity <= 0)
                    {
                        Destroy(slot.currentItem);
                        slot.currentItem = null;
                    }
                    RebuildItemCounts(); // Tính toán lại số lượng sau khi tiêu hao đồ
                    return;
                }
            }
        }
        Debug.LogWarning("Không tìm thấy vật phẩm có ID tương ứng để trừ.");
    }

    // Xuất danh sách thông tin vật phẩm hiện tại để phục vụ việc Save Game.
    public List<InventorySaveData> GetInventoryItem()
    {
        List<InventorySaveData> invData = new List<InventorySaveData>();
        if (inventoryPanel == null) return invData;

        foreach(Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if(slot != null && slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item != null)
                {
                    invData.Add(new InventorySaveData {
                        itemID = item.ID, 
                        slotIndex = slotTransform.GetSiblingIndex(), 
                        quantity = item.quantity
                    });
                }
            }
        }
        return invData;
    }

    // Nạp dữ liệu từ danh sách khôi phục để vẽ lại hòm đồ khi Load Game.
    public void SetInventoryItem(List<InventorySaveData> inventorySaveData)
    {
        if (inventoryPanel == null)
        {
            InventoryPanelTag uiTag = FindAnyObjectByType<InventoryPanelTag>(FindObjectsInactive.Include);
            if (uiTag != null) inventoryPanel = uiTag.gameObject;
            else return;
        }

        // Xóa sạch các ô Slot cũ còn sót lại
        foreach(Transform child in inventoryPanel.transform) Destroy(child.gameObject);

        // Sinh lại bộ khung ô Slot trống cố định
        for(int i = 0; i < slotCount; i++) Instantiate(slotPrefab, inventoryPanel.transform);

        // Đổ dữ liệu vật phẩm chui vào làm con của từng ô Slot theo đúng chỉ số index lưu trữ
        foreach(InventorySaveData data in inventorySaveData)
        {
            if(data.slotIndex < slotCount)
            {
                Slot slot = inventoryPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>();
                if (slot == null || itemDictionary == null) continue;

                GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);
                if(itemPrefab != null)
                {
                    GameObject item = Instantiate(itemPrefab, slot.transform);
                    item.transform.localScale = Vector3.one;
                    item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                    Item itemComponent = item.GetComponent<Item>();
                    if(itemComponent != null)
                    {
                        itemComponent.SnapToSlot();
                        if (data.quantity > 1)
                        {
                            itemComponent.quantity = data.quantity;
                            itemComponent.UpdateQuantityDisplay();
                        }
                    }

                    slot.currentItem = item;
                }
            }
        }
        // Ép hệ thống tính toán lại bộ cache đếm số lượng phục vụ Quest sau khi khôi phục hòm đồ thành công
        RebuildItemCounts();
    }

    public bool HasItem(int itemID)
    {
        if (inventoryPanel == null) return false;
        foreach(Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if(slot != null && slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if(item != null && item.ID == itemID && item.quantity > 0) return true;
            }
        }
        return false;
    }

    public bool HasItemGlobal(int itemID)
    {
        if (HasItem(itemID)) return true;

        HotbarController hotbar = Object.FindFirstObjectByType<HotbarController>();
        if (hotbar != null)
        {
            var hotbarItems = hotbar.GetHotbarItem();
            return hotbarItems.Exists(x => x.itemID == itemID);
        }
        return false;
    }

    public void RemoveItemGlobal(int itemID, int amount)
    {
        if (HasItem(itemID))
        {
            RemoveItem(itemID, amount);
            return;
        }

        HotbarController hotbar = Object.FindFirstObjectByType<HotbarController>();
        if (hotbar != null)
        {
            foreach (Transform slotTransform in hotbar.hotbarPanel.transform)
            {
                Slot slot = slotTransform.GetComponent<Slot>();
                if (slot != null && slot.currentItem != null)
                {
                    Item item = slot.currentItem.GetComponent<Item>();
                    if (item != null && item.ID == itemID)
                    {
                        for(int i = 0; i < amount; i++) item.ConsumeOne();
                        RebuildItemCounts(); // Làm mới lại cache
                        return;
                    }
                }
            }
        }
    }
}