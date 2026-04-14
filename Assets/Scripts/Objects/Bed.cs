using System.Collections;
using NUnit.Framework;
using TMPro;
using UnityEngine;

public class Bed : MonoBehaviour
{
    private static Bed Instance;
    public GameObject interactPrompt;
    private TMP_Text promptText;

    private bool isPlayerNearby = false;
    private bool isSleeping = false;
    private DayAndNight dayAndNight;

    void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if(interactPrompt != null){
            promptText = interactPrompt.GetComponentInChildren<TMP_Text>(true);
            interactPrompt.SetActive(false);
        }
        dayAndNight = Object.FindAnyObjectByType<DayAndNight>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if(interactPrompt != null)
            {
                interactPrompt.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if(interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }
        }
    }

    void Update()
    {
        if(PauseController.IsGamePause || isSleeping) return;

        if (isPlayerNearby)
        {
            bool canSleep = (dayAndNight.currentTime / dayAndNight.dayDuration) >= 0.75f;

            if(promptText != null)
            {
                if (canSleep)
                {
                    promptText.text = "Press E to sleep";
                }
                else
                {
                    promptText.text = "You can just sleep until 18:00";
                }
            }

            if(Input.GetKeyDown(KeyCode.E) && canSleep)
            {
                StartCoroutine(SleepRoutine());
            }
        }
    }

    private IEnumerator SleepRoutine()
    {
        isSleeping = true;
        if(interactPrompt != null) interactPrompt.SetActive(false);

        // Mờ dần
        yield return StartCoroutine(SleepUIController.Instance.FadeOut());

        if(dayAndNight != null)
        {
            // Reset về 6 giờ sáng
            dayAndNight.currentTime = (6f / 24f) * dayAndNight.dayDuration;
            dayAndNight.day += 1;
            dayAndNight.onNewDayStarted?.Invoke();
        }

        if(SaveController.Instance != null)
        {
            SaveController.Instance.SaveGame();
            Debug.Log("Tự save được rồi nài");
        }

        // 1 giây thời gian trôi
        yield return new WaitForSeconds(1f);

        // Sáng lại
        yield return StartCoroutine(SleepUIController.Instance.FadeIn());

        isSleeping = false;

        // Bật lại UI
        if(isPlayerNearby && interactPrompt != null) interactPrompt.SetActive(true);
    }

    public bool GetIsSleeping()
    {
        return isSleeping;
    }
}
