using UnityEngine;

public class PauseController : MonoBehaviour
{
    // Biến static để các script khác (như NPC.cs) có thể truy cập mà không cần kéo link
    public static bool IsGamePaused { get; private set; }

    public static void SetPause(bool pause)
    {
        IsGamePaused = pause;

        if (pause)
        {
            // Dừng thời gian trong game (NPC ngừng đi, Player ngừng di chuyển)
            Time.timeScale = 0f;
        }
        else
        {
            // Trả lại thời gian bình thường
            Time.timeScale = 1f;
        }
    }
}