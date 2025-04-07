using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class FlashlightToggle : MonoBehaviour
{
    [Header("Flashlight Objects")]
    public GameObject flashlightObject;
    public Button flashlightButton;

    [Header("Audio Sources")]
    public AudioSource normalClickAudio;
    public AudioSource brokenClickAudio;

    [Header("Settings")]
    [Range(0f, 1f)] public float brokenLightAlpha = 0.05f; // Dim (5% brightness)
    private float fadeDuration = 0.1f;
    private float clickFlashDuration = 0.01f;

    private Renderer flashlightRenderer;
    private bool isFading = false;
    private Coroutine flashCoroutine;
    private bool isHolding = false;
    private bool flashlightDisabled = false;
    private bool isBroken = false;

    private void Start()
    {
        flashlightRenderer = flashlightObject.GetComponent<Renderer>();
        SetObjectAlpha(0f);

        var eventTrigger = flashlightButton.gameObject.AddComponent<EventTrigger>();

        // Pointer Down (hold to keep on)
        var pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((data) => { OnButtonPressed(); });
        eventTrigger.triggers.Add(pointerDown);

        // Pointer Up (release to turn off)
        var pointerUp = new EventTrigger.Entry();
        pointerUp.eventID = EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((data) => { OnButtonReleased(); });
        eventTrigger.triggers.Add(pointerUp);

        // Click (quick flash)
        flashlightButton.onClick.AddListener(OnButtonClicked);

        InitializeAudioSources();
        CheckTimeForBrokenState();
    }

    private void InitializeAudioSources()
    {
        if (normalClickAudio != null)
        {
            normalClickAudio.playOnAwake = false;
            normalClickAudio.loop = false;
        }

        if (brokenClickAudio != null)
        {
            brokenClickAudio.playOnAwake = false;
            brokenClickAudio.loop = false;
        }
    }

    private void CheckTimeForBrokenState()
    {
        TimerScript timer = FindObjectOfType<TimerScript>();
        if (timer != null && (timer.GetCurrentHour() > 8 || (timer.GetCurrentHour() == 8 && timer.GetCurrentMinute() >= 30)))
        {
            SetBrokenState();
        }
    }

    public void SetBrokenState()
    {
        isBroken = true;
        flashlightButton.interactable = true;
    }

    public void DisableFlashlight()
    {
        flashlightDisabled = true;
        flashlightButton.interactable = false;
        
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
        
        if (isFading)
        {
            StopAllCoroutines();
            isFading = false;
        }
        
        SetObjectAlpha(0f);
        StopAllSounds();
    }

    private void StopAllSounds()
    {
        if (normalClickAudio != null) normalClickAudio.Stop();
        if (brokenClickAudio != null) brokenClickAudio.Stop();
    }

    private void OnButtonPressed()
    {
        if (flashlightDisabled) return;

        isHolding = true;
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        if (isBroken)
        {
            // Broken flashlight behavior (very dim)
            PlaySound(brokenClickAudio);
            if (!isFading)
                StartCoroutine(FadeObject(brokenLightAlpha));
        }
        else
        {
            // Normal flashlight behavior
            PlaySound(normalClickAudio);
            if (!isFading)
                StartCoroutine(FadeObject(1f));

            if (MonsterSpawnManager.Instance != null && MonsterSpawnManager.Instance.IsMonsterActive())
            {
                MonsterSpawnManager.Instance.ScareAwayMonster();
            }
        }
    }

    private void OnButtonReleased()
    {
        if (flashlightDisabled) return;

        isHolding = false;
        if (!isFading && flashCoroutine == null)
        {
            StartCoroutine(FadeObject(0f));
        }
    }

    private void OnButtonClicked()
    {
        if (flashlightDisabled) return;

        if (!isHolding)
        {
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);

            flashCoroutine = StartCoroutine(isBroken ? BrokenQuickFlash() : QuickFlash());
        }
    }

    private IEnumerator QuickFlash()
    {
        PlaySound(normalClickAudio);
        yield return StartCoroutine(FadeObject(1f));
        yield return new WaitForSeconds(clickFlashDuration);
        if (!isHolding)
            yield return StartCoroutine(FadeObject(0f));
        flashCoroutine = null;
    }

    private IEnumerator BrokenQuickFlash()
    {
        PlaySound(brokenClickAudio);
        yield return StartCoroutine(FadeObject(brokenLightAlpha));
        yield return new WaitForSeconds(clickFlashDuration);
        if (!isHolding)
            yield return StartCoroutine(FadeObject(0f));
        flashCoroutine = null;
    }

    private IEnumerator FadeObject(float targetAlpha)
    {
        isFading = true;
        float startAlpha = flashlightRenderer.material.color.a;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            SetObjectAlpha(newAlpha);
            yield return null;
        }

        SetObjectAlpha(targetAlpha);
        isFading = false;
    }

    private void SetObjectAlpha(float alpha)
    {
        Color color = flashlightRenderer.material.color;
        color.a = alpha;
        flashlightRenderer.material.color = color;
    }

    private void PlaySound(AudioSource audioSource)
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}