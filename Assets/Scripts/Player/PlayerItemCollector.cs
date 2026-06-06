using UnityEngine;

public class PlayerItemCollector : MonoBehaviour
{
    private InventoryController inventoryController;
    void Start()
    {
        inventoryController = FindAnyObjectByType<InventoryController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Item"))
        {
            Item item = collision.GetComponent<Item>();
            if(item != null)
            {
                //Thêm item vào kho đồ
                bool itemAdded = inventoryController.AddItem(collision.gameObject);

                if (itemAdded)
                {
                    item.Pickup();
                    Destroy(collision.gameObject);
                    Debug.Log("Nhặt item rồi");
                }
            }
        }
    }
}
