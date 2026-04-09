using UnityEngine;

public class PauseController : MonoBehaviour
{
    public static bool IsGamePause {get; private set;} = false;

    public static void SetPause(bool pause)
    {
        IsGamePause = pause;
    }
}
