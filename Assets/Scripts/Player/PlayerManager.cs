using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private float moveSpeed = 5f;
    private Vector2 targetPosition;
    private bool movesByMouse = false;
    private Vector2 movement;
    private Animator anim;
    
    // Update Using Tools
    public Boolean isActing;
    public Vector2 lastDirection = Vector2.down;
    public GameObject attackZone;
    public int stateTools = 0;

    // Update Player Info
    Player player;

    // check ngủ
    Bed bed;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        player = GetComponent<Player>();
        bed = FindAnyObjectByType<Bed>();
    }

    void Start()
    {
        if(attackZone != null)
        {
            attackZone.SetActive(false);
        }
    }

    void Update()
    {
        if (PauseController.IsGamePause || bed.GetIsSleeping())
        {
            movement = Vector2.zero;
            return;
        }

        // Update Using Tools
        if(isActing) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 keyboardInput = new Vector2(h, v);

        if (keyboardInput != Vector2.zero)
        {
            movement = keyboardInput.normalized;
            movesByMouse = false; 
        }
 
        else if (Input.GetMouseButtonDown(0))
        {
            targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            movesByMouse = true;
        }

        if (movesByMouse && keyboardInput == Vector2.zero)
        {
            Vector2 direction = targetPosition - (Vector2)transform.position;
            
            if (direction.magnitude < 0.1f)
            {
                movement = Vector2.zero;
                movesByMouse = false;
            }
            else
            {
                movement = direction.normalized;
            }
        }
        else if (keyboardInput == Vector2.zero)
        {
            movement = Vector2.zero;
        }

        UpdateAnimation();

        // Update Using Tools
        if (stateTools == 1)
        {
            StartAction("chop");
            PositionAttackZone();
            if(attackZone != null) attackZone.SetActive(true);
        }
        if (stateTools == 2)
        {
            StartAction("till");
            Vector3Int cellPos = Vector3Int.FloorToInt(GetTargetGridPosition());
            FarmingController.Instance.TillSoil(cellPos);
        }
        if (stateTools == 3)
        {
            StartAction("water");
            Vector3Int cellPos = Vector3Int.FloorToInt(GetTargetGridPosition());
            FarmingController.Instance.WaterSoil(cellPos);
        }
        if(stateTools == 4)
        {
            StartAction("plant_wheat");
            Vector3 worldPos = GetTargetGridPosition();
            Vector3Int cellPos = FarmingController.Instance.farmingTilemap.WorldToCell(worldPos);
            FarmingController.Instance.PlantSeed(cellPos, "wheat");
        }
        if(stateTools == 5)
        {
            StartAction("plant_tomato");
            Vector3Int cellPos = Vector3Int.FloorToInt(GetTargetGridPosition());
            FarmingController.Instance.PlantSeed(cellPos, "tomato");
        }
    }

    void UpdateAnimation()
    {
        if (movement != Vector2.zero)
        {
            // Update using tools
            lastDirection = movement;

            anim.SetFloat("moveX", movement.x);
            anim.SetFloat("moveY", movement.y);
            anim.SetBool("isMoving", true);
        }
        else
        {
            anim.SetBool("isMoving", false);
        }
    }

    public void StartAction(String action)
    {
        isActing = true;
        movement = Vector2.zero;
        movesByMouse = false;
        
        anim.SetFloat("moveX", lastDirection.x);
        anim.SetFloat("moveY", lastDirection.y);

        if(action == "plant_wheat" || action == "plant_tomato") anim.SetTrigger("till");
        else anim.SetTrigger(action);
    }

    public Vector3 GetTargetGridPosition()
    {
        Vector2 facingDirection = Vector2.zero;

        if(Mathf.Abs(lastDirection.x) > Mathf.Abs(lastDirection.y))
        {
            facingDirection = new Vector2(Mathf.Sign(lastDirection.x), 0);
        }
        else
        {
            facingDirection = new Vector2(0, Mathf.Sign(lastDirection.y));
        }

        float playerCellX = Mathf.Floor(transform.position.x);
        float playerCellY = Mathf.Floor(transform.position.y);

        float targetCellX = playerCellX + facingDirection.x;
        float targetCellY = playerCellY + facingDirection.y;

        return new Vector3(targetCellX + 0.5f, targetCellY + 0.5f, 0);
    }

    public void PositionAttackZone()
    {
        if(attackZone == null) return;
        Vector2 facingDirection = Vector2.zero;
        if(Mathf.Abs(lastDirection.x) > Mathf.Abs(lastDirection.y))
        {
            facingDirection = new Vector2(Mathf.Sign(lastDirection.x), 0);
        }
        else
        {
            facingDirection = new Vector2(0, Mathf.Sign(lastDirection.y));
        }

        Vector3 playerPos = transform.position;
        PlayerAttackZone playerAttackZone = attackZone.GetComponent<PlayerAttackZone>();
        attackZone.transform.position = playerPos + (Vector3)(facingDirection * playerAttackZone.attackOffset);
    }

    public void EndAction()
    {
        isActing = false;
        stateTools = 0;
        if(attackZone != null)
        {
            attackZone.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        if (!isActing)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }
}