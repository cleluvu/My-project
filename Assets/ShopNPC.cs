using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class ShopNPC : MonoBehaviour, IInteractable
{
    [Header("Shop Info")]
    [SerializeField] private string shopID = "shop_merchant_01";
    [SerializeField] private string shopkeepName = "Merchant";
    [SerializeField] private List<ShopStockItem> defaultShopStock = new();
    [SerializeField] private int autoGenerateItemCount = 4;
    [SerializeField] private int autoGenerateStockPerItem = 5;

    private readonly List<ShopStockItem> currentShopStock = new();
    private bool isInitialized = false;

    [System.Serializable]
    public class ShopStockItem
    {
        public ShopItemData itemData;
        public int quantity;
    }

    public string ShopID => shopID;
    public string ShopkeepName => shopkeepName;

    private void Start()
    {
        InitializeShop();
    }

    private void InitializeShop()
    {
        if (isInitialized) return;

        foreach (var item in defaultShopStock)
        {
            if (item.itemData == null || item.quantity <= 0)
            {
                continue;
            }

            currentShopStock.Add(new ShopStockItem
            {
                itemData = item.itemData,
                quantity = item.quantity
            });
        }

        if (currentShopStock.Count == 0)
        {
            AutoGenerateStockFromItemDictionary();
        }

        isInitialized = true;
    }

    private void AutoGenerateStockFromItemDictionary()
    {
        ItemDictionary dictionary = FindAnyObjectByType<ItemDictionary>();
        if (dictionary == null || dictionary.itemPrefabs == null || dictionary.itemPrefabs.Count == 0)
        {
            return;
        }

        int maxCount = Mathf.Min(autoGenerateItemCount, dictionary.itemPrefabs.Count);
        for (int i = 0; i < maxCount; i++)
        {
            Item itemPrefab = dictionary.itemPrefabs[i];
            if (itemPrefab == null)
            {
                continue;
            }

            ShopItemData runtimeData = ScriptableObject.CreateInstance<ShopItemData>();
            runtimeData.name = $"RuntimeShopItem_{itemPrefab.name}";
            runtimeData.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;

            string itemName = string.IsNullOrWhiteSpace(itemPrefab.Name) ? itemPrefab.name : itemPrefab.Name;
            int buyPrice = Mathf.Max(1, itemPrefab.buyPrice);
            int sellPrice = Mathf.Max(1, itemPrefab.GetSellPrice());
            Sprite icon = itemPrefab.GetComponentInChildren<Image>()?.sprite;
            runtimeData.SetRuntimeData(itemName, buyPrice, sellPrice, icon, itemPrefab.gameObject);

            currentShopStock.Add(new ShopStockItem
            {
                itemData = runtimeData,
                quantity = Mathf.Max(1, autoGenerateStockPerItem)
            });
        }
    }

    public bool CanInteract()
    {
        return true;
    }

    public void Interact()
    {
        if (ShopController.Instance == null) return;

        if (ShopController.Instance.IsOpen)
        {
            ShopController.Instance.CloseShop();
        }
        else
        {
            ShopController.Instance.OpenShop(this);
        }
    }

    public List<ShopStockItem> GetCurrentStock()
    {
        EnsureStockReady();
        return currentShopStock;
    }

    public void EnsureStockReady()
    {
        if (!isInitialized)
        {
            InitializeShop();
        }

        if (currentShopStock.Count == 0)
        {
            AutoGenerateStockFromItemDictionary();
        }
    }

    public bool TryDecreaseStock(ShopItemData itemData, int amount)
    {
        ShopStockItem existing = currentShopStock.Find(s => s.itemData == itemData);
        if (existing != null && existing.quantity >= amount)
        {
            existing.quantity -= amount;
            return true;
        }
        return false;
    }

    public int GetStockAmount(ShopItemData itemData)
    {
        ShopStockItem existing = currentShopStock.Find(s => s.itemData == itemData);
        return existing != null ? existing.quantity : 0;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (ShopController.Instance != null && ShopController.Instance.IsOpen)
        {
            ShopController.Instance.CloseShop();
        }

    }
}