using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FarmingManager : MonoBehaviour
{
    public Tilemap soilTilemap;
    public RuleTile ruleTile;

    public Dictionary<Vector3Int, CropData> farmData = new Dictionary<Vector3Int, CropData>();

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
                farmData[cellPos].plantType = plantType;
                farmData[cellPos].currentGrowState = 0;

                Vector3 spawnPos = soilTilemap.GetCellCenterWorld(cellPos);
                GameObject newPlant = Instantiate(seedPrefab, spawnPos, Quaternion.identity);
                newPlant.transform.SetParent(this.transform);

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
}
