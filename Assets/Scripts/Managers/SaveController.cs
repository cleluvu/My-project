using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveController : MonoBehaviour
{
    public static SaveController Instance;

    private string savePath;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        savePath = Application.persistentDataPath + "/savegame.json";
    }
    void Start()
    {
        LoadGame();
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();

        // Lưu vị trí Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) data.playerPosition = player.transform.position;

        // Lưu thời gian
        DayAndNight dayNight = Object.FindFirstObjectByType<DayAndNight>();
        if (dayNight != null)
        {
            data.savedCurrentTime = dayNight.currentTime;
            data.savedDay = dayNight.day;
        }

        // Chuyển thành JSON và lưu file
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Đã Save vị trí và thời gian!");
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath)) return;

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // Load vị trí Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) player.transform.position = data.playerPosition;

        // Load thời gian (MỚI)
        DayAndNight dayNight = Object.FindFirstObjectByType<DayAndNight>();
        if (dayNight != null)
        {
            dayNight.currentTime = data.savedCurrentTime;
            dayNight.day = data.savedDay;
        }

        Debug.Log("Đã Load thành công!");
    }
}