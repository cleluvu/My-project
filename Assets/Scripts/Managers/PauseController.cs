using UnityEngine;

public class PauseController : MonoBehaviour
{
    // Biến static để các script khác (như NPC.cs) có thể truy cập mà không cần kéo link
    public static bool IsGamePause { get; private set; }

    private void Awake()
    {
        // Ensure a clean state every time gameplay starts in Editor/runtime.
        ForceResume();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetPauseOnLoad()
    {
        ForceResume();
    }

    public static void SetPause(bool pause)
    {
        IsGamePause = pause;

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

    private static void ForceResume()
    {
        IsGamePause = false;
        Time.timeScale = 1f;
    }
}