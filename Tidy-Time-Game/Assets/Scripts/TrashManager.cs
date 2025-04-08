using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrashManager : MonoBehaviour
{
    public static TrashManager Instance { get; private set; }

    [Header("Bin Visuals")]
    public GameObject trashbinEmpty;
    public GameObject trashbinFull;

    [Header("UI Elements")]
    public GameObject completionPanel;
    
    [Header("Audio")]
    public AudioSource trashAudioSource; // Reference to configured AudioSource

    [Header("UI")]
    public TextMeshProUGUI trashCountText;

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

                if (trashbinEmpty != null) trashbinEmpty.SetActive(true);
                if (trashbinFull != null) trashbinFull.SetActive(false);
            }
            else
            {
                completionPanel.SetActive(true);
                GameObject[] trashObjects = GameObject.FindGameObjectsWithTag("Trash");
                foreach (GameObject trash in trashObjects)
                {
                    trash.SetActive(false);
                }

                if (trashCountText != null)
                {
                    trashCountText.gameObject.SetActive(false);
                }

                if (trashbinEmpty != null) trashbinEmpty.SetActive(false);
                if (trashbinFull != null) trashbinFull.SetActive(true);
            }
        }

        // Initialize trash collection state
        GameObject[] allTrash = GameObject.FindGameObjectsWithTag("Trash");
        int collectedCount = 0;
        int totalCount = allTrash.Length;

        foreach (GameObject trash in allTrash)
        {
            ItemsIDTracking idTracker = trash.GetComponent<ItemsIDTracking>();
            if (idTracker != null)
            {
                if (ItemCollectionTracker.IsCollected(idTracker.itemID))
                {
                    trash.SetActive(true);
                    collectedCount++;
                    Debug.Log(idTracker.itemID + " was collected — showing trash piece.");
                }
                else
                {
                    trash.SetActive(false);
                    Debug.Log(idTracker.itemID + " not collected — hiding trash piece");
                }
            }
        }

        // Update the UI text
        if (trashCountText != null)
        {
            trashCountText.text = $"Trash Collected: {collectedCount} / {totalCount}";
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

        if (trashCountText != null)
        {
            trashCountText.gameObject.SetActive(false);
        }

        if (trashbinEmpty != null) trashbinEmpty.SetActive(false);
        if (trashbinFull != null) trashbinFull.SetActive(true);
    }
}