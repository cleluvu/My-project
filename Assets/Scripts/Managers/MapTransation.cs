using UnityEngine;
using Unity.Cinemachine;

public class MapTransition : MonoBehaviour
{
    public enum TransitionDirection { Up, Down, Left, Right }

    [Header("Transition Settings")]
    [Tooltip("PolygonCollider2D vùng giới hạn camera của bản đồ mới mà bạn muốn chuyển tới.")]
    [SerializeField] private Collider2D targetMapBounds; 

    [Tooltip("Hướng dịch chuyển của nhân vật khi bước qua ranh giới.")]
    [SerializeField] private TransitionDirection direction;

    [Tooltip("Khoảng cách đẩy nhân vật lên phía trước để tránh lặp trigger (Mặc định: 1.5 - 2f)")]
    [SerializeField] private float additivePosition = 1.5f;

    private CinemachineConfiner2D confiner;

    private void Awake()
    {
        // Tự động tìm kiếm bộ lọc confiner của Cinemachine trong Scene
        confiner = FindFirstObjectByType<CinemachineConfiner2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem Object chạm vào có phải là Player không
        if (collision.CompareTag("Player"))
        {
            if (confiner != null && targetMapBounds != null)
            {
                // 1. Cập nhật vùng giới hạn Camera mới cho Cinemachine
                confiner.BoundingShape2D = targetMapBounds;
                
                confiner.InvalidateCache(); 
                
                // Tìm kiếm camera chính của Cinemachine để ép nó dịch chuyển tức thời không chờ Damping
                var cameraPipeline = confiner.GetComponent<Unity.Cinemachine.CinemachineCamera>();
                if (cameraPipeline != null)
                {
                    cameraPipeline.ForceCameraPosition(collision.transform.position, Quaternion.identity);
                }
            }

            // 2. Đẩy tọa độ nhân vật tiến lên phía trước để sang hẳn map mới
            UpdatePlayerPosition(collision.gameObject);
        }
    }
    private void UpdatePlayerPosition(GameObject player)
    {
        Vector3 newPosition = player.transform.position;

        // Dựa vào hướng đã cấu hình để cộng trừ tọa độ tương ứng
        switch (direction)
        {
            case TransitionDirection.Up:
                newPosition.y += additivePosition;
                break;
            case TransitionDirection.Down:
                newPosition.y -= additivePosition;
                break;
            case TransitionDirection.Left:
                newPosition.x -= additivePosition;
                break;
            case TransitionDirection.Right:
                newPosition.x += additivePosition;
                break;
        }

        // Cập nhật vị trí mới cho nhân vật
        player.transform.position = newPosition;
    }
}