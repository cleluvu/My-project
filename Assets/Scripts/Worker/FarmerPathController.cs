using UnityEngine;
using Pathfinding; 

public class FarmerPathController : MonoBehaviour
{
    [Header("Cấu hình AI")]
    public MyAgent agent;
    
    [Header("Thông số đường đi")]
    public float nextWaypointDistance = 1.2f;
    
    private Seeker seeker;
    private Path path;
    private int currentWaypoint = 0;

    void Start()
    {
        if (agent != null) 
        {
            seeker = agent.GetComponent<Seeker>();
        }

        if (seeker == null) {
            Debug.LogError("Vẫn không tìm thấy Seeker trên đối tượng Agent!");
        }

        InvokeRepeating("UpdatePath", 0f, 1.0f);
    }

    void UpdatePath()
    {
        if (seeker != null && seeker.IsDone())
        {
            Transform activeGoal = agent.GetActiveTarget(); 
            
            if (activeGoal != null)
            {
                seeker.StartPath(agent.transform.position, activeGoal.position, OnPathComplete);
            }
        }
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            if (path.vectorPath.Count > 1) 
            {
                currentWaypoint = 1; 
            }
            else 
            {
                currentWaypoint = 0;
            }
        }
    }

    void Update()
    {
        if (path == null || currentWaypoint >= path.vectorPath.Count) 
        {
            if (agent != null) 
            {
                agent.nextWaypointPosition = agent.transform.position; 
                agent.isFinalWaypoint = true;
            }
            return;
        }

        // Check điểm cuối cùng
        agent.isFinalWaypoint = (currentWaypoint == path.vectorPath.Count - 1);
        
        // Gửi tọa độ mốc cho Agent
        agent.nextWaypointPosition = path.vectorPath[currentWaypoint];

        float distance = Vector2.Distance(agent.transform.position, path.vectorPath[currentWaypoint]);
        
        if (distance < nextWaypointDistance && !agent.isFinalWaypoint)
        {
            currentWaypoint++;
        }
    }
}