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
        if (HireManager.Instance != null)
        {
            HireManager.Instance.OpenHireUI(this);
        }
        else
        {
            Debug.LogError("Không thấy HireManager");
        }
    }
}