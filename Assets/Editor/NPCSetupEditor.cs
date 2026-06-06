using UnityEngine;
using UnityEditor;
using Pathfinding;

public class NPCSetupEditor : EditorWindow
{
    [MenuItem("Tools/Setup NPC Components")]
    public static void ShowWindow()
    {
        GetWindow<NPCSetupEditor>("NPC Setup");
    }

    void OnGUI()
    {
        GUILayout.Label("NPC Component Setup", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Fix All NPCs in Scene"))
        {
            FixAllNPCs();
        }
        
        GUILayout.Space(10);
        GUILayout.Label("This will add missing NPC, AIPath, and Animator components to all GameObjects with NPCCoreSystems.", EditorStyles.wordWrappedLabel);
    }

    static void FixAllNPCs()
    {
        NPCCoreSystems[] allNPCSystems = Object.FindObjectsByType<NPCCoreSystems>(FindObjectsSortMode.None);
        int fixedCount = 0;

        foreach (var npcSystem in allNPCSystems)
        {
            GameObject npcObj = npcSystem.gameObject;
            bool needsSave = false;

            // Add NPC component if missing
            if (npcObj.GetComponent<NPC>() == null)
            {
                npcObj.AddComponent<NPC>();
                needsSave = true;
                Debug.Log($"Added NPC component to {npcObj.name}");
            }

            // Add AIPath if missing
            if (npcObj.GetComponent<AIPath>() == null)
            {
                AIPath path = npcObj.AddComponent<AIPath>();
                path.gravity = new Vector3(0, -3f, 0);
                needsSave = true;
                Debug.Log($"Added AIPath component to {npcObj.name}");
            }

            // Add Animator if missing
            if (npcObj.GetComponent<Animator>() == null)
            {
                npcObj.AddComponent<Animator>();
                needsSave = true;
                Debug.Log($"Added Animator component to {npcObj.name}");
            }

            if (needsSave)
            {
                fixedCount++;
                EditorUtility.SetDirty(npcObj);
            }
        }

        if (fixedCount > 0)
        {
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Success", $"Fixed {fixedCount} NPCs!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Info", "All NPCs already have required components.", "OK");
        }
    }
}
