using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class PlushHeadSwap : MonoBehaviour
{
    // This stores the head’s last valid (snapped) position – essentially, the body position.
    private Vector3 startPosition;
    // Used to hold the offset between the head's position and the mouse click point.
    private Vector3 dragOffset;
    // Flag for whether this head is currently being dragged.
    private bool isDragging = false;

    void Start()
    {
        // Store the initial position as the valid body attachment position.
        startPosition = transform.position;
    }

    // Called when the head is clicked.
    private void OnMouseDown()
    {
        UnityEngine.Debug.Log(gameObject.name + " clicked");
        isDragging = true;
        // Calculate the offset between the head position and the mouse's world position.
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // Maintain the z value (we assume heads are at z=0).
        mouseWorldPos.z = 0;
        dragOffset = transform.position - mouseWorldPos;
    }

    // Called every frame while the mouse is held down.
    private void OnMouseDrag()
    {
        if (isDragging)
        {
            // Get the current mouse position in world space.
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;
            // Apply the offset so the head doesn't jump.
            transform.position = mousePosition + dragOffset;
        }
    }

    // Called when the mouse button is released.
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


    // Swaps the stored valid positions of the two heads, and then snaps them to those positions.
    private void SwapHeads(PlushHeadSwap otherHead)
    {
        if (otherHead != null && otherHead != this)
        {
            UnityEngine.Debug.Log("Swapping " + gameObject.name + " with " + otherHead.gameObject.name);

            // Swap the stored valid positions.
            Vector3 tempPosition = startPosition;
            startPosition = otherHead.startPosition;
            otherHead.startPosition = tempPosition;

            // Snap each head to its new valid position.
            transform.position = startPosition;
            otherHead.transform.position = otherHead.startPosition;
        }

        // Check if the overall game task is completed after swapping.
        PlushGameManager.Instance.CheckTaskCompletion();
    }
}
