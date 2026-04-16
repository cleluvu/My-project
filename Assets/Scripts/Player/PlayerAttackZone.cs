using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttackZone : MonoBehaviour
{
    public float attackOffset = 0.5f;
    private void OnTriggerEnter2D(Collider2D collider2D)
    {
        Debug.Log("Vào attack zone rồi");
        PlayerManager playerManager = gameObject.GetComponentInParent<PlayerManager>();
        if(playerManager == null) Debug.Log("Không lấy được player");
        if (collider2D.CompareTag("Tree") && playerManager.stateTools == 1)
        {
            Tree tree = collider2D.GetComponent<Tree>();
            if(tree != null)
            {
                Debug.Log("Chặt cây rồi");
                tree.GetDamage(1);
            }
        }

        if (collider2D.CompareTag("Stone") && playerManager.stateTools == 1)
        {
            Stone stone = collider2D.GetComponent<Stone>();
            if(stone != null)
            {
                Debug.Log("Đào đá rồi");
                stone.GetDamage(1);
            }
        }

        if(playerManager.stateTools == 6)
        {
            Entity entity = collider2D.GetComponent<Entity>();
            if(entity != null)
            {
                if (entity.isHungry)
                {
                    int requiredFoodID = entity.foodItemID;
                    if(InventoryController.Instance != null && InventoryController.Instance.HasItem(requiredFoodID))
                    {
                        bool feedSuccess = entity.TryFeed(requiredFoodID);

                        if (feedSuccess)
                        {
                            InventoryController.Instance.RemoveItem(requiredFoodID, 1);
                            Debug.Log("Cho ăn thành công");
                        }
                    }
                    else
                    {
                        Debug.Log("Không có thức ăn");
                    }
                }
                else
                {
                    Debug.Log("Chưa đói");
                }
            }
        }
    }
}
