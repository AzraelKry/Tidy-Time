private void OnMouseUp()
{
    isDragging = false;

    // Get this head's collider to use its bounds.
    Collider2D myCollider = GetComponent<Collider2D>();
    // Use OverlapBoxAll with the collider's center and size.
    Collider2D[] hitColliders = Physics2D.OverlapBoxAll(myCollider.bounds.center, myCollider.bounds.size, 0f);

    PlushHeadSwap targetHead = null;
    Collider2D targetBody = null;

    // Look through all overlapping colliders.
    foreach (Collider2D col in hitColliders)
    {
        // If we find another head (and it isn't ourselves), mark it as the target.
        if (col.CompareTag("PlushHead") && col.gameObject != gameObject)
        {
            targetHead = col.GetComponent<PlushHeadSwap>();
            break; // Give swapping priority.
        }
        // Otherwise, if a body is found, record it.
        else if (col.CompareTag("PlushBody"))
        {
            targetBody = col;
        }
    }

    if (targetHead != null)
    {
        // Swap directly if a head is detected.
        SwapHeads(targetHead);
    }
    else if (targetBody != null)
    {
        // Use a slightly relaxed threshold to determine if a head is already attached to this body.
        float snapThreshold = 0.3f; // Adjust as needed based on your art and collider sizes.
        PlushHeadSwap attachedHead = null;

        // Search through all heads to find one that is currently snapped to this body.
        foreach (PlushHeadSwap head in FindObjectsOfType<PlushHeadSwap>())
        {
            if (head != this && Vector3.Distance(head.startPosition, targetBody.transform.position) < snapThreshold)
            {
                attachedHead = head;
                break;
            }
        }

        if (attachedHead != null)
        {
            // If there is already a head on this body, swap with that head.
            SwapHeads(attachedHead);
        }
        else
        {
            // Otherwise, snap this head to the body's position and update the stored valid position.
            transform.position = targetBody.transform.position;
            startPosition = targetBody.transform.position;
        }
    }
    else
    {
        UnityEngine.Debug.Log("Dropped outside valid area, resetting.");
        // If no valid colliders are detected, reset to the last valid (attached) position.
        transform.position = startPosition;
    }
}