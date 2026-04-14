using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    Transform originalParent;
    CanvasGroup canvasGroup;

    public float minDropDistance = 2f;
    public float maxDropDistance = 3f;

    private InventoryController inventoryController;
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        inventoryController = InventoryController.Instance;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(transform.root);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {   
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        Slot dropSlot = null;

        if (eventData.pointerEnter != null)
        {
            dropSlot = eventData.pointerEnter.GetComponentInParent<Slot>();
        }

        Slot originalSlot = originalParent.GetComponent<Slot>();

        if (dropSlot != null)
        {
            if (dropSlot.currentItem != null)
            {
                Item draggedItem = GetComponent<Item>();
                Item targetItem = dropSlot.currentItem.GetComponent<Item>();

                if(draggedItem.ID == targetItem.ID)
                {
                    targetItem.AddToStack(draggedItem.quantity);
                    originalSlot.currentItem = null;
                    Destroy(gameObject);
                }
                else
                {
                    GameObject temp = dropSlot.currentItem;
                    temp.transform.SetParent(originalSlot.transform);
                    temp.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    originalSlot.currentItem = temp;

                    transform.SetParent(dropSlot.transform);
                    dropSlot.currentItem = gameObject;
                    GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                }
            }
            else
            {
                originalSlot.currentItem = null;
                transform.SetParent(dropSlot.transform);
                dropSlot.currentItem = gameObject;
                GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
        }
        else
        {
            // Nếu kéo ra ngoài kho -> bỏ luôn ra ngoài
            if (!IsWithinInventory(eventData.position))
            {
                DropItem(originalSlot);
            }
            else
            {
                transform.SetParent(originalParent);
                GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
        }
    }

    public bool IsWithinInventory(Vector2 mousePosition)
    {
        RectTransform inventoryRect =  originalParent.parent.GetComponent<RectTransform>();
        return RectTransformUtility.RectangleContainsScreenPoint(inventoryRect, mousePosition);
    }

    public void DropItem(Slot originalSlot)
    {
        Item item = GetComponent<Item>();
        int quantity = item.quantity;

        if(quantity > 1)
        {
            item.RemoveFromStack();

            transform.SetParent(originalParent);
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            quantity = 1;
        }
        else
        {
            originalSlot.currentItem = null;
        }

        // Xem người chơi ở đâu
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if(playerTransform == null)
        {
            Debug.Log("Player không có tag");
            return;
        }

        // Điểm rơi ngẫu nhiên quanh người chơi
        Vector2 dropOffset = Random.insideUnitCircle.normalized * Random.Range(minDropDistance, maxDropDistance);
        Vector2 dropPosition = (Vector2)playerTransform.position + dropOffset;

        // Khởi tạo item tại điểm rơi
        GameObject dropItem = Instantiate(gameObject, dropPosition, Quaternion.identity);
        Item droppedItem = dropItem.GetComponent<Item>();
        droppedItem.quantity = 1;

        dropItem.GetComponent<BounceEffect>().StartBounce();

        // Hủy UI của item (lúc kéo ra)
        if(quantity <= 1 && originalSlot.currentItem == null)
        {
            Destroy(gameObject);
        } 
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Right)
        {
            SplitStack();
        }
    }

    private void SplitStack()
    {
        Item item = GetComponent<Item>();
        if(item == null || item.quantity <= 1)
        {
            return;
        }

        int splitAmount = item.quantity / 2;
        if(splitAmount <= 0) return;

        item.RemoveFromStack(splitAmount);
        GameObject newItem = item.CloneItem(splitAmount);

        if(inventoryController == null || newItem == null) return;

        // Kiếm ô trống nào đó để vứt vào
        foreach(Transform slotTransform in inventoryController.inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if(slot != null && slot.currentItem == null)
            {
                slot.currentItem = newItem;
                newItem.transform.SetParent(slot.transform);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                return;
            }
        }

        // Không có ô trống thì
        item.AddToStack(splitAmount);
        Destroy(newItem);   
    }
}
