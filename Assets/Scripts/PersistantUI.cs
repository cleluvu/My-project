using UnityEngine;

public class PersistentUI : MonoBehaviour
{
    public static PersistentUI Instance;

    void Awake()
    {
        // Đảm bảo chỉ có DUY NHẤT 1 bộ UI tồn tại trong game
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Ban thẻ bất tử cho UI
        }
        else
        {
            // Nếu load lại scene mà đã có UI rồi thì hủy bản sao mới đi
            Destroy(gameObject); 
        }
    }
}