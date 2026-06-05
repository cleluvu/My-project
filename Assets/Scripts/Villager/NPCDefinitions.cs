using UnityEngine;

[System.Serializable]
public class ScheduleTask
{
    [Header("Bắt đầu lúc (0 - 23 giờ)")]
    [Range(0, 23)] public int startHour;
    
    public NPCAction action;
    public Transform targetLocation;
}

public enum PersonalityType
{
    Friendly, Lazy, Greedy, Social, Emotional
}

public enum EmotionState
{
    Happy, Neutral, Sad, Angry, Tired
}

public enum NPCAction
{
    Talk, Trade, Wander, Rest, Work, GoHome, IgnorePlayer
}

[System.Serializable]
public class Needs
{
    [Range(0, 100)] public float hunger = 100f;
    [Range(0, 100)] public float energy = 100f;
    [Range(0, 100)] public float socialNeed = 100f;

    public void Decay(float hungerRate, float energyRate, float socialRate)
    {
        hunger = Mathf.Clamp(hunger - hungerRate * Time.deltaTime, 0, 100);
        energy = Mathf.Clamp(energy - energyRate * Time.deltaTime, 0, 100);
        socialNeed = Mathf.Clamp(socialNeed - socialRate * Time.deltaTime, 0, 100);
    }
}

[System.Serializable]
public class Relationship
{
    [Range(-100, 100)] public float friendship = 0f;
    [Range(0, 100)] public float trust = 0f;
    [Range(0, 100)] public float familiarity = 0f;
}