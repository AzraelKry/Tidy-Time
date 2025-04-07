using UnityEngine;
using UnityEngine.UI;

public class TrashManager : MonoBehaviour
{
    public static TrashManager Instance { get; private set; }

    [Header("UI Elements")]
    public GameObject completionPanel;
    
    [Header("Audio")]
    public AudioSource trashAudioSource; // Reference to configured AudioSource

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize audio source if not set
        if (trashAudioSource == null)
        {
            trashAudioSource = GetComponent<AudioSource>();
            if (trashAudioSource == null)
            {
                trashAudioSource = gameObject.AddComponent<AudioSource>();
                Debug.LogWarning("Created default AudioSource on TrashManager");
            }
        }
    }

    private void Start()
    {
        if (ChoreManager.Instance != null)
        {
            if (!ChoreManager.Instance.IsChoreCompleted("garbage"))
            {
                completionPanel.SetActive(false);
            }
            else
            {
                completionPanel.SetActive(true);
                GameObject[] trashObjects = GameObject.FindGameObjectsWithTag("Trash");
                foreach (GameObject trash in trashObjects)
                {
                    trash.SetActive(false);
                }
            }
        }

        // Initialize trash collection state
        GameObject[] allTrash = GameObject.FindGameObjectsWithTag("Trash");
        foreach (GameObject trash in allTrash)
        {
            ItemsIDTracking idTracker = trash.GetComponent<ItemsIDTracking>();
            if (idTracker != null)
            {
                trash.SetActive(ItemCollectionTracker.IsCollected(idTracker.itemID));
            }
        }
    }

    public void PlayTrashThrowSound()
    {
        if (trashAudioSource != null && trashAudioSource.clip != null)
        {
            trashAudioSource.Play();
        }
        else
        {
            Debug.LogWarning("Trash audio not properly configured!");
        }
    }

    public void NotifyTrashCompleted()
    {
        if (completionPanel != null)
        {
            completionPanel.SetActive(true);
        }
    }
}