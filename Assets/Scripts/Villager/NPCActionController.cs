using UnityEngine;
using Pathfinding; // Thêm thư viện A* Pathfinding

[RequireComponent(typeof(NPCCoreSystems))]
public class NPCActionController : MonoBehaviour
{
    private NPCCoreSystems coreSystems;
    private NPC originalNPC;
    private IAstarAI aiPath; 

    [Header("Dialogue Assets")]
    public NPCDialogue[] happyDialogues;
    public NPCDialogue[] sadDialogues;
    public NPCDialogue[] angryDialogues;
    public NPCDialogue[] tiredDialogues;
    public NPCDialogue[] neutralDialogues;

    [Header("Movement Settings")]
    public float wanderRadius = 20f;

    [Header("Environment References")]
    public Transform playerTransform;

    void Awake()
    {
        coreSystems = GetComponent<NPCCoreSystems>();
        originalNPC = GetComponent<NPC>();
        
        aiPath = GetComponent<IAstarAI>(); 
    }

    public void ExecuteAction(NPCAction action)
    {

        float distanceToPlayer = 100f; // Mặc định ở rất xa
        if (playerTransform != null)
        {
            distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        }

        switch (action)
        {
            case NPCAction.Talk:
                if (distanceToPlayer <= 3f && coreSystems.CanInteractWithPlayerToday()) 
                {
                    StopMovement();
                    SetupDynamicDialogue();
                    if (originalNPC.CanInteract()) originalNPC.Interact(); 
                }
                break;
                
            case NPCAction.Trade:
                if (distanceToPlayer <= 3f && coreSystems.CanInteractWithPlayerToday())
                {
                    if (coreSystems.playerRelationship.trust >= 30f)
                    {
                        StopMovement();
                        coreSystems.TryDropItemForPlayer(); 
                    }
                    else
                    {
                        Debug.Log($"[Trade Failed] {gameObject.name} từ chối giao dịch! (Trust: {coreSystems.playerRelationship.trust}/30)");
                        MoveToRandomLocation(); 
                    }
                }
                break;
                
            case NPCAction.Wander:
                if (aiPath.reachedDestination || !aiPath.hasPath)
                {
                    MoveToRandomLocation();
                }
                break;
                
            case NPCAction.Rest:
                ScheduleTask restTask = coreSystems.currentTask;
                if (restTask != null && restTask.targetLocation != null)
                {
                    MoveToTarget(restTask.targetLocation.position);

                    float distanceToRestSpot = Vector3.Distance(transform.position, restTask.targetLocation.position);
                    if (distanceToRestSpot < 1.5f)
                    {
                        StopMovement();

                        if (coreSystems.currentHour >= 11 && coreSystems.currentHour <= 16)
                        {
                            coreSystems.currentNeeds.energy = Mathf.Clamp(coreSystems.currentNeeds.energy + 30f * Time.deltaTime, 0, 100);
                            coreSystems.currentNeeds.hunger = Mathf.Clamp(coreSystems.currentNeeds.hunger + 40f * Time.deltaTime, 0, 100);
                        }
                        else
                        {
                            coreSystems.currentNeeds.energy = Mathf.Clamp(coreSystems.currentNeeds.energy + 20f * Time.deltaTime, 0, 100);
                        }
                    }
                }
                else
                {
                    StopMovement();
                    coreSystems.currentNeeds.energy = Mathf.Clamp(coreSystems.currentNeeds.energy + 10f * Time.deltaTime, 0, 100);
                }
                break;
                
            case NPCAction.Work:
                ScheduleTask workTask = coreSystems.currentTask;
                if (workTask != null && workTask.targetLocation != null)
                {
                    MoveToTarget(workTask.targetLocation.position);
                    float distanceToWork = Vector3.Distance(transform.position, workTask.targetLocation.position);
                    if (distanceToWork < 1.5f)
                    {
                        // Đứng im làm việc
                        StopMovement();
                        // Làm việc tiêu hao năng lượng nhiều hơn
                        coreSystems.currentNeeds.energy = Mathf.Clamp(coreSystems.currentNeeds.energy - 2f * Time.deltaTime, 0, 100);
                    }
                }
                break;
                
            case NPCAction.GoHome:
                ScheduleTask homeTask = coreSystems.currentTask;
                if (homeTask != null && homeTask.targetLocation != null)
                {
                    // Di chuyển về phía ngôi nhà
                    MoveToTarget(homeTask.targetLocation.position);
                    float distanceToHome = Vector3.Distance(transform.position, homeTask.targetLocation.position);
                    if (distanceToHome < 2f)
                    {
                        // Về đến nhà thì đi lang thang trong nhà để chờ giờ đi ngủ
                        if (aiPath.reachedDestination || !aiPath.hasPath)
                        {
                            MoveToRandomLocation(1.5f);
                        }
                    }
                }
                break;
                
            case NPCAction.IgnorePlayer:
                // Nếu lỡ đứng gần thì lập tức hủy tương tác và bỏ đi chỗ khác
                if (distanceToPlayer <= 3f)
                {
                    // Chọn một điểm ngẫu nhiên để lách né người chơi đi chỗ khác
                    MoveToRandomLocation(); 
                }
                else
                {
                    // Nếu ở xa rồi thì cứ tiếp tục hành trình làm việc của mình
                    if (coreSystems.currentTask != null && coreSystems.currentTask.targetLocation != null)
                    {
                        MoveToTarget(coreSystems.currentTask.targetLocation.position);
                    }
                }
                break;
        }
    }
    public void MoveToTarget(Vector3 targetPosition)
    {
        if (aiPath != null)
        {
            Vector2 randomOffset = Random.insideUnitCircle * 2f; 
            
            Vector3 finalDestination = targetPosition + new Vector3(randomOffset.x, randomOffset.y, 0f);

            aiPath.isStopped = false;
            aiPath.destination = finalDestination;
            
            // aiPath.SearchPath();
        }
    }
    private void MoveToRandomLocation(float customRadius = -1f)
    {
        if (aiPath != null)
        {
            float radiusToUse = customRadius > 0f ? customRadius : wanderRadius; 
            
            Vector2 randomDirection = Random.insideUnitCircle * radiusToUse;
            Vector3 randomPoint = transform.position + new Vector3(randomDirection.x, randomDirection.y, 0f);

            NNInfo nearestNode = AstarPath.active.GetNearest(randomPoint, NNConstraint.Default);
            if (nearestNode.node != null)
            {
                MoveToTarget((Vector3)nearestNode.node.position);
            }
        }
    }

    public void StopMovement()
    {
        if (aiPath != null)
        {
            aiPath.isStopped = true;
            aiPath.destination = transform.position;
        }
    }

    public void SetupDynamicDialogue()
    {
        NPCDialogue selectedDialogue = null;

        switch (coreSystems.currentEmotion)
        {
            case EmotionState.Happy:
                if (happyDialogues.Length > 0) selectedDialogue = happyDialogues[Random.Range(0, happyDialogues.Length)];
                break;
            case EmotionState.Angry:
                if (angryDialogues.Length > 0) selectedDialogue = angryDialogues[Random.Range(0, angryDialogues.Length)];
                break;
            case EmotionState.Sad:
                if (sadDialogues.Length > 0) selectedDialogue = sadDialogues[Random.Range(0, sadDialogues.Length)];
                break;
            case EmotionState.Tired:
                if (tiredDialogues.Length > 0) selectedDialogue = tiredDialogues[Random.Range(0, sadDialogues.Length)];
                break;
            case EmotionState.Neutral:
                if (neutralDialogues.Length > 0) selectedDialogue = neutralDialogues[Random.Range(0, sadDialogues.Length)];
                break;
            default:
                if (happyDialogues.Length > 0) selectedDialogue = happyDialogues[0];
                break;
        }

        if (selectedDialogue != null)
        {
            originalNPC.dialogueData = selectedDialogue; 
        }
    }
}