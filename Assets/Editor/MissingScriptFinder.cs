using UnityEditor;
using UnityEngine;
using System.Linq;

public static class MissingScriptFinder
{
    [MenuItem("Tools/Validation/Find Missing Scripts In Open Scenes")]
    private static void FindMissingScriptsInOpenScenes()
    {
        int totalMissing = 0;
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (GameObject go in allObjects)
        {
            int missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
            if (missingCount <= 0)
            {
                continue;
            }

            totalMissing += missingCount;
            Debug.LogWarning($"Missing script x{missingCount} on scene object: {GetHierarchyPath(go)}", go);
        }

        if (totalMissing == 0)
        {
            Debug.Log("No missing scripts found in currently loaded scenes.");
            return;
        }

        Debug.LogWarning($"Found total missing script references in open scenes: {totalMissing}");
    }

    [MenuItem("Tools/Validation/Find Missing Scripts In Selected Assets")]
    private static void FindMissingScriptsInSelectedAssets()
    {
        int totalMissing = 0;
        foreach (Object selected in Selection.objects)
        {
            if (selected is not GameObject root)
            {
                continue;
            }

            GameObject[] children = root.GetComponentsInChildren<Transform>(true).Select(t => t.gameObject).ToArray();
            foreach (GameObject go in children)
            {
                int missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
                if (missingCount <= 0)
                {
                    continue;
                }

                totalMissing += missingCount;
                Debug.LogWarning($"Missing script x{missingCount} on selected asset object: {GetHierarchyPath(go)}", go);
            }
        }

        if (totalMissing == 0)
        {
            Debug.Log("No missing scripts found in selected assets.");
            return;
        }

        Debug.LogWarning($"Found total missing script references in selected assets: {totalMissing}");
    }

    [MenuItem("Tools/Validation/Remove Missing Scripts In Open Scenes")]
    private static void RemoveMissingScriptsInOpenScenes()
    {
        int removedTotal = 0;
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (GameObject go in allObjects)
        {
            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            if (removed > 0)
            {
                removedTotal += removed;
                Debug.LogWarning($"Removed missing script x{removed} on scene object: {GetHierarchyPath(go)}", go);
            }
        }

        if (removedTotal == 0)
        {
            Debug.Log("No missing scripts to remove in currently loaded scenes.");
            return;
        }

        Debug.LogWarning($"Removed total missing script references in open scenes: {removedTotal}");
    }

    [MenuItem("Tools/Validation/Remove Missing Scripts In Selected Assets")]
    private static void RemoveMissingScriptsInSelectedAssets()
    {
        int removedTotal = 0;

        foreach (Object selected in Selection.objects)
        {
            if (selected is not GameObject root)
            {
                continue;
            }

            GameObject[] children = root.GetComponentsInChildren<Transform>(true).Select(t => t.gameObject).ToArray();
            foreach (GameObject go in children)
            {
                int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                if (removed > 0)
                {
                    removedTotal += removed;
                    Debug.LogWarning($"Removed missing script x{removed} on selected object: {GetHierarchyPath(go)}", go);
                }
            }
        }

        if (removedTotal == 0)
        {
            Debug.Log("No missing scripts to remove in selected assets.");
            return;
        }

        Debug.LogWarning($"Removed total missing script references in selected assets: {removedTotal}");
    }

    private static string GetHierarchyPath(GameObject go)
    {
        string path = go.name;
        Transform current = go.transform.parent;
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }

        return path;
    }
}
