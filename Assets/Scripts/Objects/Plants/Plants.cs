using System;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
public class Plants : MonoBehaviour
{
    public List<Sprite> stateImages;
    public GameObject offerWater;
    public float dayToGrow = 5;

    public PlantType plantType;
    public PlantState plantState;

    public float capacity = 5;
    public bool isWatered = false;

    private SpriteRenderer spriteRenderer;

    public void Initialize()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        isWatered = false;
        plantState = PlantState.WithoutWater;
        offerWater.SetActive(true);
        UpdateSprite(0);
    }

    public void UpdateGrowthState(int newState)
    {
        if(newState >= stateImages.Count - 1)
        {
            plantState = PlantState.Harvesting;
            UpdateSprite(stateImages.Count - 1);
            return;
        }
        UpdateSprite(newState);
    }

    public void GiveWater()
    {
        isWatered = true;
        offerWater.SetActive(false);
    }
    
    void UpdateSprite(int index)
    {
        if (stateImages.Count > 0 && index < stateImages.Count)
        {
            spriteRenderer.sprite = stateImages[index];
        }
    }

    public void ObjectCollected()
    {
        gameObject.SetActive(false);
    }
}
