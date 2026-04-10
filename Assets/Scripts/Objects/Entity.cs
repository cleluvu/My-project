using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Entity : MonoBehaviour
{
    [Header("Identity")]
    public string ID;
    [Header("Move zone")]
    public BoxCollider2D moveZone;
    public float moveSpeed = 1.5f;
    public float minWaitTime = 1f;
    public float maxWaitTime = 3f;

    [Header("Farming and drop")]
    public int foodItemID;
    public int dropItemID;
    public int daysToDrop = 3;

    [Header("UI box chat")]
    public GameObject hungerBoxUI;
    public Image foodIconUI;
    public Sprite footSprite;

    // Thông tin cần lưu
    private int daysSinceLastDrop = 0;
    private int daysFedSinceLastDrop = 0;
    public bool isHungry {get; private set;} = false;

    private Vector2 targetPosition;
    private bool isWaiting;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    private DayAndNight dayAndNight;
    private int currentDayTracker = -1;

    void Awake()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if(string.IsNullOrEmpty(ID)) ID = System.Guid.NewGuid().ToString();
    }

    void Start()
    {
        GetNewRandomPosition();

        dayAndNight = Object.FindAnyObjectByType<DayAndNight>();
        if(dayAndNight != null) currentDayTracker = dayAndNight.day;

        UpdateHungerUI();
    }

    void Update()
    {
        CheckNewDay();
        HandleMovement();
    }

    public void CheckNewDay()
    {
        if(dayAndNight != null && dayAndNight.day != currentDayTracker)
        {
            currentDayTracker = dayAndNight.day;
            OnNewDayStarted();
        }
    }

    public void OnNewDayStarted()
    {
        isHungry = true;
        UpdateHungerUI();

        daysSinceLastDrop ++;
        if(daysSinceLastDrop >= daysToDrop)
        {
            float p = (float) daysFedSinceLastDrop / daysToDrop;

            if(Random.value <= p)
            {
                SpawnDropItem();
            }

            daysSinceLastDrop = 0;
            daysFedSinceLastDrop = 0;
        }
    }

    public bool TryFeed(int itemID)
    {
        if(!isHungry) return false;
        if(itemID == foodItemID)
        {
            isHungry = false;
            daysFedSinceLastDrop ++;
            UpdateHungerUI();
            Debug.Log("Cho ăn rồi");
            return true;
        }
        Debug.Log("Cho ăn thất bại");
        return false;
    }

    public void SpawnDropItem()
    {
        ItemDictionary dictionary = Object.FindAnyObjectByType<ItemDictionary>();
        if(dictionary != null)
        {
            GameObject itemPrefeb = dictionary.GetItemPrefab(dropItemID);
            if(itemPrefeb != null)
            {
                Instantiate(itemPrefeb, transform.position, Quaternion.identity);
                Debug.Log("Rơi đồ rồi này");
            }
        }
    }

    public void UpdateHungerUI()
    {
        if(hungerBoxUI != null)
        {
            hungerBoxUI.SetActive(isHungry);
            if(isHungry && foodIconUI != null && footSprite != null)
            {
                foodIconUI.sprite = footSprite;
            }
        }
    }

    public EntitySaveData GetSaveData()
    {
        return new EntitySaveData
        {
            ID = this.ID,
            position = transform.position,
            daysSinceLastDrop = this.daysSinceLastDrop,
            daysFedSinceLastDrop = this.daysFedSinceLastDrop,
            isHungry = this.isHungry
        };
    }

    public void RestoreData(EntitySaveData data)
    {
        transform.position = data.position;
        this.daysSinceLastDrop = data.daysSinceLastDrop;
        this.daysFedSinceLastDrop = data.daysFedSinceLastDrop;
        this.isHungry = data.isHungry;

        UpdateHungerUI();
    }

    public void HandleMovement()
    {
        if(isWaiting) return;

        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        UpdateAnimation(direction);

        if(Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            StartCoroutine(WaitRoutine());
        }
    }

    void GetNewRandomPosition()
    {
        if(moveZone == null) return;
        Bounds bounds = moveZone.bounds;

        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomY = Random.Range(bounds.min.y, bounds.max.y);

        targetPosition = new Vector2(randomX, randomY);
    }
    
    IEnumerator WaitRoutine()
    {
        isWaiting = true;
        UpdateAnimation(Vector2.zero);

        float waitTime = Random.Range(minWaitTime, maxWaitTime);
        yield return new WaitForSeconds(waitTime);

        GetNewRandomPosition();
        isWaiting = false;
    }

    void UpdateAnimation(Vector2 moveDirection)
    {
        if(anim != null)
        {
            if(moveDirection != Vector2.zero)
            {
                anim.SetBool("isMoving", true);
                if(moveDirection.x > 0)
                {
                    spriteRenderer.flipX = false;
                }
                else if(moveDirection.x < 0)
                {
                    spriteRenderer.flipX = true;
                }
            }
            else
            {
                anim.SetBool("isMoving", false);
            }
        }
    }
}
