using UnityEngine;

public class SoupManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject completionPanel;

    [Header("Chore Settings")]
    public string choreName = "AlphabetSoup";

    [Header("Audio Settings")]
    public AudioSource letterSound; // Single sound for letter drops

    private LetterRandomizer letterRandomizer;

    private void Start()
    {
        letterRandomizer = FindObjectOfType<LetterRandomizer>();

        if (completionPanel != null)
        {
            completionPanel.SetActive(false);
        }

        if (ChoreManager.Instance != null && ChoreManager.Instance.IsChoreCompleted(choreName))
        {
            PlaceLettersCorrectly();
            MarkChoreComplete();
        }
        else
        {
            letterRandomizer.RandomizeLetters(false);
        }

        // Hide uncollected letters (B, U, R, P)
        string[] collectedLetters = { "letterB", "letterU", "letterR", "letterP" };

        foreach (string letterID in collectedLetters)
        {
            DraggableLetter[] allLetters = FindObjectsOfType<DraggableLetter>();
            foreach (DraggableLetter letter in allLetters)
            {
                ItemsIDTracking idTracker = letter.GetComponent<ItemsIDTracking>();
                if (idTracker != null && idTracker.itemID == letterID)
                {
                    letter.gameObject.SetActive(ItemCollectionTracker.IsCollected(letterID));
                }
            }
        }
    }

    public void PlayLetterSound()
    {
        if (letterSound != null)
        {
            letterSound.PlayOneShot(letterSound.clip);
        }
    }

    public void CheckForCompletion()
    {
        DraggableLetter[] allLetters = FindObjectsOfType<DraggableLetter>();
        foreach (DraggableLetter letter in allLetters)
        {
            if (!letter.isCorrectlyPlaced) return;
        }

        CompleteSoupChore();
    }

    private void CompleteSoupChore()
    {
        if (ChoreManager.Instance != null)
        {
            ChoreManager.Instance.CompleteChore(choreName);
        }
        MarkChoreComplete();
    }

    private void MarkChoreComplete()
    {
        if (completionPanel != null)
        {
            completionPanel.SetActive(true);
        }
    }

    private void PlaceLettersCorrectly()
    {
        DraggableLetter[] allLetters = FindObjectsOfType<DraggableLetter>();
        LetterPlaceholder[] allPlaceholders = FindObjectsOfType<LetterPlaceholder>();

        foreach (DraggableLetter letter in allLetters)
        {
            string letterName = letter.name.Replace("Letter ", "");
            foreach (LetterPlaceholder placeholder in allPlaceholders)
            {
                if (placeholder.name == "Placeholder " + letterName)
                {
                    letter.transform.SetParent(placeholder.transform);
                    letter.rectTransform.anchoredPosition = Vector2.zero;
                    letter.startParent = placeholder.transform;
                    letter.startPosition = Vector2.zero;
                    letter.isCorrectlyPlaced = true;
                    break;
                }
            }
        }
    }
}