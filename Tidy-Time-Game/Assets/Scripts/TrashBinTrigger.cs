using UnityEngine;

public class TrashBinTrigger : MonoBehaviour
{
    private int trashInBinCount = 0;
    private int totalTrashPieces;

    void Start()
    {
        // Count all trash pieces at the start
        totalTrashPieces = GameObject.FindGameObjectsWithTag("Trash").Length;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Trash"))
        {
            Debug.Log("Trash got in the bin");
            trashInBinCount++;
            other.tag = "Untagged"; // Prevent double-counting

            // Disable future collision with other trash
            TrashCollisionDisabling disable = other.GetComponent<TrashCollisionDisabling>();
            if (disable != null)
            {
                disable.DisableCollisionWithOtherTrash();
            }

            if (trashInBinCount >= totalTrashPieces)
            {
                Debug.Log("All trash is in the bin");
                
                // Safely call ChoreManager (your original requirement)
                if (ChoreManager.Instance != null)
                {
                    ChoreManager.Instance.CompleteChore("garbage");
                }
                else
                {
                    Debug.LogError("ChoreManager.Instance is missing!");
                }

                // Safely call TrashManager (your new requirement)
                if (TrashManager.Instance != null)
                {
                    TrashManager.Instance.NotifyTrashCompleted();
                }
                else
                {
                    Debug.LogWarning("TrashManager.Instance is missing (optional)");
                }
            }
        }
    }
}