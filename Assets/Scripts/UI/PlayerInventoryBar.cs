using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventoryBar : MonoBehaviour
{
    public GameObject playerGameObject;
    public List<Text> listInventoryText;
    private Player player;

    void Awake()
    {
        player = playerGameObject.GetComponent<Player>();
    }

    void Update()
    {
        for(int i = 0; i < listInventoryText.Count; i++)
        {
            if(listInventoryText[i] != null)
            {
                listInventoryText[i].text = player.items[i].ToString();
            }
        }
    }
}
