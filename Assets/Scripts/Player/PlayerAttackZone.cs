using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackZone : MonoBehaviour
{
    public float attackOffset = 0.5f;
    private List<GameObject> hitObjects = new List<GameObject>();

    private void OnEnable()
    {
        hitObjects.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collider2D)
    {
        PlayerManager playerManager = gameObject.GetComponentInParent<PlayerManager>();
        if (playerManager == null) return;

        if (hitObjects.Contains(collider2D.gameObject)) return;

        // Chặt cây
        if (collider2D.CompareTag("Tree") && playerManager.stateTools == 1)
        {
            CollectedObject resource = collider2D.GetComponent<CollectedObject>();
            if (resource != null)
            {
                hitObjects.Add(collider2D.gameObject);
                Debug.Log("CHẶT CÂY THÀNH CÔNG!");
                resource.GetDamage(1);
            }
        }

        // Phá đá
        if (collider2D.CompareTag("Stone") && playerManager.stateTools == 1)
        {
            CollectedObject resource = collider2D.GetComponent<CollectedObject>();
            if (resource != null)
            {
                hitObjects.Add(collider2D.gameObject);
                Debug.Log("ĐÀO ĐÁ THÀNH CÔNG!");
                resource.GetDamage(1);
            }
        }

        // Cho động vật ăn
        if (playerManager.stateTools == 6)
        {
            Entity entity = collider2D.GetComponent<Entity>();
            if (entity != null && entity.isHungry)
            {
                int requiredFoodID = entity.foodItemID;
                HotbarController hotbar = Object.FindFirstObjectByType<HotbarController>();
                Item equippedItem = hotbar != null ? hotbar.GetEquippedItem() : null;

                if (equippedItem != null && equippedItem.ID == requiredFoodID)
                {
                    hitObjects.Add(collider2D.gameObject);
                    bool feedSuccess = entity.TryFeed(requiredFoodID);
                    if (feedSuccess)
                    {
                        equippedItem.ConsumeOne();
                        Debug.Log("Cho ăn thành công!");
                    }
                }
            }
        }
    }
}