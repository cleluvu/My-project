using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    private ItemDictionary itemDictionary;

    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount;
    public GameObject[] itemPrefab;

    public static InventoryController Instance {get; private set;}

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        itemDictionary = FindAnyObjectByType<ItemDictionary>();
    }

    public bool AddItem(GameObject groundItemObj)
    {
        Item groundItem = groundItemObj.GetComponent<Item>();
        if (groundItem == null) return false;

        // Tìm chỗ trống trong hotbar trước
        HotbarController hotbar = Object.FindFirstObjectByType<HotbarController>();
        
        if (hotbar != null && TryStackItemInPanel(groundItem, hotbar.hotbarPanel.transform)) return true;
        if (TryStackItemInPanel(groundItem, inventoryPanel.transform)) return true;

        // Tìm trong rương
        if (hotbar != null && TryAddToEmptySlotInPanel(groundItem, hotbar.hotbarPanel.transform)) return true;
        if (TryAddToEmptySlotInPanel(groundItem, inventoryPanel.transform)) return true;

        return false; 
    }

    // Tìm ô có đồ giống nhau để stack
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

    // Tìm ô trống
    private bool TryAddToEmptySlotInPanel(Item itemToAdd, Transform panelTransform)
    {
        foreach (Transform slotTransform in panelTransform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
            {
                ItemDictionary dict = Object.FindFirstObjectByType<ItemDictionary>();
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
                    return;
                }
            }
        }
        Debug.Log("Không tìm thấy item để trừ");
    }

    public List<InventorySaveData> GetInventoryItem()
    {
        // Lấy thông tin để lưu
        List<InventorySaveData> invData = new List<InventorySaveData>();
        foreach(Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if(slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                invData.Add(new InventorySaveData {itemID = item.ID, slotIndex = slotTransform.GetSiblingIndex(), quantity = item.quantity});
            }
        }
        return invData;
    }

    public void SetInventoryItem(List<InventorySaveData> inventorySaveData)
    {
        if (inventoryPanel == null)
        {
            InventoryPanelTag uiTag = FindAnyObjectByType<InventoryPanelTag>(FindObjectsInactive.Include);
            
            if (uiTag != null)
            {
                inventoryPanel = uiTag.gameObject;
            }
            else
            {
                Debug.LogError("Không tìm thấy InventoryPanelTag! Hãy chắc chắn bạn đã gắn script này vào Panel UI bên SampleScene.");
                return;
            }
        }


        // Xóa những gì còn xót lại
        foreach(Transform child in inventoryPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // Tạo slot mới
        for(int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, inventoryPanel.transform);
        }

        // Gắn slot với item
        foreach(InventorySaveData data in inventorySaveData)
        {
            if(data.slotIndex < slotCount)
            {
                Slot slot = inventoryPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>();
                GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);
                if(itemPrefab != null)
                {
                    GameObject item = Instantiate(itemPrefab, slot.transform);
                    item.transform.localScale = Vector3.one;
                    item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                    Item itemComponent = item.GetComponent<Item>();
                    itemComponent.SnapToSlot();
                    if(itemComponent != null && data.quantity > 1)
                    {
                        itemComponent.quantity = data.quantity;
                        itemComponent.UpdateQuantityDisplay();
                    }

                    slot.currentItem = item;
                }
            }
        }
    }

    public bool HasItem(int itemID)
    {
        Debug.Log("Check xem có thức ăn không");
        foreach(Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if(slot != null && slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if(item != null && item.ID == itemID && item.quantity > 0){
                    return true;
                }
            }
        }
        return false;
    }

    public bool HasItemGlobal(int itemID)
    {
        // Check kho đồ
        if (HasItem(itemID)) return true;

        // Check hotbar
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
        // Trừ trong kho đồ trước
        if (HasItem(itemID))
        {
            RemoveItem(itemID, amount);
            return;
        }

        // Trừ trong hotbar
        HotbarController hotbar = Object.FindFirstObjectByType<HotbarController>();
        if (hotbar != null)
        {
            foreach (Transform slotTransform in hotbar.hotbarPanel.transform)
            {
                Slot slot = slotTransform.GetComponent<Slot>();
                if (slot.currentItem != null)
                {
                    Item item = slot.currentItem.GetComponent<Item>();
                    if (item.ID == itemID)
                    {
                        for(int i = 0; i < amount; i++) item.ConsumeOne();
                        return;
                    }
                }
            }
        }
    }
}
