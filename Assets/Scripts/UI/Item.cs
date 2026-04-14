using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Item : MonoBehaviour
{
    public int ID;
    public string Name;
    public int quantity = 1;
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
}
