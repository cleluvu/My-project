using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(NPC))] 
[RequireComponent(typeof(AIPath))]  
[RequireComponent(typeof(Animator))] 
public class NPCCoreSystems : MonoBehaviour
{
    [Header("Personality (Fixed)")]
    public PersonalityType personality;

    [Header("Dynamic Systems")]
    public Needs currentNeeds;
    public Relationship playerRelationship;
    public EmotionState currentEmotion = EmotionState.Neutral;

    [Header("Schedule Info")]
    public int currentHour; 
    public DayAndNight timeManager; 
    public List<ScheduleTask> dailySchedule; 
    public ScheduleTask currentTask; 

    [Header("Trading System")]
    public GameObject[] giftItems; 

    [Header("Multi-Agent Social Systems")]
    public Dictionary<int, Relationship> socialMatrix = new Dictionary<int, Relationship>();

    private NPC originalNPC;
    private AIPath aiPath;
    private Animator animator;

    private bool wasInteracting = false; 
    private float lastTradeTime = 0f;
    public int lastInteractedDay = -1;
    private int previousHour = -1;

    void Awake()
    {
        originalNPC = GetComponent<NPC>();
        aiPath = GetComponent<AIPath>();      
        animator = GetComponent<Animator>(); 

        if (dailySchedule != null)
        {
            dailySchedule.Sort((a, b) => a.startHour.CompareTo(b.startHour));
        }
    }

    void Update()
    {
        UpdateNeeds();
        EvaluateEmotion();

        if (timeManager != null)
        {
            currentHour = timeManager.GetCurrentHour();
            if (currentHour != previousHour)
            {
                UpdateSchedule();
                previousHour = currentHour;
            }
        }

        UpdateAnimationDirection();

        bool isInteracting = !originalNPC.CanInteract(); 
        
        if (isInteracting && !wasInteracting)
        {
            OnPlayerInteracted();
        }
        
        wasInteracting = isInteracting; 
    }

    public Relationship GetRelationship(GameObject target)
    {
        int targetID = target.GetInstanceID();
        if (!socialMatrix.ContainsKey(targetID))
        {
            socialMatrix[targetID] = new Relationship();
        }
        return socialMatrix[targetID];
    }

    private void UpdateAnimationDirection()
    {
        if (aiPath == null || animator == null) return;

        Vector2 velocity = aiPath.velocity;

        if (velocity.sqrMagnitude > 0.01f)
        {
            animator.SetFloat("moveX", velocity.x);
            animator.SetFloat("moveY", velocity.y);
            
            animator.SetFloat("lastMoveX", velocity.x);
            animator.SetFloat("lastMoveY", velocity.y);
            
            animator.SetBool("isMoving", true);
        }
        else
        {
            animator.SetFloat("moveX", 0f);
            animator.SetFloat("moveY", 0f);
            
            animator.SetBool("isMoving", false);
        }
    }

    private void UpdateNeeds()
    {
        float hungerDecay = personality == PersonalityType.Lazy ? 0.5f : 1f;
        float energyDecay = personality == PersonalityType.Lazy ? 0.2f : 1f;
        float socialDecay = personality == PersonalityType.Social ? 1.5f : 0.5f;

        currentNeeds.Decay(hungerDecay, energyDecay, socialDecay);
    }

    private void EvaluateEmotion()
    {
        if (currentNeeds.energy < 20) currentEmotion = EmotionState.Tired;
        else if (currentNeeds.hunger < 30) currentEmotion = EmotionState.Angry;
        else if (currentNeeds.socialNeed < 20) currentEmotion = EmotionState.Sad;
        else if (currentNeeds.hunger > 80 && currentNeeds.energy > 80) currentEmotion = EmotionState.Happy;
        else currentEmotion = EmotionState.Neutral;
    }

    public bool CanInteractWithPlayerToday()
    {
        if (timeManager == null) return true;
        return lastInteractedDay != timeManager.day;
    }

    public void MarkInteractionDone()
    {
        if (timeManager != null)
        {
            lastInteractedDay = timeManager.day;
        }
    }

    private void OnPlayerInteracted()
    {
        if (!CanInteractWithPlayerToday()) return; 

        Debug.Log($"[Interaction] Player vừa nói chuyện với {gameObject.name}!");
        
        // Tăng mức độ giao tiếp xã hội
        currentNeeds.socialNeed = Mathf.Clamp(currentNeeds.socialNeed + 30f, 0, 100);

        // Cập nhật mức độ quen thuộc
        float friendshipBoost = personality == PersonalityType.Friendly ? 5f : 2f;
        playerRelationship.friendship = Mathf.Clamp(playerRelationship.friendship + friendshipBoost, -100, 100);
        playerRelationship.familiarity = Mathf.Clamp(playerRelationship.familiarity + 5f, 0, 100);


        float trustBoost = 0f;

        // Giao tiếp đủ nhiều mới tăng độ tin cậy
        if (playerRelationship.familiarity > 30f)
        {
            float baseTrust = 1f;
            if (personality == PersonalityType.Friendly) baseTrust = 2f;  
            else if (personality == PersonalityType.Greedy) baseTrust = 0.5f;

            trustBoost += baseTrust;
        }

        if (currentEmotion == EmotionState.Sad || currentEmotion == EmotionState.Angry || currentEmotion == EmotionState.Tired)
        {
            trustBoost += 5f;
            Debug.Log($"[Trust] {gameObject.name} rất cảm kích vì Player đã quan tâm lúc họ đang {currentEmotion}!");
        }

        if (trustBoost > 0f)
        {
            playerRelationship.trust = Mathf.Clamp(playerRelationship.trust + trustBoost, 0, 100);
        }

        MarkInteractionDone(); 
        TryDropItemForPlayer();
    }

    public void TryDropItemForPlayer()
    {
        if (Time.time - lastTradeTime < 10f) return; 

        if (giftItems == null || giftItems.Length == 0) return;

        float dropChance = 0.1f; 
        if (currentEmotion == EmotionState.Happy) dropChance += 0.3f; 
        if (playerRelationship.friendship > 30) dropChance += 0.4f;

        if (Random.value <= dropChance)
        {
            GameObject itemToDrop = giftItems[Random.Range(0, giftItems.Length)];
            Instantiate(itemToDrop, transform.position + new Vector3(0, -0.5f, 0), Quaternion.identity);
            
            playerRelationship.friendship -= 10f; 
            
            lastTradeTime = Time.time; 
        }
    }

    private void UpdateSchedule()
    {
        ScheduleTask activeTask = null;

        foreach (var task in dailySchedule)
        {
            if (currentHour >= task.startHour)
            {
                activeTask = task; 
            }
        }

        if (activeTask != null && currentTask != activeTask)
        {
            currentTask = activeTask;
            Debug.Log($"[Schedule] {gameObject.name} chuyển sang làm: {currentTask.action} lúc {currentHour}h");
        }
    }

    public NPCSaveData GetSaveData()
    {
        NPCSaveData data = new NPCSaveData();
        data.npcID = gameObject.name; // Dùng tên làm ID
        data.position = transform.position;

        data.hunger = currentNeeds.hunger;
        data.energy = currentNeeds.energy;
        data.socialNeed = currentNeeds.socialNeed;

        data.playerFriendship = playerRelationship.friendship;
        data.playerTrust = playerRelationship.trust;
        data.playerFamiliarity = playerRelationship.familiarity;

        data.lastInteractedDay = lastInteractedDay; 

        return data;
    }

    public void RestoreData(NPCSaveData data)
    {
        if (aiPath != null) aiPath.Teleport(data.position);
        else transform.position = data.position;

        currentNeeds.hunger = data.hunger;
        currentNeeds.energy = data.energy;
        currentNeeds.socialNeed = data.socialNeed;

        playerRelationship.friendship = data.playerFriendship;
        playerRelationship.trust = data.playerTrust;
        playerRelationship.familiarity = data.playerFamiliarity;

        lastInteractedDay = data.lastInteractedDay;
    }
}