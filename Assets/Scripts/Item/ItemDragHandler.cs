using System.Collections.Generic;
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

                    NotifyInventoryChanged();
                }
                else
                {
                    GameObject temp = dropSlot.currentItem;
                    temp.transform.SetParent(originalSlot.transform);
                    temp.GetComponent<Item>().SnapToSlot();
                    originalSlot.currentItem = temp;

                    transform.SetParent(dropSlot.transform);
                    dropSlot.currentItem = gameObject;
                    GetComponent<Item>().SnapToSlot();
                }
            }
            else
            {
                // Thả vào ô trống
                originalSlot.currentItem = null;
                transform.SetParent(dropSlot.transform);
                dropSlot.currentItem = gameObject;
                GetComponent<Item>().SnapToSlot();
            }
        }
        else
        {
            if (!IsWithinInventory(eventData.position))
            {
                DropItem(originalSlot);
            }
            else
            {
                transform.SetParent(originalParent);
                GetComponent<Item>().SnapToSlot();
            }
        }
    }

    public bool IsWithinInventory(Vector2 mousePosition)
    {
        // Kiểm tra an toàn tránh lỗi Null nếu parent của Slot bị thay đổi cấu trúc
        if (originalParent == null || originalParent.parent == null) return false;
        
        RectTransform inventoryRect = originalParent.parent.GetComponent<RectTransform>();
        if (inventoryRect == null) return false;

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
            GetComponent<Item>().SnapToSlot();
            quantity = 1;
        }
        else
        {
            originalSlot.currentItem = null;
        }

        Transform playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if(playerTransform == null)
        {
            Debug.Log("Player không có tag");
            return;
        }

        Vector2 dropOffset = Random.insideUnitCircle.normalized * Random.Range(minDropDistance, maxDropDistance);
        Vector2 dropPosition = (Vector2)playerTransform.position + dropOffset;

        GameObject dropItem = Instantiate(gameObject, dropPosition, Quaternion.identity);
        Item droppedItem = dropItem.GetComponent<Item>();
        if (droppedItem != null) droppedItem.quantity = 1;

        BounceEffect bounce = dropItem.GetComponent<BounceEffect>();
        if (bounce != null) bounce.StartBounce();

        if(quantity <= 1 && originalSlot.currentItem == null)
        {
            Destroy(gameObject);
        } 

        NotifyInventoryChanged();
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
        if(item == null || item.quantity <= 1) return;

        int splitAmount = item.quantity / 2;
        if(splitAmount <= 0) return;

        item.RemoveFromStack(splitAmount);
        GameObject newItem = item.CloneItem(splitAmount);

        if(inventoryController == null || newItem == null) return;

        foreach(Transform slotTransform in inventoryController.inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if(slot != null && slot.currentItem == null)
            {
                slot.currentItem = newItem;
                newItem.transform.SetParent(slot.transform);
                newItem.GetComponent<Item>().SnapToSlot();

                NotifyInventoryChanged();
                return;
            }
        }

        // Nếu hòm đồ đầy không tách được, hoàn trả lại số lượng ban đầu
        item.AddToStack(splitAmount);
        Destroy(newItem);   
    }

    private void NotifyInventoryChanged()
    {
        if (InventoryController.Instance != null)
        {
            InventoryController.Instance.RebuildItemCounts();
        }
    }
}