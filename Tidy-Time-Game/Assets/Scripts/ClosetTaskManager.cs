using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosetTaskManager : MonoBehaviour
{
    public GameObject[] clothingItems; // unorganized
    public GameObject[] organizedVersions; // organized
    public GameObject[] collectables; // trash and fish food
    public GameObject uiWithHints; // UI at the start
    public GameObject finalUi; // UI after completion (no bubbles)
    public GameObject completionPanel; // UI panel shown on completion

    [Header("Audio")]
    public AudioSource placementSound; // Sound when any clothing is placed

    private bool taskComplete = false;
    private List<GameObject> clothingQueue = new List<GameObject>();
    private int currentItemIndex = 0;
    private bool[] wasActiveLastFrame; // Track previous frame state

    void Start()
    {
        wasActiveLastFrame = new bool[clothingItems.Length];
        
        // Initialize tracking array
        for (int i = 0; i < clothingItems.Length; i++)
        {
            wasActiveLastFrame[i] = clothingItems[i].activeSelf;
        }

        // Check completion if ChoreManager exists in scene
        if (ChoreManager.Instance != null && ChoreManager.Instance.IsChoreCompleted("organizecloset"))
        {
            SetSceneToCompletedState();
            taskComplete = true;
            return;
        }

        // Shuffle clothingItems into clothingQueue
        clothingQueue = new List<GameObject>(clothingItems);
        Shuffle(clothingQueue);

        // Deactivate all, then activate first
        foreach (GameObject item in clothingItems)
            item.SetActive(false);

        if (clothingQueue.Count > 0)
            clothingQueue[0].SetActive(true);

        foreach (GameObject item in collectables)
        {
            ItemsIDTracking idTracker = item.GetComponent<ItemsIDTracking>();
            if (idTracker != null && ItemCollectionTracker.IsCollected(idTracker.itemID))
            {
                item.SetActive(false);
                Debug.Log("Hiding already collected item - " + idTracker.itemID);
            }
        }
    }

    void Update()
    {
        if (taskComplete) return;

        // Progress through clothing items
        if (currentItemIndex < clothingQueue.Count && !clothingQueue[currentItemIndex].activeSelf)
        {
            currentItemIndex++;
            if (currentItemIndex < clothingQueue.Count)
                clothingQueue[currentItemIndex].SetActive(true);
        }

        // Check each item for placement changes
        for (int i = 0; i < clothingItems.Length; i++)
        {
            bool isActiveNow = clothingItems[i].activeSelf;
            
            // If was active last frame but not now = just placed
            if (wasActiveLastFrame[i] && !isActiveNow)
            {
                if (placementSound != null)
                {
                    placementSound.Play();
                    Debug.Log("Played placement sound for item " + i);
                }
            }
            
            // Update tracking for next frame
            wasActiveLastFrame[i] = isActiveNow;
        }

        bool allClothesPlaced = true;
        foreach (GameObject item in clothingItems)
        {
            if (item.activeSelf)
            {
                allClothesPlaced = false;
                break;
            }
        }

        bool allCollected = true;
        foreach (GameObject item in collectables)
        {
            if (item.activeSelf)
            {
                allCollected = false;
                break;
            }
        }

        if (allClothesPlaced && allCollected)
        {
            taskComplete = true;
            Debug.Log("Closet chore completed");
            ChoreManager.Instance.CompleteChore("organizecloset");

            if (uiWithHints != null) uiWithHints.SetActive(false);
            if (finalUi != null) finalUi.SetActive(true);
            if (completionPanel != null) completionPanel.SetActive(true);
        }
    }

    private void SetSceneToCompletedState()
    {
        if (uiWithHints != null) uiWithHints.SetActive(false);
        if (finalUi != null) finalUi.SetActive(true);
        if (completionPanel != null) completionPanel.SetActive(true);

        foreach (GameObject item in clothingItems)
            item.SetActive(false);

        foreach (GameObject item in organizedVersions)
            item.SetActive(true);

        foreach (GameObject item in collectables)
            item.SetActive(false);
    }

    void Shuffle(List<GameObject> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            GameObject temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}