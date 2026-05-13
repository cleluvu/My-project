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

    private bool isFullScreenMode;

    void Start()
    {
        if (confirmPanel != null) confirmPanel.SetActive(false);

        if (PersistentUI.Instance != null)
        {
            PersistentUI.Instance.gameObject.SetActive(false);
        }

        int savedScreenState = PlayerPrefs.GetInt("IsFullScreen", 1);
        isFullScreenMode = (savedScreenState == 1);

        if (isFullScreenMode)
        {
            Screen.fullScreen = true;
        }
        else
        {
            Screen.SetResolution(1280, 720, false);
        }

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

        isFullScreenMode = !isFullScreenMode;

        if (isFullScreenMode)
        {
            Screen.fullScreen = true;
        }
        else
        {
            Screen.SetResolution(1280, 720, false);
        }

        PlayerPrefs.SetInt("IsFullScreen", isFullScreenMode ? 1 : 0);
        PlayerPrefs.Save();
        
        UpdateBtnText(); 
    }

    public void UpdateBtnText()
    {
        if (btnText != null)
        {
            btnText.text = isFullScreenMode ? "WINDOW" : "FULL SCREEN";
        }
    }
}