using UnityEngine;

public class PlayerCollectZone : MonoBehaviour
{
    Player player;

    void Awake()
    {
        player = GetComponentInParent<Player>();
    }
    private void OnTriggerEnter2D(Collider2D collider2D)
    {
        if (collider2D.CompareTag("Wood"))
        {
            Tree tree = collider2D.GetComponentInParent<Tree>();
            if(tree != null)
            {
                Debug.Log("Nhặt rồi");
                player.items[0] += 1;
                tree.ObjectCollected();
            }
        }

        if (collider2D.CompareTag("Piece of Stone"))
        {
            Stone stone = collider2D.GetComponentInParent<Stone>();
            if(stone != null)
            {
                Debug.Log("Nhặt rồi");
                player.items[1] += 1;
                stone.ObjectCollected();
            }
        }

        if (collider2D.CompareTag("Plant"))
        {
            Plants plant = collider2D.GetComponentInParent<Plants>();
            if(plant != null && plant.plantState == PlantState.Harvesting)
            {
                Debug.Log("Nhặt rồi");
                if(plant.plantType == PlantType.Wheat)
                {
                    player.items[2] += (int)plant.capacity;
                }
                else if(plant.plantType == PlantType.Tomato)
                {
                    player.items[3] += (int)plant.capacity;
                }
                plant.ObjectCollected();
            }
        }
    }
}
