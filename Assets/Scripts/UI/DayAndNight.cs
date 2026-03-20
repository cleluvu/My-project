using UnityEngine;
using UnityEngine.Rendering.Universal;
public class DayAndNight : MonoBehaviour
{
    // Fix light
    public Light2D globalLight;
    public Gradient lightColor;

    // Fix time
    public float dayDuration = 60f;
    public float currentTime = 0f;
    public float day = 0;
    public float speedTime = 1f;
    public float allTimeFromBegin = 0;

    void Awake()
    {
        speedTime = Time.deltaTime;
    }

    void Update()
    {
        allTimeFromBegin += speedTime;
        currentTime += speedTime;

        if(currentTime >= dayDuration)
        {
            currentTime %= dayDuration;
            day += 1;
        }

        float timePercent = currentTime / dayDuration;

        if(globalLight != null)
        {
            globalLight.color = lightColor.Evaluate(timePercent);
        }
    }
}
