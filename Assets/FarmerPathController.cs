using UnityEngine;
using Pathfinding; // Bắt buộc phải có để gọi lệnh từ A* Project

public class FarmerPathController : MonoBehaviour
{
    [Header("Cấu hình AI")]
    public MyAgent agent;       // Kéo script MyAgent của bạn vào đây
    public Transform finalGoal; // Con Gà hoặc Cây Lúa (Đích cuối cùng)
    
    [Header("Thông số đường đi")]
    public float nextWaypointDistance = 1.2f; // Khoảng cách để tính là đã qua 1 điểm mốc
    
    private Seeker seeker;
    private Path path;
    private int currentWaypoint = 0;

    void Start()
    {
        // Thay vì tìm trên chính mình, hãy tìm Seeker trên cái đối tượng Agent mà mình đã kéo vào
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
        if (finalGoal != null && seeker.IsDone())
        {
            // Yêu cầu A* tìm đường từ Nông dân đến Gà
            seeker.StartPath(agent.transform.position, finalGoal.position, OnPathComplete);
        }
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0; // Reset lại điểm mốc đầu tiên của đường mới
        }
    }

    void Update()
    {
        // SỬA TẠI ĐÂY: Nếu chưa có đường, hãy bắt Agent đứng yên tại chỗ
        if (path == null || currentWaypoint >= path.vectorPath.Count) 
        {
            if (agent != null) 
                agent.nextWaypointPosition = transform.position; // Đích đến = Vị trí hiện tại
            return;
        }

        // Nếu đã đi hết các điểm mốc trên đường
        if (currentWaypoint >= path.vectorPath.Count) return;

        // GỬI TỌA ĐỘ ĐIỂM MỐC TIẾP THEO CHO AGENT
        agent.nextWaypointPosition = path.vectorPath[currentWaypoint];

        // Kiểm tra xem đã đến gần điểm mốc hiện tại chưa để chuyển sang điểm tiếp theo
        float distance = Vector2.Distance(agent.transform.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }
    }
}