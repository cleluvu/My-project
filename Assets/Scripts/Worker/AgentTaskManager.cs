using System.Collections.Generic;
using UnityEngine;

// Đưa các Enum ra ngoài để dùng chung toàn cục
public enum AgentRole 
{
    AnimalCaretaker, 
    Farmer,
    Gatherer 
}

public enum FarmAction
{
    None,
    Watering,
    Planting
}

public class AgentTaskManager : MonoBehaviour
{
    // Cơ chế đặt lịch
    private static HashSet<Entity> claimedEntities = new HashSet<Entity>();
    private static HashSet<Vector3Int> claimedTiles = new HashSet<Vector3Int>();
    private static HashSet<Item> claimedItems = new HashSet<Item>();
    private static HashSet<CollectedObject> claimedResources = new HashSet<CollectedObject>(); 

    [Header("Phân công công việc")]
    public AgentRole role; 
    public Transform homeTarget;
    
    [Header("Cấu hình Trồng trọt")]
    public string seedToPlantID; 
    public int seedID;

    [Header("Cấu hình Khai thác")]
    public float hitCooldown = 1f; 
    private float hitTimer = 1f;   

    [Header("Hợp đồng thuê")]
    public string hiredEmployeeID;
    public int remainingDays = 0;
    public bool isLeaving = false;
    private Transform dockTarget;

    // Các biến nhớ mục tiêu
    private Entity currentTargetEntity;
    private Item currentTargetItem;
    private Vector3Int? currentTargetTile = null; 
    private FarmAction currentFarmAction = FarmAction.None; 
    private CollectedObject currentTargetResource; 
    private Transform dummyMarker; 

    private MyAgent myAgent;

    private void Start()
    {
        DayAndNight timeManager = Object.FindFirstObjectByType<DayAndNight>();
        if (timeManager != null) timeManager.onNewDayStarted.AddListener(OnNewDay);
    }

    private void Awake()
    {
        myAgent = GetComponent<MyAgent>();
        GameObject markerObj = new GameObject(gameObject.name + "_TargetMarker");
        dummyMarker = markerObj.transform;
    }

    private void OnDestroy() { ResetAllTargets(); }
    private void OnDisable() { ResetAllTargets(); }

    public void SetupHiredAgent(string id, int days, Transform dock, Transform home)
    {
        hiredEmployeeID = id;
        remainingDays = days;
        dockTarget = dock;
        homeTarget = home;
        isLeaving = false;
    }

    private void OnNewDay()
    {
        if (isLeaving) return;

        remainingDays--;
        if (remainingDays <= 0)
        {
            isLeaving = true;
            ResetAllTargets();
        }
    }

    public Transform GetActiveTarget()
    {
        // Bị sa thải
        if (isLeaving && dockTarget != null) return dockTarget;

        // Quản lý thời gian làm việc
        DayAndNight dayAndNight = Object.FindAnyObjectByType<DayAndNight>();
        bool isWorkTime = false; 
        if (dayAndNight != null)
        {
            float currentHour = (dayAndNight.currentTime / dayAndNight.dayDuration) * 24f;
            isWorkTime = (currentHour >= 6f && currentHour <= 21f);
        }

        // Đi về 
        if (!isWorkTime) 
        {
            ResetAllTargets();
            return homeTarget;
        }

        // Xác định chỗ con vật
        if (role == AgentRole.AnimalCaretaker)
        {
            if (currentTargetEntity != null && currentTargetEntity.isHungry && 
                InventoryController.Instance != null && InventoryController.Instance.HasItemGlobal(currentTargetEntity.foodItemID))
                return currentTargetEntity.transform;
            
            ResetAllTargets(); 
            Entity bestEntity = null;
            float closestDist = Mathf.Infinity;
            Entity[] allEntities = Object.FindObjectsByType<Entity>(FindObjectsSortMode.None);

            foreach (Entity entity in allEntities)
            {
                if (entity.isHungry && !claimedEntities.Contains(entity) && 
                    InventoryController.Instance != null && InventoryController.Instance.HasItemGlobal(entity.foodItemID))
                {
                    float distance = Vector2.Distance(transform.position, entity.transform.position);
                    if (distance < closestDist)
                    {
                        closestDist = distance;
                        bestEntity = entity;
                    }
                }
            }
            if (bestEntity != null) 
            {
                currentTargetEntity = bestEntity;
                claimedEntities.Add(bestEntity); 
                return currentTargetEntity.transform;
            }
        }

        // Xác định chỗ cây trồng
        else if (role == AgentRole.Farmer)
        {
            List<FarmTileData> farmDataList = FarmingController.Instance.GetFarmSaveData();

            if (currentTargetTile.HasValue)
            {
                bool stillValid = false;
                foreach (var tile in farmDataList)
                {
                    if (tile.position == currentTargetTile.Value)
                    {
                        if (currentFarmAction == FarmAction.Watering && tile.state == SoilState.Tilled && !string.IsNullOrEmpty(tile.plantedCropID)) stillValid = true;
                        else if (currentFarmAction == FarmAction.Planting && (tile.state == SoilState.Tilled || tile.state == SoilState.Watered) && string.IsNullOrEmpty(tile.plantedCropID) && InventoryController.Instance.HasItemGlobal(seedID)) stillValid = true;
                        break;
                    }
                }
                if (stillValid) return dummyMarker;
            }

            ResetAllTargets(); 
            Vector3Int? bestTile = null;
            FarmAction bestAction = FarmAction.None;
            float closestDist = Mathf.Infinity;

            // Tìm đất để tưới
            foreach (var tile in farmDataList)
            {
                if (tile.state == SoilState.Tilled && !string.IsNullOrEmpty(tile.plantedCropID) && !claimedTiles.Contains(tile.position))
                {
                    Vector3 tileWorldPos = FarmingController.Instance.farmingTilemap.GetCellCenterWorld(tile.position);
                    float distance = Vector2.Distance(transform.position, tileWorldPos);
                    if (distance < closestDist)
                    {
                        closestDist = distance;
                        bestTile = tile.position;
                        bestAction = FarmAction.Watering;
                        dummyMarker.position = tileWorldPos;
                    }
                }
            }

            // Tìm đất để trồng
            if (!bestTile.HasValue && !string.IsNullOrEmpty(seedToPlantID) && InventoryController.Instance.HasItemGlobal(seedID))
            {
                closestDist = Mathf.Infinity;
                foreach (var tile in farmDataList)
                {
                    if ((tile.state == SoilState.Tilled || tile.state == SoilState.Watered) && string.IsNullOrEmpty(tile.plantedCropID) && !claimedTiles.Contains(tile.position))
                    {
                        Vector3 tileWorldPos = FarmingController.Instance.farmingTilemap.GetCellCenterWorld(tile.position);
                        float distance = Vector2.Distance(transform.position, tileWorldPos);
                        if (distance < closestDist)
                        {
                            closestDist = distance;
                            bestTile = tile.position;
                            bestAction = FarmAction.Planting; 
                            dummyMarker.position = tileWorldPos;
                        }
                    }
                }
            }

            if (bestTile.HasValue) 
            {
                currentTargetTile = bestTile.Value;
                currentFarmAction = bestAction;
                claimedTiles.Add(bestTile.Value); 
                return dummyMarker;
            }
        }

        // Đi khai thác và nhặt đồ
        else if (role == AgentRole.Gatherer)
        {
            if (currentTargetResource != null && currentTargetResource.HP > 0) return currentTargetResource.transform;

            ResetAllTargets(); 
            CollectedObject bestResource = null;
            float closestDist = Mathf.Infinity;
            CollectedObject[] allResources = Object.FindObjectsByType<CollectedObject>(FindObjectsSortMode.None);

            foreach (CollectedObject res in allResources)
            {
                if (res.HP > 0 && !claimedResources.Contains(res))
                {
                    float distance = Vector2.Distance(transform.position, res.transform.position);
                    if (distance < closestDist)
                    {
                        closestDist = distance;
                        bestResource = res;
                    }
                }
            }
            if (bestResource != null) 
            {
                currentTargetResource = bestResource;
                claimedResources.Add(bestResource); 
                return currentTargetResource.transform;
            }

            if (currentTargetItem != null && currentTargetItem.gameObject.activeInHierarchy) return currentTargetItem.transform;

            Item bestItem = null;
            float closestItemDist = Mathf.Infinity;
            Item[] allItems = Object.FindObjectsByType<Item>(FindObjectsSortMode.None);

            foreach (Item item in allItems)
            {
                if (item.CompareTag("Item") && !claimedItems.Contains(item)) 
                {
                    float distance = Vector2.Distance(transform.position, item.transform.position);
                    if (distance < closestItemDist)
                    {
                        closestItemDist = distance;
                        bestItem = item;
                    }
                }
            }
            if (bestItem != null) 
            {
                currentTargetItem = bestItem;
                claimedItems.Add(bestItem); 
                return currentTargetItem.transform;
            }
        }

        return homeTarget;
    }

    private void ResetAllTargets()
    {
        if (currentTargetEntity != null) claimedEntities.Remove(currentTargetEntity);
        if (currentTargetTile.HasValue) claimedTiles.Remove(currentTargetTile.Value);
        if (currentTargetItem != null) claimedItems.Remove(currentTargetItem);
        if (currentTargetResource != null) claimedResources.Remove(currentTargetResource);

        currentTargetEntity = null;
        currentTargetItem = null;
        currentTargetTile = null;
        currentTargetResource = null;
        currentFarmAction = FarmAction.None;
    }

    public void HandleTasksUpdate()
    {
        // Bỏ nhà ra đi
        if (isLeaving && dockTarget != null)
        {
            float distToDock = Vector2.Distance(transform.position, dockTarget.position);
            if (distToDock < 1.0f)
            {
                HireManager.Instance.RemoveAgent(this);
                Destroy(gameObject);
            }
            return;
        }

        // Làm nông
        if (role == AgentRole.Farmer && currentTargetTile.HasValue)
        {
            float distToTile = Vector2.Distance(transform.position, dummyMarker.position);
            if (distToTile < 0.8f) 
            {
                myAgent.TriggerActionAnim();

                if (currentFarmAction == FarmAction.Watering) 
                {
                    FarmingController.Instance.WaterSoil(currentTargetTile.Value);
                }
                else if (currentFarmAction == FarmAction.Planting) 
                {
                    if (InventoryController.Instance.HasItemGlobal(seedID))
                    {
                        bool isPlanted = FarmingController.Instance.PlantSeed(currentTargetTile.Value, seedToPlantID);
                        
                        if (isPlanted)
                        {
                            InventoryController.Instance.RemoveItemGlobal(seedID, 1);
                            Debug.Log("Farmer đã trồng và trừ đi 1 hạt giống!");
                        }
                    }
                    else
                    {
                        Debug.Log("Farmer không còn hạt giống để trồng! Đang bỏ qua...");
                    }
                }
                
                ResetAllTargets();
            }
        }

        // Thu thập tài nguyên
        if (role == AgentRole.Gatherer && currentTargetResource != null)
        {
            float distToRes = Vector2.Distance(transform.position, currentTargetResource.transform.position);
            if (distToRes < 1.2f) 
            {
                hitTimer += Time.deltaTime;
                if (hitTimer >= hitCooldown) 
                {
                    hitTimer = 0f; 
                    myAgent.TriggerActionAnim(); 
                    
                    currentTargetResource.GetDamage(1f); 
                    if (currentTargetResource.HP <= 0) ResetAllTargets(); 
                }
            }
        }
    }

    public void HandleTriggerTasks(Collider2D collision)
    {
        Transform currentGoal = GetActiveTarget();

        // Cho động vật ăn
        if (role == AgentRole.AnimalCaretaker && collision.CompareTag("Target") && currentTargetEntity != null && currentGoal == currentTargetEntity.transform) 
        {
            Entity entity = collision.GetComponent<Entity>();
            if (entity != null && entity == currentTargetEntity && entity.isHungry)
            {
                int requiredFoodID = entity.foodItemID;
                if (InventoryController.Instance.HasItemGlobal(requiredFoodID))
                {
                    if (entity.TryFeed(requiredFoodID))
                    {
                        myAgent.TriggerActionAnim();
                        InventoryController.Instance.RemoveItemGlobal(requiredFoodID, 1);
                        ResetAllTargets(); 
                    }
                }
            }
        }

        // Nhặt Item
        else if (collision.CompareTag("Item") && currentTargetItem != null && currentGoal == currentTargetItem.transform)
        {
            Item groundItem = collision.GetComponent<Item>();
            if (groundItem != null && groundItem == currentTargetItem)
            {
                bool added = InventoryController.Instance.AddItem(groundItem.gameObject);
                if (added)
                {
                    Destroy(groundItem.gameObject); 
                    ResetAllTargets(); 
                }
                else ResetAllTargets(); 
            }
        }
    }

    public void FireAgent()
    {
        if (isLeaving) return;

        isLeaving = true;
        ResetAllTargets();
        Debug.Log($"Nhân viên {gameObject.name} đã bị sa thải!");
    }
}