using UnityEngine;

public class CollectiblesStateEnabler : MonoBehaviour
{
    private void Start()
    {
        ItemsIDTracking idTracker = GetComponent<ItemsIDTracking>();

        if (idTracker != null && ItemCollectionTracker.IsCollected(idTracker.itemID))
        {
            gameObject.SetActive(false);
            Debug.Log($"{idTracker.itemID} was already collected — hiding it.");
        }
    }
}