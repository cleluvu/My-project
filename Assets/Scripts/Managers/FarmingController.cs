using System.Collections.Generic;
using System.IO;
using UnityEditor.MPE;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FarmingController : MonoBehaviour
{
    public static FarmingController Instance; 

    [Header("Tilemaps")]
    public Tilemap farmingTilemap;
    public Tilemap cropTilemap;

    [Header("Tiles")]
    public TileBase tilledTile;
    public TileBase wateredTile;

    [Header("Data")]
    public List<CroptData> croptDataBase;
    public ItemDictionary itemDictionary;

    private Dictionary<Vector3Int, FarmTileData> farmData = new Dictionary<Vector3Int, FarmTileData>();

    private void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private CroptData GetCroptData(string cropID)
    {
        return croptDataBase.Find(c => c.cropID == cropID);
    }

    public void TillSoil(Vector3Int cellPos)
    {
        if (!farmData.ContainsKey(cellPos))
        {
            farmData[cellPos] = new FarmTileData{position = cellPos, state = SoilState.Tilled};
            UpdateTileVisual(cellPos);
        }
    }

    public void WaterSoil(Vector3Int cellPos)
    {
        if(farmData.TryGetValue(cellPos, out FarmTileData tile))
        {
            if(tile.state == SoilState.Tilled)
            {
                tile.state = SoilState.Watered;
                UpdateTileVisual(cellPos);
            }
        }
    }

    public void PlantSeed(Vector3Int cellPos, string cropID)
    {
        if(farmData.TryGetValue(cellPos, out FarmTileData tile))
        {
            if((tile.state == SoilState.Tilled || tile.state == SoilState.Watered) && string.IsNullOrEmpty(tile.plantedCropID))
            {
                tile.plantedCropID = cropID;
                tile.currentGrowthStage = 0;
                UpdateTileVisual(cellPos);
            }
        }
    }

    // Kiểm soát cây lớn lên
    public void OnDayPassed()
    {
        List<Vector3Int> tilesToHarvest = new List<Vector3Int>();

        foreach(var kvp in farmData)
        {
            FarmTileData tile = kvp.Value;

            if(!string.IsNullOrEmpty(tile.plantedCropID) && tile.state == SoilState.Watered)
            {
                CroptData crop = GetCroptData(tile.plantedCropID);
                if(crop != null && tile.currentGrowthStage < crop.daysToGrow)
                {
                    Debug.Log("Cây lớn lên nè");
                    tile.currentGrowthStage ++;

                    if(tile.currentGrowthStage >= crop.daysToGrow)
                    {
                        tilesToHarvest.Add(kvp.Key);
                    }
                }
            }

            if(tile.state == SoilState.Watered)
            {
                tile.state = SoilState.Tilled;
            }

            UpdateTileVisual(kvp.Key);
        }
        foreach(Vector3Int pos in tilesToHarvest)
        {
            HarvestCrop(pos);
        }
    }

    // Thu hoạch
    private void HarvestCrop(Vector3Int pos)
    {
        FarmTileData tile = farmData[pos];
        CroptData crop = GetCroptData(tile.plantedCropID);

        if(crop != null)
        {
            // Spawn ra tại chỗ trồng
            Vector3 spawnPos = farmingTilemap.GetCellCenterWorld(pos);
            GameObject itemPrefab = itemDictionary.GetItemPrefab(crop.dropItemID);
            if(itemPrefab != null)
            {
                Instantiate(itemPrefab, spawnPos, Quaternion.identity);
            }

            // Reset lại ô đất
            tile.plantedCropID = "";
            tile.currentGrowthStage = 0;
            UpdateTileVisual(pos);
        }
    }

    // Cập nhật lại tilemap
    public void UpdateTileVisual(Vector3Int cellPos)
    {
        FarmTileData tile = farmData[cellPos];

        if(tile.state == SoilState.Tilled) farmingTilemap.SetTile(cellPos, tilledTile);
        else if(tile.state == SoilState.Watered) farmingTilemap.SetTile(cellPos, wateredTile);
        else farmingTilemap.SetTile(cellPos, null);

        farmingTilemap.RefreshTile(cellPos);

        if (!string.IsNullOrEmpty(tile.plantedCropID))
        {
            CroptData crop = GetCroptData(tile.plantedCropID);
            int spriteIndex = Mathf.Clamp(tile.currentGrowthStage, 0, crop.growthStages.Length - 1);

            Tile cropTile = ScriptableObject.CreateInstance<Tile>();
            cropTile.sprite = crop.growthStages[spriteIndex];
            cropTilemap.SetTile(cellPos, cropTile);
        }
        else
        {
            cropTilemap.SetTile(cellPos, null);
        }
    }

    // Save game

    public List<FarmTileData> GetFarmSaveData()
    {
        return new List<FarmTileData>(farmData.Values);
    }

    public void RestoreFarmData(List<FarmTileData> saveData)
    {
        // Clear trước khi ghi lại
        farmData.Clear();
        farmingTilemap.ClearAllTiles();
        cropTilemap.ClearAllTiles();

        if(saveData != null && saveData.Count > 0)
        {
            foreach(var tile in saveData)
            {
                farmData[tile.position] = tile;
                UpdateTileVisual(tile.position);
            }
        }
        Debug.Log("Đọc lại dữ liệu thành công");
    }
}
