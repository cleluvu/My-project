using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveButton : MonoBehaviour
{
    [Header("UI Cleanup")]
    public GameObject settingPageToClose; 

    public void ClickToSaveGame()
    {
        if (SaveController.Instance != null) SaveController.Instance.SaveGame();
        else Debug.LogError("Không tìm thấy SaveController!");
    }

    public void GoToHome()
    {
        if (SaveController.Instance != null) SaveController.Instance.SaveGame();

        Time.timeScale = 1f;

        if (settingPageToClose != null) 
        {
            settingPageToClose.SetActive(false);
        }

        SceneManager.LoadScene("Home");
    }

    public void ExitGame()
    {
        if (SaveController.Instance != null) SaveController.Instance.SaveGame();

        Debug.Log("Đang thoát game...");
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}