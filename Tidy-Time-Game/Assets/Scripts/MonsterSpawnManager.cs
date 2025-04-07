using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MonsterSpawnManager : MonoBehaviour
{
    public static MonsterSpawnManager Instance;

    public bool monsterIsActive = false;
    public AudioSource monsterAudioSource;
    private FlashlightToggle flashlightToggle;

    // Spawn timing variables
    private float initialSpawnCooldownMin = 30f;
    private float initialSpawnCooldownMax = 60f;
    private float finalSpawnCooldownMin = 10f;
    private float finalSpawnCooldownMax = 20f;
    private float monsterSpawnDuration = 20f;
    private bool gameOverTriggered = false;
    private bool finalSequenceActive = false;
    private bool monsterDisabled = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (monsterAudioSource != null)
            {
                DontDestroyOnLoad(monsterAudioSource.gameObject);
                monsterAudioSource.volume = 0.3f;
                monsterAudioSource.playOnAwake = false;
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        flashlightToggle = FindObjectOfType<FlashlightToggle>();
        StartCoroutine(MonsterSpawnLoop());
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    IEnumerator MonsterSpawnLoop()
    {
        yield return new WaitForSeconds(3f);

        while (!gameOverTriggered && !finalSequenceActive && !monsterDisabled)
        {
            float progress = GetTimeProgress();
            float currentMinCooldown = Mathf.Lerp(initialSpawnCooldownMin, finalSpawnCooldownMin, progress);
            float currentMaxCooldown = Mathf.Lerp(initialSpawnCooldownMax, finalSpawnCooldownMax, progress);

            float cooldown = Random.Range(currentMinCooldown, currentMaxCooldown);
            Debug.Log($"[Monster] Next spawn in {cooldown:F1} seconds");

            yield return new WaitForSeconds(cooldown);

            if (!finalSequenceActive && !monsterDisabled)
            {
                yield return StartCoroutine(SpawnMonsterRoutine());
            }
        }
    }

    IEnumerator SpawnMonsterRoutine()
    {
        if (monsterDisabled) yield break;

        monsterIsActive = true;
        Debug.Log("[Monster] Monster has spawned!");

        if (monsterAudioSource != null && !monsterDisabled)
        {
            monsterAudioSource.Play();
        }

        float timeWaited = 0f;
        while (timeWaited < monsterSpawnDuration && !finalSequenceActive && !monsterDisabled)
        {
            if (!monsterIsActive)
            {
                Debug.Log("[Monster] Monster scared away!");
                if (monsterAudioSource != null && monsterAudioSource.isPlaying)
                {
                    monsterAudioSource.Stop();
                }
                yield break;
            }

            yield return new WaitForSeconds(1f);
            timeWaited += 1f;
        }

        if (monsterIsActive && !finalSequenceActive && !monsterDisabled)
        {
            TriggerGameOver();
        }
    }

    public void TriggerFinalSequence()
    {
        if (monsterDisabled) return;

        finalSequenceActive = true;
        Debug.Log("[Monster] Starting final sequence!");

        // Disable flashlight
        if (flashlightToggle != null)
        {
            flashlightToggle.DisableFlashlight();
        }

        // Force spawn the monster (sound will start later when time reaches 8:40)
        StartCoroutine(ForceSpawnMonster());
    }

    IEnumerator ForceSpawnMonster()
    {
        if (monsterDisabled) yield break;

        monsterIsActive = true;
        Debug.Log("[Monster] Final monster spawn initiated!");

        // Wait until 8:40 (4 hours 40 minutes in game time)
        while (!monsterDisabled)
        {
            TimerScript timer = FindObjectOfType<TimerScript>();
            if (timer != null)
            {
                float currentTime = timer.GetCurrentHour() + (timer.GetCurrentMinute() / 60f);
                if (currentTime >= 4 + (40f / 60f)) // 4:40 in game time = 8:40 real time
                {
                    break;
                }
            }
            yield return new WaitForSeconds(1f);
        }

        if (monsterDisabled) yield break;

        // Now it's 8:40 - start the monster sound
        if (monsterAudioSource != null)
        {
            monsterAudioSource.Play();
            Debug.Log("[Monster] Final sequence monster sound started at 8:40!");
        }

        // Wait for 20 seconds before jumpscare
        yield return new WaitForSeconds(20f);

        if (!monsterDisabled)
        {
            TriggerGameOver();
        }
    }

    public void ScareAwayMonster()
    {
        if (monsterIsActive && !finalSequenceActive && !monsterDisabled)
        {
            monsterIsActive = false;
            if (monsterAudioSource != null && monsterAudioSource.isPlaying)
            {
                monsterAudioSource.Stop();
            }
        }
    }

    public void TriggerGameOver()
    {
        if (gameOverTriggered || monsterDisabled) return;

        gameOverTriggered = true;
        Debug.Log("[Monster] Triggering game over!");
        SceneManager.LoadScene(12); // Jumpscare scene
    }

    public bool IsMonsterActive() => monsterIsActive && !monsterDisabled;

    private float GetTimeProgress()
    {
        // Safe way to get time progress without direct Instance access
        TimerScript timer = FindObjectOfType<TimerScript>();
        if (timer == null)
        {
            Debug.LogWarning("TimerScript not found in scene");
            return 0f;
        }

        return Mathf.Clamp01((timer.GetCurrentHour() - 4 + timer.GetCurrentMinute() / 60f) / 5f);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Disable monster in Main Menu (0) or Call Mom (10) scenes
        if (scene.buildIndex == 0 || scene.buildIndex == 10)
        {
            monsterDisabled = true;
            CleanUpMonster();
        }
        else
        {
            monsterDisabled = false;
        }
    }

    void CleanUpMonster()
    {
        // Stop all monster-related activities
        monsterIsActive = false;
        finalSequenceActive = false;
        gameOverTriggered = true; // Prevent any further jumpscares

        if (monsterAudioSource != null && monsterAudioSource.isPlaying)
        {
            monsterAudioSource.Stop();
        }

        StopAllCoroutines();

        // If we're in the main menu, destroy everything
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            if (monsterAudioSource != null)
            {
                Destroy(monsterAudioSource.gameObject);
            }
            Destroy(gameObject);
        }
    }
}