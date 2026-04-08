using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FarmingManager : MonoBehaviour
{
    // Tilemap
    public Tilemap soilTilemap;
    public RuleTile ruleTile;

    // Time
    public DayAndNight dayAndNight;

    // Data
    public Dictionary<Vector3Int, CropData> farmData = new Dictionary<Vector3Int, CropData>();

    void Update()
    {
        if(dayAndNight == null) return;

        float timeAdded = Time.deltaTime * dayAndNight.timeMultiplier;

        foreach(var fdt in farmData)
        {
            CropData crop = fdt.Value;

            if(crop.plantType != "" && crop.plantGameObject != null)
            {
                Plants plantScript = crop.plantGameObject.GetComponent<Plants>();

                if (plantScript.isWatered)
                {
                    crop.growthTime += timeAdded;

                    float totalTimeToGrow = plantScript.dayToGrow * dayAndNight.dayDuration;
                    float growPercentage = crop.growthTime / totalTimeToGrow;

                    int calculatedState = Mathf.FloorToInt(growPercentage * plantScript.stateImages.Count);
                    if (calculatedState >= plantScript.stateImages.Count) calculatedState = plantScript.stateImages.Count - 1;

                    if(calculatedState > crop.currentGrowState)
                    {
                        crop.currentGrowState = calculatedState;
                        plantScript.UpdateGrowthState(crop.currentGrowState);
                    }
                }
            }
        }
    }

    public void TillGround(Vector3 targetPos)
    {
        Vector3Int cellPos = soilTilemap.WorldToCell(targetPos);

        if(farmData.ContainsKey(cellPos) && farmData[cellPos].isTilled)
        {
            Debug.Log("Đào ô này rồi");
            return;
        }

        CropData newData = new CropData();
        farmData[cellPos] = newData;

        soilTilemap.SetTile(cellPos, ruleTile);
        Debug.Log("Đã cuốc vào ô: " + cellPos);
    }

    public void PlantSeed(Vector3 targetPos, GameObject seedPrefab, string plantType)
    {
        Vector3Int cellPos = soilTilemap.WorldToCell(targetPos);
        if (farmData.ContainsKey(cellPos) && farmData[cellPos].isTilled)
        {
            if(farmData[cellPos].plantType == "")
            {
                // Update Data
                farmData[cellPos].plantType = plantType;
                farmData[cellPos].currentGrowState = 0;
                farmData[cellPos].growthTime = 0f;

                // Update View
                Vector3 spawnPos = soilTilemap.GetCellCenterWorld(cellPos);
                GameObject newPlant = Instantiate(seedPrefab, spawnPos, Quaternion.identity);
                newPlant.transform.SetParent(this.transform);

                newPlant.GetComponent<Plants>().Initialize();

                farmData[cellPos].plantGameObject = newPlant;

                Debug.Log("Đã trồng cây " + plantType + " tại ô: " + cellPos);
            }
            else
            {
                Debug.Log("Ô này có trồng rồi");
            }
        }
        else
        {
            Debug.Log("Chưa gieo hạt được, cuốc đất trước");
        }
    }

    public void WaterGround(Vector3 targetPos)
    {
        Vector3Int cellPos = soilTilemap.WorldToCell(targetPos);
        if(farmData.ContainsKey(cellPos) && farmData[cellPos].plantType != "")
        {
            farmData[cellPos].plantGameObject.GetComponent<Plants>().GiveWater();
        }
    }
}
