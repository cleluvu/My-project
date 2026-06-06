using UnityEngine;

public class MyAgent : MonoBehaviour 
{
    [Header("Hoạt ảnh & Di chuyển")]
    public Animator anim;
    public float moveSpeed = 5f;
    public Rigidbody2D agentRb;
    
    [Header("Pathfinding")]
    public Vector3 nextWaypointPosition; 
    [HideInInspector] public bool isFinalWaypoint = false;

    private Vector2 lastMoveDirection = new Vector2(0, -1);
    private AgentTaskManager taskManager; // Tham chiếu sang bộ não

    protected void Awake()
    {
        taskManager = GetComponent<AgentTaskManager>();
    }

    public Transform GetActiveTarget()
    {
        if (taskManager != null) return taskManager.GetActiveTarget();
        return transform;
    }

    public void TriggerActionAnim()
    {
        if (anim != null) anim.SetTrigger("doAction");
    }

    private void Update()
    {
        if (taskManager != null) taskManager.HandleTasksUpdate();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (taskManager != null) taskManager.HandleTriggerTasks(collision);
    }

    private void FixedUpdate()
    {
        if(agentRb != null)
        {
            float distance = Vector2.Distance(transform.position, nextWaypointPosition);
            float step = moveSpeed * Time.fixedDeltaTime;

            if (nextWaypointPosition == transform.position || (isFinalWaypoint && distance <= step))
            {
                agentRb.linearVelocity = Vector2.zero;
            }
            else
            {
                Vector2 perfectDirection = (nextWaypointPosition - transform.position).normalized;
                agentRb.linearVelocity = perfectDirection * moveSpeed;
            }
        }   

        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (anim == null) return;

        Vector2 currentVelocity = agentRb.linearVelocity;
        bool isMoving = currentVelocity.sqrMagnitude > 0.01f;
        anim.SetBool("isMoving", isMoving);

        if (isMoving)
        {
            anim.SetFloat("moveX", currentVelocity.normalized.x);
            anim.SetFloat("moveY", currentVelocity.normalized.y);
            lastMoveDirection = currentVelocity.normalized;
        }

        anim.SetFloat("lastMoveX", lastMoveDirection.x);
        anim.SetFloat("lastMoveY", lastMoveDirection.y);
    }
}