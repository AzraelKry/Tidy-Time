using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlushHeadSwap : MonoBehaviour
{
    private Vector3 startPosition;
    private bool isDragging = false;

    void Start()
    {
        startPosition = transform.position;
    }

    private void OnMouseDown()
    {
        Debug.Log(gameObject.name + " clicked");
        isDragging = true;
    }

    private void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePosition.x, mousePosition.y, 0);
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;

        Collider2D hitCollider = Physics2D.OverlapPoint(transform.position);

        if (hitCollider != null)
        {
            if (hitCollider.CompareTag("PlushHead")) // Swapping with another head
            {
                SwapHeads(hitCollider.GetComponent<PlushHeadSwap>());
            }
            else if (hitCollider.CompareTag("PlushBody")) // Dropping on an empty body
            {
                transform.position = hitCollider.transform.position; // Snap into place
            }
        }
        else
        {
            Debug.Log("Dropped outside valid area, resetting.");
            transform.position = startPosition; // Reset if dropped in an invalid area
        }
    }

    
    private void SwapHeads(PlushHeadSwap otherHead)
    {
        if (otherHead != null && otherHead != this) // Ensure swapping with another head
        {
            Debug.Log("Swapping " + gameObject.name + " with " + otherHead.gameObject.name);

            // Store original positions
            Vector3 firstPosition = transform.position;
            Vector3 secondPosition = otherHead.transform.position;

            // Swap positions
            transform.position = secondPosition;
            otherHead.transform.position = firstPosition;
        }

        PlushGameManager.Instance.CheckTaskCompletion();
    }
}

