using UnityEngine;

public class CollectiblesStateReset : MonoBehaviour
{
    private void Start()
    {
        ItemCollectionTracker.ResetCollection();
    }
}
