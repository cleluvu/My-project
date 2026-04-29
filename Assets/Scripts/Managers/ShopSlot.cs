using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text buyPriceText;
    [SerializeField] private TMP_Text sellPriceText;
    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text actionButtonText;
    [SerializeField] private TMP_Text stockText;

    private ShopItemData slotData;
    private Action<ShopItemData> onBuyClicked;
    private bool isButtonBound;
    private string currentActionLabel = "Buy";

    private void Awake()
    {
        EnsureReferences();
        BindButtonIfNeeded();
    }

    private void OnDestroy()
    {
        if (buyButton != null)
        {
            buyButton.onClick.RemoveListener(HandleBuyClicked);
        }
    }

    public void Setup(ShopItemData itemData, int currentStock, string actionLabel, Action<ShopItemData> onAction)
    {
        EnsureReferences();
        RebindButton();

        slotData = itemData;
        onBuyClicked = onAction;
        currentActionLabel = actionLabel;

        if (iconImage != null)
        {
            iconImage.sprite = itemData.Icon;
        }

        if (itemNameText != null)
        {
            itemNameText.text = itemData.ItemName;
        }

        if (buyPriceText != null)
        {
            buyPriceText.text = itemData.BuyPrice.ToString();
        }

        if (sellPriceText != null)
        {
            sellPriceText.text = itemData.SellPrice.ToString();
        }

        if (stockText != null)
        {
            stockText.text = currentStock > 0 ? $"x{currentStock}" : "Sold out";
        }

        if (buyButton != null)
        {
            buyButton.interactable = currentStock > 0;
        }

        if (actionButtonText != null)
        {
            actionButtonText.text = actionLabel;
        }
    }

    private void EnsureReferences()
    {
        if (iconImage == null)
        {
            iconImage = transform.Find("ItemIcon")?.GetComponent<Image>();
        }
        if (itemNameText == null)
        {
            itemNameText = transform.Find("ItemNameText")?.GetComponent<TMP_Text>();
        }
        if (buyPriceText == null)
        {
            buyPriceText = transform.Find("BuyPriceText")?.GetComponent<TMP_Text>() ?? transform.Find("PriceText")?.GetComponent<TMP_Text>();
        }
        if (sellPriceText == null)
        {
            sellPriceText = transform.Find("SellPriceText")?.GetComponent<TMP_Text>();
        }
        if (stockText == null)
        {
            stockText = transform.Find("StockText")?.GetComponent<TMP_Text>();
        }
        if (buyButton == null)
        {
            buyButton = transform.Find("ActionButton")?.GetComponent<Button>();
        }
        if (actionButtonText == null)
        {
            actionButtonText = transform.Find("ActionButton/ActionButtonText")?.GetComponent<TMP_Text>();
        }
    }

    private void BindButtonIfNeeded()
    {
        if (isButtonBound || buyButton == null)
        {
            return;
        }

        buyButton.onClick.AddListener(HandleBuyClicked);
        isButtonBound = true;
    }

    private void HandleBuyClicked()
    {
        if (slotData == null)
        {
            Debug.LogWarning("[ShopSlot] Clicked but slotData is null.");
            return;
        }

        Debug.Log($"[ShopSlot] {currentActionLabel} clicked for {slotData.ItemName}");
        onBuyClicked?.Invoke(slotData);
    }

    private void RebindButton()
    {
        if (buyButton == null)
        {
            return;
        }

        buyButton.onClick.RemoveListener(HandleBuyClicked);
        buyButton.onClick.AddListener(HandleBuyClicked);
        isButtonBound = true;
    }
}
