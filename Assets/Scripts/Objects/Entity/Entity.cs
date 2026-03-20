using System.Collections;
using UnityEngine;

public class Entity : MonoBehaviour
{
    // Move zone
    public BoxCollider2D moveZone;

    // Attribute
    public float moveSpeed = 1.5f;
    public float minWaitTime = 1f;
    public float maxWaitTime = 3f;

    private Vector2 targetPosition;
    private bool isWaiting;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        GetNewRandomPosition();
    }

    void Update()
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
