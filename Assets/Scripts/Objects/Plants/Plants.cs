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
    private SpriteRenderer spriteRenderer;
    private DayAndNight dayAndNight;
    private float currentTime = 0;
    private int currentStateIndex = 0;

    void Awake()
    {   
        GameObject timeManager = GameObject.Find("TimeManager");
        dayAndNight = timeManager.GetComponent<DayAndNight>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        plantState = PlantState.WithoutWater;
        offerWater.SetActive(true);
        UpdateSprite();
    }

    void Update()
    {
        if(plantState == PlantState.WithoutWater) return;
        else
        {
            offerWater.SetActive(false);
        }

        if(currentStateIndex >= stateImages.Count - 1)
        {
            plantState = PlantState.Harvesting;
            return;
        }

        float timeToGrow = dayToGrow * dayAndNight.dayDuration;
        currentTime += dayAndNight.speedTime;

        float growPercentage = currentTime / timeToGrow;
        int stateIndex = Mathf.FloorToInt(growPercentage * stateImages.Count);
        if(stateIndex >= stateImages.Count)
        {
            stateIndex = stateImages.Count - 1;
        }

        if(stateIndex > currentStateIndex)
        {
            currentStateIndex = stateIndex;
            UpdateSprite();
        }
    }

    void UpdateSprite()
    {
        if(stateImages.Count > 0 && currentStateIndex < stateImages.Count)
        {
            spriteRenderer.sprite = stateImages[currentStateIndex];
        }
    }

    public void ObjectCollected()
    {
        gameObject.SetActive(false);
    }
}
