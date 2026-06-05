using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HotbarController : MonoBehaviour
{
    [Header("Setting")]
    public GameObject hotbarPanel;
    public GameObject slotPrefab;
    public int slotCount = 8;

    [Header("Minecraft Mechanic")]
    public int selectedSlotIndex = 0;

    [Header("Highlight Settings")]
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;

    private ItemDictionary itemDictionary;

    private Key[] hotbarKeys;
    void Awake()
    {
        itemDictionary = FindAnyObjectByType<ItemDictionary>();
        hotbarKeys = new Key[slotCount];
        for(int i = 0; i < slotCount; i++)
        {
            hotbarKeys[i] = i < 9 ? (Key)((int)Key.Digit1 + i) : Key.Digit0;
        }
    }

    void Update()
    {
        for(int i = 0; i < slotCount; i++)
        {
            if (Keyboard.current[hotbarKeys[i]].wasPressedThisFrame)
            {
                SelectSlot(i);
            }
        }
    }

    void SelectSlot(int idx)
    {
        selectedSlotIndex = idx;
        
        for (int i = 0; i < slotCount; i++)
        {
            UnityEngine.UI.Image slotImage = hotbarPanel.transform.GetChild(i).GetComponent<UnityEngine.UI.Image>();
            
            if (slotImage != null)
            {
                slotImage.color = (i == selectedSlotIndex) ? highlightColor : normalColor;
            }
        }

        Debug.Log("Đang cầm vật phẩm ở ô số: " + (idx + 1));
    }

    public Item GetEquippedItem()
    {
        Slot slot = hotbarPanel.transform.GetChild(selectedSlotIndex).GetComponent<Slot>();
        if(slot.currentItem != null)
        {
            return slot.currentItem.GetComponent<Item>();
        }
        return null;
    }

    public List<InventorySaveData> GetHotbarItem()
    {
        List<InventorySaveData> hotbarData = new List<InventorySaveData>();
        foreach(Transform slotTransform in hotbarPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if(slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                hotbarData.Add(new InventorySaveData {itemID = item.ID, slotIndex = slotTransform.GetSiblingIndex(), quantity = item.quantity});
            }
        }
        return hotbarData;
    }

    public void SetHotbarItem(List<InventorySaveData> inventorySaveData)
    {
        if (hotbarPanel == null)
        {
            InventoryPanelTag uiTag = FindAnyObjectByType<InventoryPanelTag>(FindObjectsInactive.Include);
            if (uiTag != null) hotbarPanel = uiTag.gameObject;
            else return; 
        }

        for (int i = 0; i < slotCount; i++)
        {
            Slot slot = hotbarPanel.transform.GetChild(i).GetComponent<Slot>();
            if (slot.currentItem != null)
            {
                Destroy(slot.currentItem);
                slot.currentItem = null; 
            }
        }

        foreach(InventorySaveData data in inventorySaveData)
        {
            if(data.slotIndex < slotCount)
            {
                Slot slot = hotbarPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>();
                GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);
                if(itemPrefab != null)
                {
                    GameObject item = Instantiate(itemPrefab, slot.transform);

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
}
