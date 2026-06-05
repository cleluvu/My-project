using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class SleepUIController : MonoBehaviour
{
    public static SleepUIController Instance;
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1.5f;

    private void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
        if(fadeCanvasGroup != null){
            fadeCanvasGroup.alpha = 0;
            fadeCanvasGroup.blocksRaycasts = false;
            fadeCanvasGroup.interactable = false;
        }
    }

    public IEnumerator FadeOut()
    {
        float time = 0;
        while(time < fadeDuration)
        {
            fadeCanvasGroup.alpha = Mathf.Lerp(0, 1, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }
        fadeCanvasGroup.alpha = 1;
    }

    public IEnumerator FadeIn()
    {
        float time = 0;
        while(time < fadeDuration)
        {
            fadeCanvasGroup.alpha = Mathf.Lerp(1, 0, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }
        fadeCanvasGroup.alpha = 0;
    }
}
