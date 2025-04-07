using UnityEngine;
using UnityEngine.SceneManagement;

public class FishManager : MonoBehaviour
{
    [Header("Fish References")]
    public GameObject fishSmall;
    public GameObject fishBig;

    [Header("Fish Food")]
    public GameObject fishFood;

    [Header("Letters")]
    public GameObject[] letters;

    [Header("Burp Settings")]
    public int clicksToBurp = 4;
    private int clickCount = 0;

    [Header("UI Settings")]
    public GameObject completionPanel;

    [Header("Audio Settings")]
    public AudioSource clickSound; // Sound when clicking the fish
    public AudioSource burpSound;  // Sound when fish burps
    public AudioSource feedSound;  // Sound when feeding fish
    public AudioSource letterSound; // Sound when collecting letters

    private bool isFed = false;
    private bool hasBurped = false;
    private int lettersCollected = 0;
    private bool isFeedFishCompleted = false;

    void Start()
    {
        // Check if the task was completed through ChoreManager
        if (ChoreManager.Instance != null && ChoreManager.Instance.IsChoreCompleted("feedfish"))
        {
            isFeedFishCompleted = true;
            completionPanel.SetActive(true);
        }
        else
        {
            isFeedFishCompleted = false;
            completionPanel.SetActive(false);
        }

        // Enable fish food in scene if it was collected from closet
        if (fishFood != null)
        {
            ItemsIDTracking idTracker = fishFood.GetComponent<ItemsIDTracking>();
            if (idTracker != null)
            {
                string id = idTracker.itemID;

                if (ItemCollectionTracker.IsCollected(id) && !isFeedFishCompleted)
                {
                    fishFood.SetActive(true);
                }
                else
                {
                    fishFood.SetActive(false);
                }
            }
            else
            {
                Debug.LogWarning("fishFood is missing ItemsIDTracking");
            }
        }

        // Set initial states
        fishSmall.SetActive(true);
        fishBig.SetActive(false);
        foreach (GameObject letter in letters)
            letter.SetActive(false);

        Debug.Log("FishManager Start - Letters Hidden");
    }

    public void FeedFish()
    {
        isFed = true;
        fishFood.SetActive(false);
        fishSmall.SetActive(false);
        fishBig.SetActive(true);
        
        // Play feeding sound
        if (feedSound != null)
        {
            feedSound.Play();
        }
        
        Debug.Log("Fish has been fed.");
    }

    public void OnBigFishClicked()
    {
        if (!isFed || hasBurped) return;

        clickCount++;

        // Play click sound
        if (clickSound != null)
        {
            clickSound.Play();
        }

        Debug.Log("Big fish clicked. Click count: " + clickCount);

        if (clickCount < clicksToBurp)
        {
            // Grow fish slightly 
            fishBig.transform.localScale *= 1.1f;
        }
        else
        {
            BurpFish();
        }
    }

    private void BurpFish()
    {
        hasBurped = true;

        // Play burp sound
        if (burpSound != null)
        {
            burpSound.Play();
        }

        // Spawn letters
        foreach (GameObject letter in letters)
        {
            letter.SetActive(true);
            Debug.Log("Letter " + letter.name + " spawned.");
        }

        // Reset fish
        fishBig.SetActive(false);
        fishSmall.SetActive(true);
        fishBig.transform.localScale = Vector3.one; // Reset size

        Debug.Log("Fish burped. Letters are visible now.");
    }

    public void CollectLetter(GameObject letter)
    {
        Debug.Log("Attempting to collect: " + letter.name);
        letter.SetActive(false);
        lettersCollected++;
        
        // Play letter collection sound
        if (letterSound != null)
        {
            letterSound.Play();
        }
        
        Debug.Log(letter.name + " collected. Total collected: " + lettersCollected);
        Debug.Log("Letters collected so far: " + lettersCollected + " out of 4.");

        if (lettersCollected == 4 && !isFeedFishCompleted)
        {
            CompleteFishChore();
        }
    }

    private void CompleteFishChore()
    {
        isFeedFishCompleted = true;
        Debug.Log("FISH TASK COMPLETED - All letters collected.");

        if (ChoreManager.Instance != null)
        {
            ChoreManager.Instance.CompleteChore("feedfish");
        }

        PlayerPrefs.SetInt("FeedFishCompleted", 1);
        ShowCompletionUI();
    }

    private void ShowCompletionUI()
    {
        if (completionPanel != null)
        {
            completionPanel.SetActive(true);
            Debug.Log("Showing completion UI panel");
        }
        else
        {
            Debug.LogWarning("Completion panel reference not set in FishManager");
        }
    }

    public void HideCompletionUI()
    {
        if (completionPanel != null)
        {
            completionPanel.SetActive(false);
        }
    }

    public bool HasFishBurped()
    {
        return hasBurped;
    }
}