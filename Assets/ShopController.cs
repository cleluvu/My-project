using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class ShopController : MonoBehaviour
{
    public static ShopController Instance;

    [Header("UI")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Transform shopInventoryGrid;
    [SerializeField] private Transform playerInventoryGrid;
    [SerializeField] private GameObject shopSlotPrefab;
    [SerializeField] private TMP_Text playerMoneyText;
    [SerializeField] private TMP_Text shopTitleText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private bool autoCreateCurrencyController = true;

    private ShopNPC currentShop;
    private readonly List<GameObject> shopSpawnedSlots = new();
    private readonly List<GameObject> playerSpawnedSlots = new();
    private readonly Dictionary<ShopItemData, int> playerShopDataToItemID = new();
    private CurrencyController cachedCurrencyController;

    public GameObject shopPanelRef => shopPanel;
    public bool IsOpen => shopPanel != null && shopPanel.activeSelf;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }

        CurrencyController currency = GetCurrencyController();
        if (currency != null)
        {
            currency.OnGoldChanged += UpdateMoneyDisplay;
            UpdateMoneyDisplay(currency.GetGold());
        }
    }

    private void OnDestroy()
    {
        CurrencyController currency = CurrencyController.Instance != null ? CurrencyController.Instance : cachedCurrencyController;
        if (currency != null)
        {
            currency.OnGoldChanged -= UpdateMoneyDisplay;
        }
    }

    private void Update()
    {
        // Safety net when close button hides panel directly.
        if (currentShop != null && shopPanel != null && !shopPanel.activeSelf && PauseController.IsGamePause)
        {
            ClearAllSlots();
            currentShop = null;
            PauseController.SetPause(false);
        }
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
        currentShop.EnsureStockReady();
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
        }

        if (shopTitleText != null)
        {
            shopTitleText.text = shop.ShopkeepName + "'s Shop";
        }

        BuildShopUI();
        BuildPlayerUI();
        SetFeedback(string.Empty);
        PauseController.SetPause(true);
    }

    public void CloseShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }

        ClearAllSlots();
        SetFeedback(string.Empty);
        currentShop = null;
        PauseController.SetPause(false);
    }

    public void BuyItem(ShopItemData itemData)
    {
        Debug.Log($"[ShopController] BuyItem called: {(itemData != null ? itemData.ItemName : "null")}");
        if (currentShop == null || itemData == null || itemData.ItemPrefab == null)
        {
            Debug.LogWarning("[ShopController] BuyItem aborted: currentShop/itemData/itemPrefab invalid.");
            SetFeedback("Item data is invalid.");
            return;
        }

        int stockAmount = currentShop.GetStockAmount(itemData);
        Debug.Log($"[ShopController] Stock check for {itemData.ItemName}: {stockAmount}");
        if (stockAmount <= 0)
        {
            Debug.LogWarning($"[ShopController] BuyItem aborted: {itemData.ItemName} is sold out.");
            SetFeedback("Item is sold out.");
            BuildShopUI();
            return;
        }

        CurrencyController currency = GetCurrencyController();
        if (currency == null)
        {
            Debug.LogWarning("[ShopController] BuyItem aborted: CurrencyController.Instance is null.");
            SetFeedback("Currency system not found.");
            return;
        }

        int goldBeforeSpend = currency.GetGold();
        bool spendSuccess = currency.SpendGold(itemData.BuyPrice);
        int goldAfterSpend = currency.GetGold();
        Debug.Log($"[ShopController] SpendGold({itemData.BuyPrice}) => {spendSuccess}. Gold: {goldBeforeSpend} -> {goldAfterSpend}");
        if (!spendSuccess)
        {
            SetFeedback("Not enough gold.");
            return;
        }

        if (InventoryController.Instance == null)
        {
            Debug.LogWarning("[ShopController] InventoryController.Instance is null. Refunding gold.");
            currency.AddGold(itemData.BuyPrice);
            SetFeedback("Inventory system not found.");
            return;
        }

        bool added = InventoryController.Instance.AddItem(itemData.ItemPrefab);
        Debug.Log($"[ShopController] AddItem({itemData.ItemName}) => {added}");
        if (!added)
        {
            currency.AddGold(itemData.BuyPrice);
            Debug.Log($"[ShopController] AddItem failed. Gold refunded. Current gold: {currency.GetGold()}");
            SetFeedback("Inventory is full.");
            return;
        }

        bool decreased = currentShop.TryDecreaseStock(itemData, 1);
        Debug.Log($"[ShopController] TryDecreaseStock({itemData.ItemName},1) => {decreased}");
        SetFeedback($"Bought {itemData.ItemName}.");
        Debug.Log($"[ShopController] BuyItem success. Current gold: {currency.GetGold()}");
        BuildShopUI();
        BuildPlayerUI();
    }

    public bool TrySellItem(Item inventoryItem, int amount = 1)
    {
        CurrencyController currency = GetCurrencyController();
        if (inventoryItem == null || amount <= 0 || InventoryController.Instance == null || currency == null)
        {
            return false;
        }

        int income = Mathf.Max(0, inventoryItem.GetSellPrice()) * amount;
        if (income <= 0)
        {
            return false;
        }

        InventoryController.Instance.RemoveItem(inventoryItem.ID, amount);
        currency.AddGold(income);
        SetFeedback($"Sold {inventoryItem.Name} x{amount}.");
        BuildPlayerUI();
        return true;
    }

    private void SellItemFromPlayerPanel(ShopItemData itemData)
    {
        if (itemData == null)
        {
            return;
        }

        if (!playerShopDataToItemID.TryGetValue(itemData, out int itemID))
        {
            SetFeedback("Cannot find item in inventory.");
            return;
        }

        Item itemPrefab = itemData.ItemPrefab != null ? itemData.ItemPrefab.GetComponent<Item>() : null;
        if (itemPrefab == null)
        {
            SetFeedback("Item data is invalid.");
            return;
        }

        itemPrefab.ID = itemID;
        bool sold = TrySellItem(itemPrefab, 1);
        if (sold)
        {
            BuildShopUI();
        }
    }

    private void BuildShopUI()
    {
        if (currentShop == null || shopInventoryGrid == null || shopSlotPrefab == null)
        {
            SetFeedback("Shop UI references are missing.");
            return;
        }

        ClearShopSlots();

        List<ShopNPC.ShopStockItem> stock = currentShop.GetCurrentStock();
        if (stock == null || stock.Count == 0)
        {
            SetFeedback("Shop has no stock. Check ShopNPC stock or ItemDictionary.");
            return;
        }

        int createdCount = 0;
        foreach (ShopNPC.ShopStockItem stockItem in stock)
        {
            if (stockItem.itemData == null)
            {
                continue;
            }

            GameObject slotObj = Instantiate(shopSlotPrefab, shopInventoryGrid);
            ShopSlot slot = slotObj.GetComponent<ShopSlot>();
            if (slot != null)
            {
                slot.Setup(stockItem.itemData, stockItem.quantity, "Buy", BuyItem);
            }
            shopSpawnedSlots.Add(slotObj);
            createdCount++;
        }

        if (createdCount == 0)
        {
            SetFeedback("No valid shop items to display.");
        }
    }

    private void BuildPlayerUI()
    {
        ClearPlayerSlots();
        playerShopDataToItemID.Clear();

        if (playerInventoryGrid == null || shopSlotPrefab == null || InventoryController.Instance == null || InventoryController.Instance.inventoryPanel == null)
        {
            return;
        }

        Dictionary<int, int> quantityById = new();
        Dictionary<int, Item> itemById = new();

        foreach (Transform slotTransform in InventoryController.Instance.inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot == null || slot.currentItem == null)
            {
                continue;
            }

            Item inventoryItem = slot.currentItem.GetComponent<Item>();
            if (inventoryItem == null)
            {
                continue;
            }

            if (!quantityById.ContainsKey(inventoryItem.ID))
            {
                quantityById[inventoryItem.ID] = 0;
                itemById[inventoryItem.ID] = inventoryItem;
            }

            quantityById[inventoryItem.ID] += Mathf.Max(1, inventoryItem.quantity);
        }

        foreach (KeyValuePair<int, int> entry in quantityById)
        {
            Item item = itemById[entry.Key];
            ShopItemData runtimeData = ScriptableObject.CreateInstance<ShopItemData>();
            runtimeData.name = $"RuntimePlayerItem_{item.name}";
            runtimeData.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;

            string itemName = string.IsNullOrWhiteSpace(item.Name) ? item.name : item.Name;
            Sprite icon = item.GetComponentInChildren<Image>()?.sprite;
            runtimeData.SetRuntimeData(itemName, Mathf.Max(1, item.buyPrice), Mathf.Max(1, item.GetSellPrice()), icon, item.gameObject);
            playerShopDataToItemID[runtimeData] = entry.Key;

            GameObject slotObj = Instantiate(shopSlotPrefab, playerInventoryGrid);
            ShopSlot slotUI = slotObj.GetComponent<ShopSlot>();
            if (slotUI != null)
            {
                slotUI.Setup(runtimeData, entry.Value, "Sell", SellItemFromPlayerPanel);
            }
            playerSpawnedSlots.Add(slotObj);
        }

        if (quantityById.Count == 0)
        {
            SetFeedback("Player inventory is empty.");
        }
    }

    private void ClearShopSlots()
    {
        if (shopInventoryGrid != null)
        {
            for (int i = shopInventoryGrid.childCount - 1; i >= 0; i--)
            {
                Destroy(shopInventoryGrid.GetChild(i).gameObject);
            }
        }

        for (int i = shopSpawnedSlots.Count - 1; i >= 0; i--)
        {
            if (shopSpawnedSlots[i] != null)
            {
                Destroy(shopSpawnedSlots[i]);
            }
        }
        shopSpawnedSlots.Clear();
    }

    private void ClearPlayerSlots()
    {
        if (playerInventoryGrid != null)
        {
            for (int i = playerInventoryGrid.childCount - 1; i >= 0; i--)
            {
                Destroy(playerInventoryGrid.GetChild(i).gameObject);
            }
        }

        for (int i = playerSpawnedSlots.Count - 1; i >= 0; i--)
        {
            if (playerSpawnedSlots[i] != null)
            {
                Destroy(playerSpawnedSlots[i]);
            }
        }
        playerSpawnedSlots.Clear();
    }

    private void ClearAllSlots()
    {
        ClearShopSlots();
        ClearPlayerSlots();
        playerShopDataToItemID.Clear();
    }

    private void SetFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
    }

    private CurrencyController GetCurrencyController()
    {
        if (CurrencyController.Instance != null)
        {
            cachedCurrencyController = CurrencyController.Instance;
            return cachedCurrencyController;
        }

        if (cachedCurrencyController != null)
        {
            return cachedCurrencyController;
        }

        cachedCurrencyController = FindAnyObjectByType<CurrencyController>();
        if (cachedCurrencyController == null && autoCreateCurrencyController)
        {
            GameObject runtimeCurrency = new GameObject("RuntimeCurrencyController");
            cachedCurrencyController = runtimeCurrency.AddComponent<CurrencyController>();
            Debug.LogWarning("[ShopController] CurrencyController not found. Auto-created RuntimeCurrencyController.");
        }

        return cachedCurrencyController;
    }
}
