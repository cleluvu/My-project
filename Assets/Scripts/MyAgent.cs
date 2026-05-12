using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class MyAgent : Agent
{
    [Header("Cấu hình di chuyển")]
    public float moveSpeed = 5f;
    public Rigidbody2D agentRb;
    
    [Header("Mục tiêu")]
    public Transform target; // Con gà
    public Vector3 nextWaypointPosition; // Điểm mốc từ FarmerPathController

    private Vector2 targetVelocity;
    
    // ĐÃ THÊM: Khai báo biến lưu khoảng cách cũ
    private float lastDistance; 

    public override void OnEpisodeBegin()
    {
        // 1. Reset vị trí Nông dân
        transform.position = new Vector3(0, 0, 0);
        agentRb.linearVelocity = Vector2.zero;

        // 2. Reset vị trí con Gà ngẫu nhiên
        float radius = Random.Range(2f, 8f); 
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        target.position = transform.position + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);

        // ĐÃ THÊM: Cập nhật khoảng cách ban đầu để tránh lỗi cộng điểm sai ở khung hình đầu tiên
        lastDistance = Vector2.Distance(transform.position, nextWaypointPosition);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector2 dirToWaypoint = (nextWaypointPosition - transform.position);
        
        sensor.AddObservation(dirToWaypoint.normalized);      // 2 số
        sensor.AddObservation(dirToWaypoint.magnitude / 10f); // 1 số
        sensor.AddObservation(agentRb.linearVelocity / moveSpeed); // 2 số
        sensor.AddObservation(transform.up.x);                // 1 số
        sensor.AddObservation(transform.up.y);                // 1 số
        // TỔNG CỘNG: 7 số
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Điều khiển
        float moveX = actions.ContinuousActions[0];
        float moveY = actions.ContinuousActions[1];
        targetVelocity = new Vector2(moveX, moveY) * moveSpeed;

        // Logic thưởng tịnh tiến
        float currentDistance = Vector2.Distance(transform.position, nextWaypointPosition);
        if (currentDistance < lastDistance) {
            AddReward(0.001f); // Chỉ thưởng khi tiến lại gần
        }
        lastDistance = currentDistance; // Lưu lại để so sánh cho khung hình sau

        // Phạt thời gian
        AddReward(-0.001f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Target")) 
        {
            SetReward(20.0f); 
            EndEpisode();    
        }
    }

    private void FixedUpdate()
    {
        if(agentRb != null)
        {
            agentRb.linearVelocity = Vector2.Lerp(agentRb.linearVelocity, targetVelocity, Time.fixedDeltaTime * 5f);
        }   
    }
}