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
    public int day = 0;

    public float timeMultiplier = 1f;
    public float allTimeFromBegin = 0;

    void Update()
    {
        float timeAdded = Time.deltaTime * timeMultiplier;

        allTimeFromBegin += timeAdded;
        currentTime += timeAdded;

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
