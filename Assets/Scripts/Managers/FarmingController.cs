using System.Collections.Generic;
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
    
    // Bộ nhớ đệm dùng chung để tránh Memory Leak (Rác bộ nhớ RAM) khi cập nhật Sprite cây
    private Tile reusableCropTile;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Khởi tạo một Tile ảo duy nhất để xài đi xài lại, không tạo mới vô tội vạ nữa
        reusableCropTile = ScriptableObject.CreateInstance<Tile>();
    }

    private CroptData GetCroptData(string cropID)
    {
        return croptDataBase.Find(c => c.cropID == cropID);
    }

    public void TillSoil(Vector3Int cellPos)
    {
        if (!farmData.ContainsKey(cellPos))
        {
            farmData[cellPos] = new FarmTileData { position = cellPos, state = SoilState.Tilled, dryDaysCount = 0 };
            UpdateTileVisual(cellPos);
        }
    }

    public void WaterSoil(Vector3Int cellPos)
    {
        if (farmData.TryGetValue(cellPos, out FarmTileData tile))
        {
            if (tile.state == SoilState.Tilled)
            {
                tile.state = SoilState.Watered;
                tile.dryDaysCount = 0; // Reset số ngày khô về 0 ngay lập tức khi được tưới
                UpdateTileVisual(cellPos);
            }
        }
    }

    public bool PlantSeed(Vector3Int cellPos, string cropID)
    {
        if (farmData.TryGetValue(cellPos, out FarmTileData tile))
        {
            if ((tile.state == SoilState.Tilled || tile.state == SoilState.Watered) && string.IsNullOrEmpty(tile.plantedCropID))
            {
                tile.plantedCropID = cropID;
                tile.currentGrowthStage = 0;
                UpdateTileVisual(cellPos);
                return true;
            }
        }
        return false;
    }

    public void OnDayPassed()
    {
        List<Vector3Int> tilesToHarvest = new List<Vector3Int>();
        List<Vector3Int> tilesToResetToGrass = new List<Vector3Int>();

        // 1. Sao chép danh sách các vị trí đất hiện tại để tính toán an toàn
        List<Vector3Int> activePositions = new List<Vector3Int>(farmData.Keys);

        foreach (Vector3Int pos in activePositions)
        {
            FarmTileData tile = farmData[pos];

            if (!string.IsNullOrEmpty(tile.plantedCropID) && tile.state == SoilState.Watered)
            {
                CroptData crop = GetCroptData(tile.plantedCropID);
                if (crop != null && tile.currentGrowthStage < crop.daysToGrow)
                {
                    tile.currentGrowthStage++;

                    if (tile.currentGrowthStage >= crop.daysToGrow)
                    {
                        tilesToHarvest.Add(pos);
                    }
                }
            }

            // Xử lý chuyển đổi trạng thái nước cho ngày hôm sau
            if (tile.state == SoilState.Watered)
            {
                tile.state = SoilState.Tilled; 
                tile.dryDaysCount = 0;        
            }
            else if (tile.state == SoilState.Tilled)
            {
                tile.dryDaysCount++;

                if (tile.dryDaysCount >= 3)
                {
                    tilesToResetToGrass.Add(pos);
                }
            }
        }

        // 2. Kích hoạt thu hoạch cho các cây chín
        foreach (Vector3Int pos in tilesToHarvest)
        {
            HarvestCrop(pos);
        }

        // 3. Ép các ô đất bỏ hoang quá 3 ngày biến mất
        foreach (Vector3Int pos in tilesToResetToGrass)
        {
            ResetTileToGrass(pos);
        }

        HashSet<Vector3Int> allPositionsToUpdate = new HashSet<Vector3Int>(activePositions);
        allPositionsToUpdate.UnionWith(tilesToResetToGrass);

        foreach (Vector3Int pos in allPositionsToUpdate)
        {
            UpdateTileVisual(pos);
        }
        
        Debug.Log("[FarmingController] Đã xử lý tính toán qua ngày và cập nhật lại toàn bộ ô đất!");
    }

    private void ResetTileToGrass(Vector3Int pos)
    {
        if (farmData.ContainsKey(pos))
        {
            cropTilemap.SetTile(pos, null);
            
            farmData.Remove(pos);
            
            farmingTilemap.SetTile(pos, null);
            
            Debug.Log($"Ô đất tại {pos} đã bị bỏ khô 3 ngày và biến trở lại thành cỏ!");
        }
    }

    private void HarvestCrop(Vector3Int pos)
    {
        if (!farmData.ContainsKey(pos)) return;

        FarmTileData tile = farmData[pos];
        CroptData crop = GetCroptData(tile.plantedCropID);

        if (crop != null)
        {
            Vector3 spawnPos = farmingTilemap.GetCellCenterWorld(pos);
            GameObject itemPrefab = itemDictionary.GetItemPrefab(crop.dropItemID);
            if (itemPrefab != null)
            {
                Instantiate(itemPrefab, spawnPos, Quaternion.identity);
            }

            // Thu hoạch xong reset về đất trống (ngày khô tính lại từ đầu)
            tile.plantedCropID = "";
            tile.currentGrowthStage = 0;
            tile.dryDaysCount = 0; 
        }
    }

    public void UpdateTileVisual(Vector3Int cellPos)
    {
        // Kiểm tra phòng thủ đề phòng ô này vừa bị xóa khỏi Dictionary ở bước ResetGrass
        if (!farmData.TryGetValue(cellPos, out FarmTileData tile))
        {
            farmingTilemap.SetTile(cellPos, null);
            cropTilemap.SetTile(cellPos, null);
            return;
        }

        // Cập nhật mặt đất
        if (tile.state == SoilState.Tilled) farmingTilemap.SetTile(cellPos, tilledTile);
        else if (tile.state == SoilState.Watered) farmingTilemap.SetTile(cellPos, wateredTile);
        else farmingTilemap.SetTile(cellPos, null);

        // Cập nhật cây trồng
        if (!string.IsNullOrEmpty(tile.plantedCropID))
        {
            CroptData crop = GetCroptData(tile.plantedCropID);
            if (crop != null && crop.growthStages != null && crop.growthStages.Length > 0)
            {
                int spriteIndex = Mathf.Clamp(tile.currentGrowthStage, 0, crop.growthStages.Length - 1);
                
                reusableCropTile.sprite = crop.growthStages[spriteIndex];
                cropTilemap.SetTile(cellPos, reusableCropTile);
            }
        }
        else
        {
            cropTilemap.SetTile(cellPos, null);
        }
    }

    public List<FarmTileData> GetFarmSaveData()
    {
        return new List<FarmTileData>(farmData.Values);
    }

    public void RestoreFarmData(List<FarmTileData> saveData)
    {
        farmData.Clear();
        farmingTilemap.ClearAllTiles();
        cropTilemap.ClearAllTiles();

        if (saveData != null && saveData.Count > 0)
        {
            foreach (var tile in saveData)
            {
                farmData[tile.position] = tile;
                UpdateTileVisual(tile.position);
            }
        }
        Debug.Log("Đọc lại dữ liệu nông trại thành công");
    }
}