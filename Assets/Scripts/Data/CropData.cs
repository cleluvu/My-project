using UnityEngine;

[CreateAssetMenu(fileName = "New Crop", menuName = "Data/Crop Data")]
public class CroptData : ScriptableObject
{
    public string cropID;
    public int daysToGrow = 3;
    public Sprite[] growthStages;
    public int dropItemID;
}
