using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlushHeadSwap : MonoBehaviour
{
    private Vector3 startPosition;
    private Vector3 dragOffset;
    private bool isDragging = false;
    private bool isLocked = false;
    private Color originalColor;
    private SpriteRenderer spriteRenderer;

    public Transform correctBody;
    public float snapThreshold = 0.5f;
    public float lockThreshold = 0.3f;
    public float hoverAlpha = 0.6f;
    public float hoverBeforePickupAlpha = 0.8f;
    public float swapHighlightAlpha = 0.5f;

    [Header("Audio")]
    public AudioSource swapAudioSource;

    private static List<Transform> allBodies = new List<Transform>();
    private static Dictionary<Transform, PlushHeadSwap> bodyToHeadMap = new Dictionary<Transform, PlushHeadSwap>();
    private static bool initialized = false;
    private static bool shouldResetOnLoad = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        // Reset static variables if needed
        if (shouldResetOnLoad)
        {
            allBodies.Clear();
            bodyToHeadMap.Clear();
            initialized = false;
            shouldResetOnLoad = false;
        }

        // Initialize the list of all bodies once
        if (!initialized)
        {
            initialized = true;
            PlushHeadSwap[] allHeads = FindObjectsOfType<PlushHeadSwap>();
            foreach (PlushHeadSwap head in allHeads)
            {
                if (head.correctBody != null && !allBodies.Contains(head.correctBody))
                {
                    allBodies.Add(head.correctBody);
                }
            }
        }
    }

    void Start()
    {
        // Assign to a random unoccupied body position at start
        List<Transform> availableBodies = new List<Transform>(allBodies);
        availableBodies.Remove(correctBody);

        // Remove already occupied bodies
        List<Transform> occupiedBodies = new List<Transform>(bodyToHeadMap.Keys);
        availableBodies.RemoveAll(body => occupiedBodies.Contains(body));

        if (availableBodies.Count > 0)
        {
            int randomIndex = Random.Range(0, availableBodies.Count);
            Transform startBody = availableBodies[randomIndex];
            
            transform.position = startBody.position;
            startPosition = startBody.position;
            bodyToHeadMap[startBody] = this;
        }
        else
        {
            transform.position = correctBody.position;
            startPosition = correctBody.position;
            bodyToHeadMap[correctBody] = this;
        }

        CheckAndUpdateLockStatus(true);
    }

    private void OnMouseEnter()
    {
        if (!isDragging && !isLocked)
        {
            SetTransparency(hoverBeforePickupAlpha);
        }
    }

    private void OnMouseExit()
    {
        if (!isDragging && !isLocked)
        {
            ResetTransparency();
        }
    }

    private void OnMouseDown()
    {
        if (isLocked) return;

        ResetAllTransparency();
        isDragging = true;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = transform.position.z;
        dragOffset = transform.position - mouseWorldPos;

        foreach (var pair in new Dictionary<Transform, PlushHeadSwap>(bodyToHeadMap))
        {
            if (pair.Value == this)
            {
                bodyToHeadMap.Remove(pair.Key);
                break;
            }
        }
    }

    private void OnMouseDrag()
    {
        if (isDragging && !isLocked)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = transform.position.z;
            transform.position = mousePosition + dragOffset;

            foreach (Transform body in allBodies)
            {
                if (bodyToHeadMap.ContainsKey(body))
                {
                    PlushHeadSwap otherHead = bodyToHeadMap[body];
                    if (otherHead != this && Vector3.Distance(transform.position, body.position) < snapThreshold)
                    {
                        otherHead.SetTransparency(swapHighlightAlpha);
                    }
                    else
                    {
                        otherHead.ResetTransparency();
                    }
                }
            }
        }
    }

    private void OnMouseUp()
    {
        if (isLocked) return;
        
        ResetAllTransparency();
        isDragging = false;

        Transform closestBody = null;
        float closestDistance = float.MaxValue;

        foreach (Transform body in allBodies)
        {
            float distance = Vector3.Distance(transform.position, body.position);
            if (distance < snapThreshold && distance < closestDistance)
            {
                closestDistance = distance;
                closestBody = body;
            }
        }

        if (closestBody != null)
        {
            if (bodyToHeadMap.ContainsKey(closestBody))
            {
                PlushHeadSwap otherHead = bodyToHeadMap[closestBody];
                otherHead.transform.position = startPosition;
                bodyToHeadMap.Remove(closestBody);
                
                otherHead.startPosition = startPosition;
                bodyToHeadMap[startPosition == correctBody.position ? correctBody : GetBodyAtPosition(startPosition)] = otherHead;
                otherHead.CheckAndUpdateLockStatus(true);
                
                if (swapAudioSource != null && !swapAudioSource.isPlaying)
                {
                    swapAudioSource.Play();
                }
            }

            transform.position = closestBody.position;
            startPosition = closestBody.position;
            bodyToHeadMap[closestBody] = this;
        }
        else
        {
            transform.position = startPosition;
            bodyToHeadMap[startPosition == correctBody.position ? correctBody : GetBodyAtPosition(startPosition)] = this;
        }

        CheckAndUpdateLockStatus(true);

        if (PlushGameManager.Instance != null)
        {
            PlushGameManager.Instance.CheckTaskCompletion();
        }
    }

    private Transform GetBodyAtPosition(Vector3 position)
    {
        foreach (Transform body in allBodies)
        {
            if (Vector3.Distance(position, body.position) < 0.1f)
            {
                return body;
            }
        }
        return null;
    }

    private void ResetAllTransparency()
    {
        foreach (PlushHeadSwap head in FindObjectsOfType<PlushHeadSwap>())
        {
            head.ResetTransparency();
        }
    }

    private void CheckAndUpdateLockStatus(bool forceCheck = false)
    {
        if (correctBody == null) return;

        float distance = Vector3.Distance(transform.position, correctBody.position);
        bool shouldBeLocked = distance < lockThreshold;

        if (shouldBeLocked)
        {
            transform.position = correctBody.position;
            startPosition = correctBody.position;
            isLocked = true;
            
            if (!bodyToHeadMap.ContainsKey(correctBody) || bodyToHeadMap[correctBody] != this)
            {
                bodyToHeadMap[correctBody] = this;
            }
        }
        else if (forceCheck || isLocked)
        {
            isLocked = false;
        }
    }

    public bool IsCorrectlyPlaced()
    {
        return isLocked;
    }

    public void SetTransparency(float alpha)
    {
        if (spriteRenderer != null)
        {
            Color newColor = originalColor;
            newColor.a = alpha;
            spriteRenderer.color = newColor;
        }
    }

    public void ResetTransparency()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    public static void MarkForReset()
    {
        shouldResetOnLoad = true;
    }
}