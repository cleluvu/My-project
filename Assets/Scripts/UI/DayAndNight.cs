using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayAndNight : MonoBehaviour
{
    [Header("Light Settings")]
    public Light2D globalLight;
    public Gradient lightColor;

    [Header("Time Settings")]
    [Tooltip("Số giây ngoài đời để hết 1 ngày. 900s = 15 phút")]
    public float dayDuration = 900f;

    public float currentTime = 0;
    public int day = 0;

    [Range(0, 10)]
    public float timeMultiplier = 1f;

    public float allTimeFromBegin = 0;

    void Start()
    {
        if (currentTime <= 0.1f)
        {
            currentTime = (7f / 24f) * dayDuration;
        }

        UpdateLight();
    }

    void Update()
    {
        float delta = Time.deltaTime * timeMultiplier;

        allTimeFromBegin += delta;
        currentTime += delta;

        // DỌN DẸP: Chỉ để lại MỘT khối kiểm tra qua ngày
        if (currentTime >= dayDuration)
        {
            currentTime -= dayDuration;
            day += 1;
            Debug.Log("Qua ngày mới: " + day);

            // Tự động save khi qua ngày mới
            if (SaveController.Instance != null) 
            {
                SaveController.Instance.SaveGame();
            }
        }

        UpdateLight();
    }


    public void UpdateLight()
    {
        if (globalLight != null && lightColor != null)
        {
            float timePercent = currentTime / dayDuration;
            globalLight.color = lightColor.Evaluate(timePercent);
        }
    }
}