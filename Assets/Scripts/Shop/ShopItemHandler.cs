using UnityEngine;
using UnityEngine.EventSystems;

public class ShopItemHandler : MonoBehaviour, IPointerClickHandler
{
    public bool isShopItem;
    public Slot originalInventorySlot;

    public void Initialize(bool shopItem) => isShopItem = shopItem;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left || eventData.button == PointerEventData.InputButton.Right)
        {
            Item item = GetComponent<Item>();
            ShopSlot slot = GetComponentInParent<ShopSlot>();

            if (item != null && slot != null)
            {
                // Truyền toàn bộ thông tin ô đồ vừa nhấn sang ShopController giải quyết bước tiếp theo
                ShopController.Instance.HandleItemSelection(item, slot, originalInventorySlot, isShopItem);
            }
        }
    }
}