using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SocialMatrixSaveData
{
    public string targetNpcID;
    public float friendship;
    public float trust;
    public float familiarity;
}

[System.Serializable]
public class NPCSaveData
{
    public string npcID;
    public Vector3 position;
    
    // Sinh tồn
    public float hunger;
    public float energy;
    public float socialNeed;

    // Quan hệ với Player
    public float playerFriendship;
    public float playerTrust;
    public float playerFamiliarity;

    // State
    public int lastInteractedDay;

    // Social Matrix
    public List<SocialMatrixSaveData> socialMatrixList = new List<SocialMatrixSaveData>();
}