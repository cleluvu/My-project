using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class TimeUIManager : MonoBehaviour
{
    public Text dayText;
    public Text timeText;
    public Button x1Button;
    Image x1Img;
    RectTransform x1Rt;
    public Button x2Button;
    Image x2Img;
    RectTransform x2Rt;
    public Button x3Button;
    Image x3Img;
    RectTransform x3Rt;

    public GameObject timeManager;
    private DayAndNight dayAndNight;

    void Awake()
    {
        dayAndNight = timeManager.GetComponent<DayAndNight>();

        x1Img = x1Button.GetComponent<Image>();
        x2Img = x2Button.GetComponent<Image>();
        x3Img = x3Button.GetComponent<Image>();

        x1Rt = x1Button.GetComponent<RectTransform>();
        x2Rt = x2Button.GetComponent<RectTransform>();
        x3Rt = x3Button.GetComponent<RectTransform>();
    }

    void Update()
    {
        // Tính toán giờ hiển thị
        float timePercent = dayAndNight.currentTime / dayAndNight.dayDuration;
        float hourInGame = timePercent * 24f;

        int hour = Mathf.FloorToInt(hourInGame);
        int minute = Mathf.FloorToInt((hourInGame - hour) * 60f);

        dayText.text = "Day " + dayAndNight.day;
        timeText.text = string.Format("{0:00}:{1:00}", hour, minute);
    }

    public void X1SpeedTimeGame()
    {
        Debug.Log("Bấm nút 1");
        dayAndNight.timeMultiplier = 1f;

        x1Img.color = Color.yellow;
        StopAllCoroutines(); 
        StartCoroutine(ScaleButton(x1Rt, Vector3.one * 1.05f));

        x2Img.color = Color.white;
        x2Rt.localScale = Vector3.one;

        x3Img.color = Color.white;
        x3Rt.localScale = Vector3.one;
    }

    public void X2SpeedTimeGame()
    {
        dayAndNight.timeMultiplier = 2f;

        x2Img.color = Color.yellow;
        StopAllCoroutines();
        StartCoroutine(ScaleButton(x2Rt, Vector3.one * 1.05f));

        x1Img.color = Color.white;
        x1Rt.localScale = Vector3.one;

        x3Img.color = Color.white;
        x3Rt.localScale = Vector3.one;
    }

    public void X3SpeedTimeGame()
    {
        dayAndNight.timeMultiplier = 3f;

        x3Img.color = Color.yellow;
        StopAllCoroutines();
        StartCoroutine(ScaleButton(x3Rt, Vector3.one * 1.05f));

        x2Img.color = Color.white;
        x2Rt.localScale = Vector3.one;

        x1Img.color = Color.white;
        x1Rt.localScale = Vector3.one;
    }

    IEnumerator ScaleButton(RectTransform rt, Vector3 target)
    {
        float time = 0;
        Vector3 start = rt.localScale;
        while (time < 0.2f) 
        {
            rt.localScale = Vector3.Lerp(start, target, time / 0.2f);
            time += Time.unscaledDeltaTime; 
            yield return null;
        }

        rt.localScale = target;
    }
     public void OnSaveButtonClicked()
    {
        SaveController.Instance.SaveGame();
    }

    public void OnLoadButtonClicked()
    {
        SaveController.Instance.LoadGame();
    }
}