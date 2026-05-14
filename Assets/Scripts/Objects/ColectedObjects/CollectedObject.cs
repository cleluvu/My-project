using System.Collections;
using UnityEngine;
using Pathfinding;

public class CollectedObject : MonoBehaviour
{
    [Header("Save Data")]
    public string ID; 

    [Header("Attribute")]
    public float HP = 2f;
    private float maxHP;

    [Header("Respawn Setting")]
    public int daysToRespawn = 3;
    public int daysDead = 0; 
    private Collider2D col;

    [Header("Visual and Item Drop")]
    public GameObject visual;
    public int dropItemID;

    [Header("Shake Image")]
    public float shakeDuration = 0.15f;
    public float shakeAngle = 8f;
    private Coroutine currentShake;
    private bool isDataRestored = false;

    void Awake()
    {
        maxHP = HP;
        col = GetComponent<Collider2D>();
    }

    void Start()
    {
        DayAndNight timeManager = Object.FindFirstObjectByType<DayAndNight>();
        if (timeManager != null) timeManager.onNewDayStarted.AddListener(OnDayPassed);
        
        Invoke(nameof(InitialVisualUpdate), 0.05f);
    }

    private void InitialVisualUpdate()
    {
        if (!isDataRestored)
        {
            UpdateVisuals();
        }
    }

    private void OnDestroy()
    {
        DayAndNight timeManager = Object.FindFirstObjectByType<DayAndNight>();
        if (timeManager != null) timeManager.onNewDayStarted.RemoveListener(OnDayPassed);
    }

    public void GetDamage(float damage)
    {
        if(HP <= 0) return;

        Debug.Log("Dính dame 1 lần");
        
        HP -= damage;
        if(currentShake != null) StopCoroutine(currentShake);
        currentShake = StartCoroutine(ShakeTreeRoutine());

        if(HP <= 0) Die();
    }

    private void Die()
    {
        daysDead = 0;
        DropItem();
        UpdateVisuals(); 
    }

    public void UpdateVisuals()
    {
        if (visual == null) return;

        if(HP > 0)
        {
            visual.SetActive(true);
            if(col != null) col.enabled = true;
        }
        else
        {
            visual.SetActive(false);
            if(col != null) col.enabled = false;
        }

        if (AstarPath.active != null && col != null)
        {
            AstarPath.active.UpdateGraphs(col.bounds); 
        }
    }

    public void DropItem()
    {
        ItemDictionary itemDict = Object.FindFirstObjectByType<ItemDictionary>();
        if(itemDict != null)
        {
            GameObject itemPrefab = itemDict.GetItemPrefab(dropItemID);
            if(itemPrefab != null)
            {
                Vector3 spawnPos = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
                Instantiate(itemPrefab, spawnPos, Quaternion.identity);
            }
        }
    }

    public void OnDayPassed()
    {
        if (HP <= 0)
        {
            daysDead++;
            if (daysDead >= daysToRespawn) Respawn();
        }
    }

    public void Respawn()
    {
        HP = maxHP;
        daysDead = 0;
        UpdateVisuals();
    }

    public IEnumerator ShakeTreeRoutine()
    {
        Quaternion originalRotation = visual.transform.rotation;
        float elapsedTime = 0;
        while(elapsedTime < shakeDuration)
        {
            float randomZ = Random.Range(-shakeAngle, shakeAngle);
            visual.transform.rotation = Quaternion.Euler(0, 0, randomZ);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        visual.transform.rotation = originalRotation;
    }

    public ResourceSaveData GetSaveData()
    {
        return new ResourceSaveData { ID = this.ID, hp = this.HP, dayDead = this.daysDead };
    }

    public void RestoreData(ResourceSaveData data)
    {
        isDataRestored = true;
        this.HP = data.hp;
        this.daysDead = data.dayDead;
        UpdateVisuals();
    }
}