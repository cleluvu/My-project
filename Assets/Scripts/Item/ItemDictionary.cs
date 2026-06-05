using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class ItemDictionary : MonoBehaviour
{
    public List<Item> itemPrefabs;
    private Dictionary<int, GameObject> itemDictionary;

    private void Awake()
    {
        itemDictionary = new Dictionary<int, GameObject>();

        // Tự tạo id
        for(int i = 0; i < itemPrefabs.Count; i++)
        {
            itemPrefabs[i].ID = i + 1;
        }

        foreach(Item item in itemPrefabs)
        {
            itemDictionary[item.ID] = item.gameObject;
        }
    }
    
    public GameObject GetItemPrefab(int itemID)
    {
        itemDictionary.TryGetValue(itemID, out GameObject prefab);
        
        if(prefab == null)
        {
            Debug.LogWarning("Item not found");
        }

        return prefab;
    }
}
