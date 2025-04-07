using System.Collections.Generic;
using UnityEngine;

public static class ItemCollectionTracker
{
    // Store IDs of all collected items
    private static HashSet<string> collectedItems = new HashSet<string>();

    public static bool IsCollected(string itemId)
    {
        return collectedItems.Contains(itemId);
    }

    public static void MarkCollected(string itemId)
    {
        if (!collectedItems.Contains(itemId))
        {
            collectedItems.Add(itemId);
            Debug.Log("Marked " + itemId + " as collected");
        }
    }

    public static void ResetCollection()
    {
        collectedItems.Clear();
        Debug.Log("Item collection tracker reset.");
    }
}