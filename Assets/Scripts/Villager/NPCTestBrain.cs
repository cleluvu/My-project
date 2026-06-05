using UnityEngine;

[RequireComponent(typeof(NPCActionController))]
[RequireComponent(typeof(NPCCoreSystems))]
public class NPCTestBrain : MonoBehaviour
{
    private NPCActionController actionController;
    private NPCCoreSystems coreSystems;
    private NPC originalNPC;

    [Header("Test Settings")]
    public float timeBetweenActions = 3f;
    private float actionTimer;

    void Awake()
    {
        actionController = GetComponent<NPCActionController>();
        coreSystems = GetComponent<NPCCoreSystems>();
        originalNPC = GetComponent<NPC>();
    }

    void Update()
    {
        // 1. Nếu NPC đang trong hội thoại hoặc game pause -> AI tạm dừng suy nghĩ
        if (!originalNPC.CanInteract()) 
        {
            actionTimer = timeBetweenActions; // Reset timer
            return;
        }

        // 2. Đếm ngược thời gian để ra quyết định mới
        actionTimer -= Time.deltaTime;
        if (actionTimer <= 0)
        {
            DecideNextAction();
            
            // Random lại thời gian chờ cho tự nhiên (ví dụ 2 đến 5 giây)
            actionTimer = timeBetweenActions + Random.Range(-1f, 2f); 
        }
    }

    private void DecideNextAction()
    {
        // ƯU TIÊN 1: Nhu cầu khẩn cấp (Sắp ngất xỉu)
        if (coreSystems.currentNeeds.energy < 15)
        {
            Debug.Log(">> Quá mệt mỏi! Bỏ việc đi ngủ (REST)");
            actionController.ExecuteAction(NPCAction.Rest);
            return;
        }

        // ƯU TIÊN 2: Lịch trình (Schedule)
        if (coreSystems.currentTask != null)
        {
            NPCAction scheduledAction = coreSystems.currentTask.action;

            // Nếu lịch là đi ngủ hoặc đi làm, NPC cần di chuyển đến đích trước
            if (scheduledAction == NPCAction.GoHome || scheduledAction == NPCAction.Work)
            {
                if (coreSystems.currentTask.targetLocation != null)
                {
                    Debug.Log($">> Đang đi đến vị trí để: {scheduledAction}");
                    actionController.MoveToTarget(coreSystems.currentTask.targetLocation.position);
                    return;
                }
            }

            // Nếu là các hành động tại chỗ (Rest, Wander, v.v.)
            Debug.Log($">> Làm theo lịch: {scheduledAction}");
            actionController.ExecuteAction(scheduledAction);
            return;
        }

        // ƯU TIÊN 3: Thời gian rảnh (Free Time)
        int randomChoice = Random.Range(0, 3);
        if (randomChoice <= 1)
        {
            actionController.ExecuteAction(NPCAction.Wander);
        }
        else
        {
            actionController.StopMovement();
        }
    }
}