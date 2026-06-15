using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class ShopController : MonoBehaviour
{
    public static ShopController Instance;

    [Header("Core UI")]
    public GameObject shopPanel;
    public Transform shopInventoryGrid, playerInventoryGrid;
    public GameObject shopSlotPrefab;
    public TMP_Text playerMoneyText, shopTitleText;

    [Header("Action Panel (New feature)")]
    public GameObject actionPanel; 
    public Button buyActionButton;
    public Button sellActionButton;

    [Header("Quantity Panel (New feature)")]
    public GameObject quantityPanel; 
    public TMP_Text quantityText;
    public Slider quantitySlider; 
    public Button confirmQuantityButton;

    [Header("Confirmation Panel (New feature)")]
    public GameObject confirmationPanel; 
    public TMP_Text confirmationText; 
    public Button confirmTransactionButton;
    public Button cancelTransactionButton;

    [Header("Animal Spawner Settings")]
    public Transform animalSpawnPoint;

    private ItemDictionary itemDictionary;
    private ShopNPC currentShop;

    private Item selectedItem;
    private ShopSlot selectedSlot;
    private Slot selectedOriginalInventorySlot;
    private bool isBuyingProcess;
    private int currentSelectedQuantity = 1;
    private int maxAllowedQuantity = 1;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        itemDictionary = FindAnyObjectByType<ItemDictionary>();
        
        shopPanel.SetActive(false);
        if (actionPanel != null) actionPanel.SetActive(false);
        if (quantityPanel != null) quantityPanel.SetActive(false);
        if (confirmationPanel != null) confirmationPanel.SetActive(false);

        if (CurrencyController.Instance != null)
        {
            CurrencyController.Instance.OnGoldChanged += UpdateMoneyDisplay;
            UpdateMoneyDisplay(CurrencyController.Instance.GetGold());
        }

        if (buyActionButton != null) buyActionButton.onClick.AddListener(OnBuyActionButtonClicked);
        if (sellActionButton != null) sellActionButton.onClick.AddListener(OnSellActionButtonClicked);
        if (confirmQuantityButton != null) confirmQuantityButton.onClick.AddListener(OnQuantityConfirmed);
        if (confirmTransactionButton != null) confirmTransactionButton.onClick.AddListener(ExecuteTransaction);
        if (cancelTransactionButton != null) cancelTransactionButton.onClick.AddListener(CancelTransaction);
        if (quantitySlider != null) quantitySlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void UpdateMoneyDisplay(int amount)
    {
        if (playerMoneyText != null)
        {
            playerMoneyText.text = amount.ToString();
        }
    }

    public void OpenShop(ShopNPC shop)
    {
        currentShop = shop;
        shopPanel.SetActive(true);
        ResetAllSubPanels();

        if (shopTitleText != null)
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
        ResetAllSubPanels();
        currentShop = null;
        PauseController.SetPause(false); 
    }

    private void ResetAllSubPanels()
    {
        if (actionPanel != null) actionPanel.SetActive(false);
        if (quantityPanel != null) quantityPanel.SetActive(false);
        if (confirmationPanel != null) confirmationPanel.SetActive(false);
        selectedItem = null;
        selectedSlot = null;
        selectedOriginalInventorySlot = null;
    }

    public void RefreshShopDisplay()
    {
        if (currentShop == null) return;
        foreach (Transform child in shopInventoryGrid) Destroy(child.gameObject);

        foreach (var stockItem in currentShop.GetCurrentStock())
        {
            if (stockItem.quantity <= 0) continue;
            CreateShopSlot(shopInventoryGrid, stockItem.itemID, stockItem.quantity, true);
        }
    }

    public void RefreshPlayerInventoryDisplay()
    {
        foreach (Transform child in playerInventoryGrid) Destroy(child.gameObject);

        if (InventoryController.Instance != null)
        {
            foreach (Transform slotTransform in InventoryController.Instance.inventoryPanel.transform)
            {
                Slot inventorySlot = slotTransform.GetComponent<Slot>();
                if (inventorySlot?.currentItem != null)
                {
                    Item originalItem = inventorySlot.currentItem.GetComponent<Item>();
                    CreateShopSlot(playerInventoryGrid, originalItem.ID, originalItem.quantity, false, inventorySlot);
                }
            }
        }

        HotbarController hotbar = Object.FindFirstObjectByType<HotbarController>();
        if (hotbar != null)
        {
            foreach (Transform slotTransform in hotbar.hotbarPanel.transform)
            {
                Slot hotbarSlot = slotTransform.GetComponent<Slot>();
                if (hotbarSlot?.currentItem != null)
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
        if (itemPrefab == null) return;

        GameObject itemInstance = Instantiate(itemPrefab, slotObj.transform);
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

        ItemDragHandler dragHandler = itemInstance.GetComponent<ItemDragHandler>();
        if (dragHandler) dragHandler.enabled = false; 

        ShopItemHandler handler = itemInstance.AddComponent<ShopItemHandler>();
        handler.Initialize(isShop);
        if (!isShop)
        {
            handler.originalInventorySlot = originalSlot;
        }
    }

    public void HandleItemSelection(Item item, ShopSlot slot, Slot originalInventorySlot, bool isShopItem)
    {
        selectedItem = item;
        selectedSlot = slot;
        selectedOriginalInventorySlot = originalInventorySlot;
        isBuyingProcess = isShopItem;

        if (actionPanel != null)
        {
            actionPanel.SetActive(true);
            if (buyActionButton != null) buyActionButton.gameObject.SetActive(isBuyingProcess);
            if (sellActionButton != null) sellActionButton.gameObject.SetActive(!isBuyingProcess);
        }
    }

    private void OnBuyActionButtonClicked()
    {
        if (selectedItem == null || selectedSlot == null) return;

        int currentGold = CurrencyController.Instance.GetGold();
        int unitPrice = selectedSlot.itemPrice;

        if (currentGold < unitPrice)
        {
            ShowWarningPopup("Bạn không đủ tiền để mua vật phẩm này!");
            return;
        }

        int maxByGold = currentGold / unitPrice;
        maxAllowedQuantity = Mathf.Min(maxByGold, selectedItem.quantity);

        SetupQuantityPanel();
    }

    private void OnSellActionButtonClicked()
    {
        if (selectedItem == null) return;
        maxAllowedQuantity = selectedItem.quantity;
        SetupQuantityPanel();
    }

    private void SetupQuantityPanel()
    {
        if (actionPanel != null) actionPanel.SetActive(false);

        currentSelectedQuantity = 1;
        if (quantityPanel != null)
        {
            quantityPanel.SetActive(true);
            if (quantitySlider != null)
            {
                quantitySlider.minValue = 1;
                quantitySlider.maxValue = maxAllowedQuantity;
                quantitySlider.value = 1;
            }
            UpdateQuantityTextDisplay();
        }
    }

    private void OnSliderValueChanged(float value)
    {
        currentSelectedQuantity = Mathf.RoundToInt(value);
        UpdateQuantityTextDisplay();
    }

    public void ChangeQuantityAmount(int amount)
    {
        currentSelectedQuantity = Mathf.Clamp(currentSelectedQuantity + amount, 1, maxAllowedQuantity);
        if (quantitySlider != null) quantitySlider.value = currentSelectedQuantity;
        UpdateQuantityTextDisplay();
    }

    private void UpdateQuantityTextDisplay()
    {
        if (quantityText != null)
        {
            quantityText.text = $"{currentSelectedQuantity} / {maxAllowedQuantity}";
        }
    }

    private void OnQuantityConfirmed()
    {
        if (quantityPanel != null) quantityPanel.SetActive(false);

        int totalCost = selectedSlot.itemPrice * currentSelectedQuantity;

        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(true);
            if (confirmTransactionButton != null) confirmTransactionButton.gameObject.SetActive(true);
            
            if (isBuyingProcess)
            {
                confirmationText.text = $"Bạn muốn mua {selectedItem.Name} với {totalCost} vàng?";
            }
            else
            {
                confirmationText.text = $"Bạn muốn bán {selectedItem.Name} với {totalCost} vàng?";
            }
        }
    }

    private void ExecuteTransaction()
    {
        if (selectedItem == null || selectedSlot == null) return;

        int totalCost = selectedSlot.itemPrice * currentSelectedQuantity;

        if (isBuyingProcess)
        {
            if (CurrencyController.Instance.GetGold() < totalCost)
            {
                ShowWarningPopup("Giao dịch thất bại! Bạn không đủ tiền.");
                return;
            }

            if (selectedItem.itemType == ItemType.Animal)
            {
                // Kiểm tra xem Item này đã được kéo gán Prefab con vật thực thể thật chưa
                if (selectedItem.animalEntityPrefab != null)
                {
                    // Trừ tiền vàng, trừ số lượng trong cửa hàng
                    CurrencyController.Instance.SpendGold(totalCost);
                    RemoveItemFromShop(selectedItem.ID, currentSelectedQuantity);

                    // Gọi hàm kích hoạt Spawn thực thể ra ngoài Map
                    SpawnAnimalToWorld(selectedItem.animalEntityPrefab, currentSelectedQuantity);
                }
                else
                {
                    Debug.LogError($"[Lỗi Hệ Thống Shop] Thẻ Item '{selectedItem.Name}' chưa được kéo gán Prefab Thực Thể thật vào ô Animal Entity Prefab!");
                    ShowWarningPopup("Mua thất bại! Vật phẩm gia súc chưa được cấu hình.");
                    return;
                }
            }
            else
            {
                // KHÔNG PHẢI ĐỘNG VẬT: Chạy cơ chế cũ nhét vào túi hòm đồ
                GameObject dummyRewardObj = new GameObject("DummyShopItem");
                Item dummyItem = dummyRewardObj.AddComponent<Item>();
                dummyItem.ID = selectedItem.ID;
                dummyItem.quantity = currentSelectedQuantity;

                if (InventoryController.Instance.AddItem(dummyRewardObj))
                {
                    CurrencyController.Instance.SpendGold(totalCost);
                    RemoveItemFromShop(selectedItem.ID, currentSelectedQuantity);
                }
                else
                {
                    ShowWarningPopup("Hòm đồ của bạn đã đầy!");
                }
                Destroy(dummyRewardObj);
            }
        }
        else
        {
            if (selectedOriginalInventorySlot == null) return;
            Item invItem = selectedOriginalInventorySlot.currentItem?.GetComponent<Item>();
            if (!invItem) return;

            if (invItem.quantity > currentSelectedQuantity)
            {
                invItem.RemoveFromStack(currentSelectedQuantity);
            }
            else
            {
                Destroy(selectedOriginalInventorySlot.currentItem);
                selectedOriginalInventorySlot.currentItem = null;
            }

            CurrencyController.Instance.AddGold(totalCost);
            AddItemToShop(selectedItem.ID, currentSelectedQuantity);
            
            if (InventoryController.Instance != null)
            {
                InventoryController.Instance.RebuildItemCounts();
            }
        }

        RefreshPlayerInventoryDisplay();
        ResetAllSubPanels();
    }

    private void SpawnAnimalToWorld(GameObject entityPrefab, int quantity)
    {
        // Xác định vị trí gốc làm trung tâm để đẻ động vật
        Vector3 spawnOrigin = Vector3.zero;
        if (animalSpawnPoint != null)
        {
            spawnOrigin = animalSpawnPoint.position;
        }
        else
        {
            Debug.LogWarning("Chưa kéo gán Transform vị trí chuồng 'Animal Spawn Point' vào ShopController! Đang spawn tạm ở gốc tọa độ (0,0,0)");
        }

        // Thực hiện vòng lặp sinh số lượng tương ứng người chơi vừa mua thành công
        for (int i = 0; i < quantity; i++)
        {
            // Tạo độ lệch ngẫu nhiên nhỏ bán kính 0.7 ô để tránh việc các con vật sinh ra đè chồng khít lên nhau
            Vector3 randomOffset = new Vector3(Random.Range(-0.7f, 0.7f), Random.Range(-0.7f, 0.7f), 0f);
            
            // Tiến hành sinh vật thể thật ra Map thế giới!
            Instantiate(entityPrefab, spawnOrigin + randomOffset, Quaternion.identity);
        }
    }

    private void CancelTransaction()
    {
        ResetAllSubPanels();
    }

    private void ShowWarningPopup(string message)
    {
        ResetAllSubPanels();
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(true);
            confirmationText.text = message;
            if (confirmTransactionButton != null) confirmTransactionButton.gameObject.SetActive(false);
        }
    }

    public void AddItemToShop(int itemID, int quantity)
    {
        if (!currentShop) return;
        currentShop.AddToStock(itemID, quantity);
        RefreshShopDisplay();
    }

    public bool RemoveItemFromShop(int itemID, int quantity)
    {
        if (!currentShop) return false;

        bool success = currentShop.RemoveFromShopStock(itemID, quantity);
        if (success) RefreshShopDisplay();
        
        return success;
    }
}