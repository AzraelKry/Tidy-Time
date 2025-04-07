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
    public GameObject completionPanel; // Assign a UI panel in the inspector

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

        Debug.Log("Fish has been fed.");
    }

    public void OnBigFishClicked()
    {
        if (!isFed || hasBurped) return;

        clickCount++;

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
        // Debug to check if the letter collection is triggered
        Debug.Log("Attempting to collect: " + letter.name);

        // Deactivate the collected letter
        letter.SetActive(false);
        lettersCollected++;

        // Debug statement to confirm letter collection
        Debug.Log(letter.name + " collected. Total collected: " + lettersCollected);

        // Ensure we log the state after each collection
        Debug.Log("Letters collected so far: " + lettersCollected + " out of 4.");

        // Check if all letters have been collected
        if (lettersCollected == 4 && !isFeedFishCompleted)
        {
            CompleteFishChore();
        }
    }

    private void CompleteFishChore()
    {
        isFeedFishCompleted = true;
        Debug.Log("FISH TASK COMPLETED - All letters collected.");

        // Mark chore as completed in ChoreManager
        if (ChoreManager.Instance != null)
        {
            ChoreManager.Instance.CompleteChore("feedfish");
        }

        // Save the task completion state to PlayerPrefs
        PlayerPrefs.SetInt("FeedFishCompleted", 1);

        // Show completion UI panel
        ShowCompletionUI();
    }

    private void ShowCompletionUI()
    {
        if (completionPanel != null)
        {
            completionPanel.SetActive(true);

            // Optional: Add any UI animation or sound here
            Debug.Log("Showing completion UI panel");
        }
        else
        {
            Debug.LogWarning("Completion panel reference not set in FishManager");
        }
    }

    // Optional: Add a method to hide the UI panel if needed
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