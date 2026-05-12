using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HomeManager : MonoBehaviour
{
    [Header("New Game Confirmation UI")]
    public GameObject confirmPanel;
    public TMP_InputField confirmInputField;
    public Button confirmButton;

    [Header("State UI")]
    public TextMeshProUGUI btnText;

    // Biến nội bộ giúp giải quyết lỗi Unity Editor không chịu đổi chữ
    private bool isFullScreenMode;

    void Start()
    {
        // 1. Giấu bảng thông báo đi khi vừa vào màn hình Home
        if (confirmPanel != null) confirmPanel.SetActive(false);

        // 2. Tắt Game UI
        if (PersistentUI.Instance != null)
        {
            PersistentUI.Instance.gameObject.SetActive(false);
        }

        // 3. TẢI CÀI ĐẶT TỪ FILE: Đọc dữ liệu cài đặt màn hình
        // Cú pháp: PlayerPrefs.GetInt("Tên_Key", Giá_Trị_Mặc_Định_Nếu_Chưa_Lưu_Bao_Giờ)
        // Quy ước: 1 là Toàn màn hình, 0 là Cửa sổ
        int savedScreenState = PlayerPrefs.GetInt("IsFullScreen", 1);
        isFullScreenMode = (savedScreenState == 1);

        // 4. Ép hệ thống hiển thị đúng như những gì đã lưu
        if (isFullScreenMode)
        {
            Screen.fullScreen = true;
        }
        else
        {
            Screen.SetResolution(1280, 720, false);
        }

        // 5. Cập nhật chữ trên nút
        UpdateBtnText();
    }

    public void ShowConfirmNewGame()
    {
        confirmPanel.SetActive(true);
        confirmInputField.text = "";
        confirmButton.interactable = false;
    }

    public void HideConfirmNewGame()
    {
        confirmPanel.SetActive(false);
    }

    public void ValidateInput(string input)
    {
        if (input == "YES") 
        {
            confirmButton.interactable = true;
        }
        else
        {
            confirmButton.interactable = false;
        }
    }

    public void ExecuteNewGame()
    {
        confirmPanel.SetActive(false);

        if (PersistentUI.Instance != null) PersistentUI.Instance.gameObject.SetActive(true);
        if (SaveController.Instance != null) SaveController.Instance.StartNewGame();
    }

    public void ContinueGame()
    {
        if (PersistentUI.Instance != null) PersistentUI.Instance.gameObject.SetActive(true);
        if (SaveController.Instance != null) SaveController.Instance.StartContinueGame();
    }

    public void QuitGame()
    {
        Debug.Log("Đang thoát game...");
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void ToggleFullScreen()
    {
        #if UNITY_EDITOR
        Debug.Log("Đã bấm đổi chế độ màn hình!");
        #endif

        // Đảo ngược trạng thái
        isFullScreenMode = !isFullScreenMode;

        // Áp dụng thay đổi cho màn hình
        if (isFullScreenMode)
        {
            Screen.fullScreen = true;
        }
        else
        {
            Screen.SetResolution(1280, 720, false);
        }

        // GHI CÀI ĐẶT VÀO FILE: Lưu lại để lần sau mở game còn nhớ
        PlayerPrefs.SetInt("IsFullScreen", isFullScreenMode ? 1 : 0);
        PlayerPrefs.Save(); // Ép hệ thống ghi file ngay lập tức
        
        // Cập nhật chữ ngay lập tức, bỏ qua Invoke()
        UpdateBtnText(); 
    }

    public void UpdateBtnText()
    {
        if (btnText != null)
        {
            // Dùng thẳng biến nội bộ để đảm bảo Editor cũng nhảy chữ mượt mà
            btnText.text = isFullScreenMode ? "WINDOW" : "FULL SCREEN";
        }
    }
}