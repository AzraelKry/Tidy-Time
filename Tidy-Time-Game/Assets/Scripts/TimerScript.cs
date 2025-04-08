using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class TimerScript : MonoBehaviour
{
    public TextMeshProUGUI timeText;
    private int currentHour = 6;
    private int currentMinute = 0;
    private int currentSecond = 0;
    private bool isRunning = true;
    private bool isPaused = false;

    public static TimerScript Instance { get; private set; } // Singleton instance

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            ReassignTimeText();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0)
        {
            ResetTimerToDefault();
        }
        else
        {
            ReassignTimeText();
            if (!isRunning)
            {
                RestartTimer();
            }
        }
    }

    void Start()
    {
        if (timeText == null) ReassignTimeText();
        if (timeText != null) UpdateTimeText();
        StartCoroutine(UpdateTimer());
    }

    IEnumerator UpdateTimer()
    {
        while (isRunning)
        {
            if (!isPaused)
            {
                yield return new WaitForSeconds(0.015f);

                currentSecond++;
                if (currentSecond >= 60)
                {
                    currentSecond = 0;
                    currentMinute++;
                }
                if (currentMinute >= 60)
                {
                    currentMinute = 0;
                    currentHour++;
                }

                // Notify MonsterSpawnManager when it's 8:30 PM
                if (currentHour == 8 && currentMinute == 30 && currentSecond == 0)
                {
                    MonsterSpawnManager.Instance?.TriggerFinalSequence();
                }

                // End game at 9:00 PM
                if (currentHour >= 9 && currentMinute >= 0)
                {
                    isRunning = false;
                    MonsterSpawnManager.Instance?.TriggerGameOver();
                    yield break;
                }

                UpdateTimeText();
            }
            else
            {
                yield return null;
            }
        }
    }

    public void ResetTimerToDefault()
    {
        StopAllCoroutines();
        currentHour = 6;
        currentMinute = 0;
        currentSecond = 0;
        isRunning = false;
        isPaused = false;
        ReassignTimeText();
        UpdateTimeText();
    }

    public int GetCurrentHour() => currentHour;
    public int GetCurrentMinute() => currentMinute;
    public int GetCurrentSecond() => currentSecond;

    public void SetTime(int hour, int minute, int second, TextMeshProUGUI newTimeText = null)
    {
        StopAllCoroutines();
        currentHour = hour;
        currentMinute = minute;
        currentSecond = second;
        isRunning = true;

        if (newTimeText != null)
        {
            timeText = newTimeText;
        }
        else
        {
            ReassignTimeText();
        }

        if (timeText != null) UpdateTimeText();
        StartCoroutine(UpdateTimer());
    }

    private void UpdateTimeText()
    {
        if (timeText == null) ReassignTimeText();
        if (timeText != null)
        {
            timeText.text = $"{currentHour}:{currentMinute:D2}:{currentSecond:D2} PM";
        }
    }

    public void PauseTimer() => isPaused = true;
    public void ResumeTimer() => isPaused = false;

    private void ReassignTimeText()
    {
        GameObject textObject = GameObject.FindWithTag("Timer");
        if (textObject != null)
        {
            timeText = textObject.GetComponent<TextMeshProUGUI>();
        }
    }

    public void RestartTimer()
    {
        StopAllCoroutines();
        currentHour = 6;
        currentMinute = 0;
        currentSecond = 0;
        isRunning = true;
        isPaused = false;
        ReassignTimeText();
        UpdateTimeText();
        StartCoroutine(UpdateTimer());
    }
}