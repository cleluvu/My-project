using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private float moveSpeed = 5f;
    private Vector2 targetPosition;
    private bool movesByMouse = false;
    private Vector2 movement;
    private Animator anim;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {

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
    }

    void UpdateAnimation()
    {
        if (movement != Vector2.zero)
        {
            anim.SetFloat("moveX", movement.x);
            anim.SetFloat("moveY", movement.y);
            anim.SetBool("isMoving", true);
        }
        else
        {
            anim.SetBool("isMoving", false);
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}