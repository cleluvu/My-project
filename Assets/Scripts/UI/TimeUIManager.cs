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
        float timePercent = dayAndNight.currentTime / dayAndNight.dayDuration;
        float hourInGame = timePercent * 24f;

        int hour = Mathf.FloorToInt(hourInGame);
        int minute = Mathf.FloorToInt((hourInGame - hour) * 60f);

        dayText.text = "Day " + dayAndNight.day;
        timeText.text = string.Format("{0:00}:{1:00}", hour, minute);
    }

    public void X1SpeedTimeGame()
    {
        dayAndNight.speedTime = Time.deltaTime;

        x1Img.color = Color.yellow;
        StartCoroutine(ScaleButton(x1Rt, Vector3.one * 1.05f));

        x2Img.color = Color.white;
        StartCoroutine(ScaleButton(x2Rt, Vector3.one));

        x3Img.color = Color.white;
        StartCoroutine(ScaleButton(x3Rt, Vector3.one));
    }

    public void X2SpeedTimeGame()
    {
        dayAndNight.speedTime = Time.deltaTime * 2;

        x2Img.color = Color.yellow;
        StartCoroutine(ScaleButton(x2Rt, Vector3.one * 1.05f));

        x1Img.color = Color.white;
        StartCoroutine(ScaleButton(x1Rt, Vector3.one));

        x3Img.color = Color.white;
        StartCoroutine(ScaleButton(x3Rt, Vector3.one));
    }

    public void X3SpeedTimeGame()
    {
        dayAndNight.speedTime = Time.deltaTime * 3;

        x3Img.color = Color.yellow;
        StartCoroutine(ScaleButton(x3Rt, Vector3.one * 1.05f));

        x2Img.color = Color.white;
        StartCoroutine(ScaleButton(x2Rt, Vector3.one));

        x1Img.color = Color.white;
        StartCoroutine(ScaleButton(x1Rt, Vector3.one));
    }

    IEnumerator ScaleButton(RectTransform rt, Vector3 target)
    {
        float time = 0;
        Vector3 start = rt.localScale;
        while(time < 0.5f)
        {
            rt.localScale = Vector3.Lerp(start, target, time / 0.5f);
            time += Time.deltaTime;
            yield return null;
        }

        rt.localScale = target;
    }
}
