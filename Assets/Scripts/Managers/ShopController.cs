using UnityEngine;
using TMPro;

public class ShopController : MonoBehaviour
{
    public static ShopController Instance;

    [Header("UI")]
    public GameObject shopPanel;
    public Transform shopInventoryGrid, playerInventoryGrid;
    public GameObject shopSlotPrefab;
    public TMP_Text playerMoneyText, shopTitleText;

    private ItemDictionary itemDictionary;
    private ShopNPC currentShop;

    void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        itemDictionary = FindAnyObjectByType<ItemDictionary>();
        shopPanel.SetActive(false);
        if(CurrencyController.Instance != null)
        {
            CurrencyController.Instance.OnGoldChanged += UpdateMoneyDisplay;
            UpdateMoneyDisplay(CurrencyController.Instance.GetGold());
        }
    }

    private void UpdateMoneyDisplay(int amount)
    {
        if(playerMoneyText != null)
        {
            playerMoneyText.text = amount.ToString();
        }
    }

    public void OpenShop(ShopNPC shop)
    {
        currentShop = shop;
        shopPanel.SetActive(true);
        if(shopTitleText != null)
        {
            shopTitleText.text = shop.shopkeeperName + "'s Shop";
        }

        RefreshShopDisplay();
        RefreshPlayerInventoryDisplay();

        PauseController.SetPause(true);
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
        currentShop = null;
        PauseController.SetPause(false); 
    }

    public void RefreshShopDisplay()
    {
        if(currentShop == null) return;
        foreach(Transform child in shopInventoryGrid) Destroy(child.gameObject);

        foreach(var stockItem in currentShop.GetCurrentStock())
        {
            if(stockItem.quantity <= 0) continue;

            CreateShopSlot(shopInventoryGrid, stockItem.itemID, stockItem.quantity, true);
        }
    }

    public void RefreshPlayerInventoryDisplay()
    {
        foreach(Transform child in playerInventoryGrid) Destroy(child.gameObject);

        // Lấy đồ từ rương
        if(InventoryController.Instance != null)
        {
            foreach(Transform slotTransform in InventoryController.Instance.inventoryPanel.transform)
            {
                Slot inventorySlot = slotTransform.GetComponent<Slot>();
                if(inventorySlot?.currentItem != null)
                {
                    Item originalItem = inventorySlot.currentItem.GetComponent<Item>();
                    CreateShopSlot(playerInventoryGrid, originalItem.ID, originalItem.quantity, false, inventorySlot);
                }
            }
        }

        // Lấy đồ từ thanh công cụ
        HotbarController hotbar = Object.FindFirstObjectByType<HotbarController>();
        if (hotbar != null)
        {
            foreach(Transform slotTransform in hotbar.hotbarPanel.transform)
            {
                Slot hotbarSlot = slotTransform.GetComponent<Slot>();
                if(hotbarSlot?.currentItem != null)
                {
                    Item originalItem = hotbarSlot.currentItem.GetComponent<Item>();
                    CreateShopSlot(playerInventoryGrid, originalItem.ID, originalItem.quantity, false, hotbarSlot);
                }
            }
        }
    }

    private void CreateShopSlot(Transform grid, int itemID, int quantity, bool isShop, Slot originalSlot = null)
    {
        GameObject slotObj = Instantiate(shopSlotPrefab, grid);
        GameObject itemPrefab = itemDictionary.GetItemPrefab(itemID);
        if(itemPrefab == null) return;

        GameObject itemInstance= Instantiate(itemPrefab, slotObj.transform);
        
        Item item = itemInstance.GetComponent<Item>();
        if (item != null)
        {
            item.quantity = quantity;
            item.UpdateQuantityDisplay();
            item.SnapToSlot(); 
        }

        int price = isShop ? item.buyPrice : item.GetSellPrice();

        ShopSlot slot = slotObj.GetComponent<ShopSlot>();
        slot.isShopSlot = isShop;
        slot.SetItem(itemInstance, price);

        // ItemHandler
        ItemDragHandler dragHandler = itemInstance.GetComponent<ItemDragHandler>();
        if(dragHandler) dragHandler.enabled = false; // Tắt kéo thả khi đang trong Shop

        ShopItemHandler handler = itemInstance.AddComponent<ShopItemHandler>();
        handler.Initialize(isShop);
        if (!isShop)
        {
            handler.originalInventorySlot = originalSlot;
        }
    }

    public void AddItemToShop(int itemID, int quantity)
    {
        if(!currentShop) return;
        currentShop.AddToStock(itemID, quantity);
        RefreshShopDisplay();
    }

    public bool RemoveItemFromShop(int itemID, int quantity)
    {
        if(!currentShop) return false;
        bool success = currentShop.RemoveFromShopStock(itemID, quantity);
        if(success) RefreshShopDisplay();
        return success;
    }
}