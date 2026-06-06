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
    public GameObject actionPanel; // Panel chứa nút Mua/Bán riêng biệt
    public Button buyActionButton;
    public Button sellActionButton;

    [Header("Quantity Panel (New feature)")]
    public GameObject quantityPanel; // Panel chọn số lượng
    public TMP_Text quantityText;
    public Slider quantitySlider; // Thanh kéo chọn số lượng
    public Button confirmQuantityButton;

    [Header("Confirmation Panel (New feature)")]
    public GameObject confirmationPanel; // Panel hỏi xác nhận cuối cùng
    public TMP_Text confirmationText; // Hiển thị chuẩn Format yêu cầu của ông
    public Button confirmTransactionButton;
    public Button cancelTransactionButton;

    private ItemDictionary itemDictionary;
    private ShopNPC currentShop;

    // Các biến trạng thái lưu trữ giao dịch đang diễn ra
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
        
        // Ẩn toàn bộ các Panel chức năng lúc đầu
        shopPanel.SetActive(false);
        if (actionPanel != null) actionPanel.SetActive(false);
        if (quantityPanel != null) quantityPanel.SetActive(false);
        if (confirmationPanel != null) confirmationPanel.SetActive(false);

        if (CurrencyController.Instance != null)
        {
            CurrencyController.Instance.OnGoldChanged += UpdateMoneyDisplay;
            UpdateMoneyDisplay(CurrencyController.Instance.GetGold());
        }

        // Đăng ký sự kiện nút bấm hệ thống
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

    // --- LOGIC XỬ LÝ MUA BÁN NÂNG CAO ---

    // Bước 1: Khi Player click chuột chọn vào một Item
    public void HandleItemSelection(Item item, ShopSlot slot, Slot originalInventorySlot, bool isShopItem)
    {
        selectedItem = item;
        selectedSlot = slot;
        selectedOriginalInventorySlot = originalInventorySlot;
        isBuyingProcess = isShopItem;

        // Bật thanh hành động (Action Panel) hiện nút Mua hoặc Bán
        if (actionPanel != null)
        {
            actionPanel.SetActive(true);
            if (buyActionButton != null) buyActionButton.gameObject.SetActive(isBuyingProcess);
            if (sellActionButton != null) sellActionButton.gameObject.SetActive(!isBuyingProcess);
        }
    }

    // Bước 2a: Ấn nút MUA hiển thị trên UI
    private void OnBuyActionButtonClicked()
    {
        if (selectedItem == null || selectedSlot == null) return;

        int currentGold = CurrencyController.Instance.GetGold();
        int unitPrice = selectedSlot.itemPrice;

        // KIỂM TRA ĐỦ TIỀN MUA ÍT NHẤT 1 CÁI KHÔNG: Nếu không đủ tiền chặn luôn và báo lỗi
        if (currentGold < unitPrice)
        {
            ShowWarningPopup("Bạn không đủ tiền để mua vật phẩm này!");
            return;
        }

        // Tính số lượng tối đa có thể mua dựa trên số tiền hiện tại và số lượng cửa hàng có sẵn
        int maxByGold = currentGold / unitPrice;
        maxAllowedQuantity = Mathf.Min(maxByGold, selectedItem.quantity);

        SetupQuantityPanel();
    }

    // Bước 2b: Ấn nút BÁN hiển thị trên UI
    private void OnSellActionButtonClicked()
    {
        if (selectedItem == null) return;

        // Số lượng bán tối đa bằng đúng số lượng stack item thực tế đang có trong ô hòm đồ
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

    // Bước 3: Xác nhận số lượng xong -> Hiện hộp thoại theo Format yêu cầu
    private void OnQuantityConfirmed()
    {
        if (quantityPanel != null) quantityPanel.SetActive(false);

        int totalCost = selectedSlot.itemPrice * currentSelectedQuantity;

        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(true);
            if (confirmTransactionButton != null) confirmTransactionButton.gameObject.SetActive(true);
            
            // Ép chuỗi văn bản hiển thị theo đúng chuẩn format ông yêu cầu
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

    // Bước 4: Thực thi giao dịch cộng trừ tiền và thêm bớt item
    private void ExecuteTransaction()
    {
        if (selectedItem == null || selectedSlot == null) return;

        int totalCost = selectedSlot.itemPrice * currentSelectedQuantity;

        if (isBuyingProcess)
        {
            // Kiểm tra an toàn lần cuối chặn đứng không cho mua âm tiền
            if (CurrencyController.Instance.GetGold() < totalCost)
            {
                ShowWarningPopup("Giao dịch thất bại! Bạn không đủ tiền.");
                return;
            }

            GameObject itemPrefab = itemDictionary.GetItemPrefab(selectedItem.ID);
            
            // Tạo Dummy object mang số lượng được chọn để chuyển dữ liệu vào hòm đồ
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
        else
        {
            // Xử lý logic người chơi bán đồ cho cửa hàng
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
            
            // Làm mới bộ cache hòm đồ để đồng bộ tiến độ Quest ngay lập tức
            if (InventoryController.Instance != null)
            {
                InventoryController.Instance.RebuildItemCounts();
            }
        }

        RefreshPlayerInventoryDisplay();
        ResetAllSubPanels();
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

    // --- ĐÃ SỬA LỖI TẬP #28: TRẢ VỀ BOOL VÀ KHỚP BIẾN ĐÚNG CHUẨN ĐỂ XÓA LỖI CS0126 ---
    public bool RemoveItemFromShop(int itemID, int quantity)
    {
        if (!currentShop) return false;

        bool success = currentShop.RemoveFromShopStock(itemID, quantity);
        if (success) RefreshShopDisplay();
        
        return success;
    }
}