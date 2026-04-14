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

        // for(int i = 0; i < slotCount; i++)
        // {
        //     Slot slot = Instantiate(slotPrefab, inventoryPanel.transform).GetComponent<Slot>();
        //     if(i < itemPrefab.Length)
        //     {
        //         GameObject item = Instantiate(itemPrefab[i], slot.transform);
        //         item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        //         slot.currentItem = item;
        //     }
        // }
    }

    public bool AddItem(GameObject itemPrefab)
    {
        Item itemToAdd = itemPrefab.GetComponent<Item>();
        if(itemToAdd == null) return false;

        // Kiểm tra xem item đó có trong inventory chưa
        foreach(Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if(slot != null && slot.currentItem != null)
            {
                Item slotItem = slot.currentItem.GetComponent<Item>();
                if(slotItem != null && slotItem.ID == itemToAdd.ID)
                {
                    slotItem.AddToStack();
                    return true;
                }
            }
        }

        // Kiếm slot trống nào đó
        foreach(Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if(slot != null && slot.currentItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab, slot.transform);
                newItem.transform.localScale = Vector3.one;
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                slot.currentItem = newItem;
                return true;
            }
        }

        Debug.Log("Thêm item thất bại");
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
}
