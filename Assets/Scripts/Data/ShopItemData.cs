using UnityEngine;

[CreateAssetMenu(fileName = "ShopItem_", menuName = "Shop/Shop Item Data")]
public class ShopItemData : ScriptableObject
{
    [Header("Display")]
    [SerializeField] private string itemName;
    [SerializeField] private Sprite icon;

    [Header("Gameplay")]
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private int buyPrice = 10;
    [SerializeField] private int sellPrice = 5;

    public string ItemName => itemName;
    public Sprite Icon => icon;
    public GameObject ItemPrefab => itemPrefab;
    public int BuyPrice => Mathf.Max(0, buyPrice);
    public int SellPrice => Mathf.Max(0, sellPrice);

    public void SetRuntimeData(string newName, int newBuyPrice, int newSellPrice, Sprite newIcon, GameObject newItemPrefab)
    {
        itemName = newName;
        buyPrice = newBuyPrice;
        sellPrice = newSellPrice;
        icon = newIcon;
        itemPrefab = newItemPrefab;
    }
}
