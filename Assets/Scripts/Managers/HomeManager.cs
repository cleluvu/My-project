using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class HomeManager : MonoBehaviour
{
    [Header("New Game Confirmation UI")]
    public GameObject confirmPanel;
    public TMP_InputField confirmInputField;
    public Button confirmButton;

    [Header("State UI")]
    public TextMeshProUGUI btnText;

    [Header("Loading UI")]
    public GameObject loadingPanel;
    public Slider loadingSlider;

    private bool isFullScreenMode;

    void Start()
    {
        if (confirmPanel != null) confirmPanel.SetActive(false);
        if (loadingPanel != null) loadingPanel.SetActive(false);

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
        StartCoroutine(NewGameRoutine());
    }

    private IEnumerator NewGameRoutine()
    {
        if (loadingPanel != null) loadingPanel.SetActive(true);
        if (loadingSlider != null) loadingSlider.value = 0f;

        // Chạy thanh load ảo
        float timer = 0f;
        float fakeLoadTime = 1.0f;
        while (timer < fakeLoadTime)
        {
            timer += Time.deltaTime;
            if (loadingSlider != null)
            {
                loadingSlider.value = Mathf.Lerp(0f, 0.9f, timer / fakeLoadTime);
            }
            yield return null;
        }

        // Load thật
        if (PersistentUI.Instance != null) PersistentUI.Instance.gameObject.SetActive(true);
        if (SaveController.Instance != null) SaveController.Instance.StartNewGame();

        if (loadingSlider != null) loadingSlider.value = 1f;

        yield return new WaitForSeconds(0.2f); 

        if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    public void ContinueGame()
    {
        StartCoroutine(ContinueGameRoutine());
    }

    private IEnumerator ContinueGameRoutine()
    {
        if (loadingPanel != null) loadingPanel.SetActive(true);
        if (loadingSlider != null) loadingSlider.value = 0f;

        // Chạy thanh loading ảo
        float timer = 0f;
        float fakeLoadTime = 1.5f;
        while (timer < fakeLoadTime)
        {
            timer += Time.deltaTime;
            if (loadingSlider != null)
            {
                loadingSlider.value = Mathf.Lerp(0f, 0.9f, timer / fakeLoadTime);
            }
            yield return null; 
        }

        // Chạy thanh loading thật
        if (PersistentUI.Instance != null) PersistentUI.Instance.gameObject.SetActive(true);
        if (SaveController.Instance != null) SaveController.Instance.StartContinueGame();

        if (loadingSlider != null) loadingSlider.value = 1f;

        yield return new WaitForSeconds(0.2f);
        
        if (loadingPanel != null) loadingPanel.SetActive(false);
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