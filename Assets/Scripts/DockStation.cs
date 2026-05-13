using UnityEngine;

public class DockStation : MonoBehaviour, IInteractable
{
    public Transform spawnPoint;

    public bool CanInteract()
    {
        return true;
    }

    public void Interact()
    {
        // Debug.Log("Nhận E");
        if (HireManager.Instance != null)
        {
            // Debug.Log("Thấy HireManager");
            HireManager.Instance.OpenHireUI(this);
        }
        else
        {
            // Debug.LogError("Không thấy HireManager");
        }
    }
}