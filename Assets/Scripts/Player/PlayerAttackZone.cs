using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttackZone : MonoBehaviour
{
    public float attackOffset = 0.5f;
    private void OnTriggerEnter2D(Collider2D collider2D)
    {
        Debug.Log("Vào attack zone rồi");
        PlayerManager playerManager = gameObject.GetComponentInParent<PlayerManager>();
        if(playerManager == null) Debug.Log("Đéo lấy được player");
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

        if(collider2D.CompareTag("Plant") && playerManager.stateTools == 3)
        {
            Plants plant = collider2D.GetComponent<Plants>();
            if(plant != null)
            {
                if(plant.plantState == PlantState.WithoutWater)
                {
                    Debug.Log("Đã tưới nước");
                    plant.plantState = PlantState.Seed;
                }
            }
        }
    }
}
