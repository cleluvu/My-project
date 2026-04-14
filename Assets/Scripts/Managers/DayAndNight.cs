using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Events;
public class DayAndNight : MonoBehaviour
{
    [Header("Light Settings")]
    public Light2D globalLight;
    public Gradient lightColor;

    // Fix time
    public float dayDuration = 60f;
    public float currentTime = 0f;
    public int day = 0;

    public float timeMultiplier = 1f;
    public float allTimeFromBegin = 0;

    public UnityEvent onNewDayStarted;

    void Update()
    {
        // 1. Tính toán lượng thời gian trôi qua (đã dùng biến timeAdded)
        float timeAdded = Time.deltaTime * timeMultiplier;

        allTimeFromBegin += timeAdded;
        currentTime += timeAdded;

        // 2. XÓA BỎ HOẶC COMMENT 2 DÒNG NÀY (Vì delta không tồn tại và bị lặp logic)
        // allTimeFromBegin += delta; 
        // currentTime += delta;

        // 3. Kiểm tra qua ngày
        if (currentTime >= dayDuration)
        {
            currentTime -= dayDuration;
            day += 1;
            onNewDayStarted?.Invoke();
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