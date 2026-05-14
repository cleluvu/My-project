using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum ItemType { None, Axe, Pickaxe, Hoe, WateringCan, Seed, Food }

public class Item : MonoBehaviour
{
    [Header("Item Data")]
    public int ID;
    public string Name;
    public int quantity = 1;
    public ItemType itemType; 
    public string seedName; 
    private TMP_Text quantityText;
    
    //Shop
    public int buyPrice = 10;
    [Range(0, 1)]
    public float sellPriceMultiple = 0.5f;

    public void Awake()
    {
        quantityText = GetComponentInChildren<TMP_Text>();
        UpdateQuantityDisplay();
    }

    public void ConsumeOne()
    {
        quantity--;
        UpdateQuantityDisplay();

        if (quantity <= 0)
        {
            Destroy(gameObject);
        }
    }

    public int GetSellPrice()
    {
        return Mathf.RoundToInt(buyPrice * sellPriceMultiple);
    }

    public void UpdateQuantityDisplay()
    {
        if(quantityText != null)
        {
            quantityText.text = quantity > 1 ? quantity.ToString() : "";
        }
    }

    public void AddToStack(int amount = 1)
    {
        quantity += amount;
        UpdateQuantityDisplay();
    }

    public int RemoveFromStack(int amount = 1)
    {
        int removed = Mathf.Min(amount, quantity);
        quantity -= removed;
        UpdateQuantityDisplay();
        return removed;
    }

    public GameObject CloneItem(int newQuantity)
    {
        GameObject clone = Instantiate(gameObject);
        Item cloneItem = clone.GetComponent<Item>();
        cloneItem.quantity = newQuantity;
        cloneItem.UpdateQuantityDisplay();
        return clone;
    }
    
    public virtual void Pickup()
    {
        Sprite itemIcon = GetComponent<Image>().sprite;
        if(ItemPickupUIController.Instance != null)
        {
            ItemPickupUIController.Instance.ShowItemPickup(Name, itemIcon);
        }
    }

    public void SnapToSlot()
    {
        RectTransform rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.localScale = Vector3.one;
            
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            
            float padding = 5f; 
            rect.offsetMin = new Vector2(padding, padding);
            rect.offsetMax = new Vector2(-padding, -padding);
            
            rect.anchoredPosition = Vector2.zero; 
        }
    }
}
