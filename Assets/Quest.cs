using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// --- ĐỊNH NGHĨA LOẠI PHẦN THƯỞNG (TẬP #29) ---
public enum RewardType
{
    Gold,
    Item
}

[System.Serializable]
public class QuestReward
{
    public RewardType type;
    public int itemID; // Chỉ sử dụng nếu type là Item
    public int amount; // Số lượng Vàng hoặc Số lượng Vật phẩm thưởng
}

[CreateAssetMenu(menuName = "Quests/Quest")]
public class Quest : ScriptableObject
{
    public string questID;
    public string questName;
    public string description;
    public List<QuestObjective> objectives;

    // --- DANH SÁCH PHẦN THƯỞNG CẤU HÌNH TRÊN INSPECTOR (TẬP #29) ---
    [Header("Quest Rewards")]
    public List<QuestReward> rewards;

    private void OnEnable()
    {
        // Đảm bảo ID nhiệm vụ là duy nhất
        if (string.IsNullOrEmpty(questID))
        {
            questID = questName + Guid.NewGuid().ToString();
        }
    }
}

[System.Serializable]
public class QuestObjective
{
    public string objectiveID;
    public string description;
    public ObjectiveType type;
    public int requiredAmount;
    public int currentAmount;

    public bool IsCompleted() => currentAmount >= requiredAmount;
}

[System.Serializable]
public enum ObjectiveType
{
    CollectItem,
    DefeatEnemy,
    ReachLocation,
    TalkToNPC,
    Custom
}

[System.Serializable]
public class QuestProgress
{
    public Quest quest;
    public List<QuestObjective> objectives;

    public QuestProgress(Quest quest)
    {
        this.quest = quest;
        objectives = new List<QuestObjective>();

        foreach (var obj in quest.objectives)
        {
            objectives.Add(new QuestObjective
            {
                objectiveID = obj.objectiveID,
                description = obj.description,
                type = obj.type,
                requiredAmount = obj.requiredAmount,
                currentAmount = 0
            });
        }
    }

    public bool IsCompleted => objectives.TrueForAll(o => o.IsCompleted());

    public string QuestID => quest.questID;
}