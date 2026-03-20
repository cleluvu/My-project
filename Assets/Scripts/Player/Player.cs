using System.Collections.Generic;
using UnityEngine;

public class Player: MonoBehaviour
{
    public List<int> items;
    public int amountOfItem = 2;

    void Awake()
    {
        items = new List<int>(amountOfItem);
        for(int i = 0; i < amountOfItem; i++)
        {
            items.Add(0);
        }   
    }
}
