using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableLetter : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Components")]
    [HideInInspector] public RectTransform rectTransform;
    [HideInInspector] public Canvas canvas;
    [HideInInspector] public CanvasGroup canvasGroup;
    
    [Header("Settings")]
    public Vector2 startPosition;
    public Transform startParent;
    public bool isCorrectlyPlaced = false;
    
    [Header("Hover Settings")]
    [Range(0.1f, 1f)]
    public float hoverAlpha = 0.7f;
    
    private float originalAlpha;
    private SoupManager soupManager;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        originalAlpha = canvasGroup.alpha;
    }

    private void Start()
    {
        soupManager = FindObjectOfType<SoupManager>();
        startParent = transform.parent;
        startPosition = rectTransform.anchoredPosition;
        CheckPlacement();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isCorrectlyPlaced)
        {
            canvasGroup.alpha = hoverAlpha;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        canvasGroup.alpha = originalAlpha;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isCorrectlyPlaced) return;
        
        startPosition = rectTransform.anchoredPosition;
        startParent = transform.parent;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = originalAlpha;
        
        LetterPlaceholder.OnDragStarted(this);
        transform.SetParent(canvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isCorrectlyPlaced) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        LetterPlaceholder.OnDragEnded();

        if (transform.parent == canvas.transform)
        {
            ReturnToStartPosition();
        }
        else
        {
            // Only play sound when successfully dropped onto a new spot
            if (soupManager != null) 
            {
                soupManager.PlayLetterSound();
            }
            CheckPlacement();
        }
    }

    public void CheckPlacement()
    {
        bool newCorrectState = IsOnCorrectPlaceholder();
        
        if (newCorrectState && !isCorrectlyPlaced)
        {
            isCorrectlyPlaced = true;
            CheckAllLettersCorrect();
        }
        else if (!newCorrectState && isCorrectlyPlaced)
        {
            isCorrectlyPlaced = false;
        }
    }

    private bool IsOnCorrectPlaceholder()
    {
        if (transform.parent == null) return false;
        string letterName = name.Replace("Letter ", "");
        string parentName = transform.parent.name.Replace("Placeholder ", "");
        return letterName == parentName;
    }

    private void CheckAllLettersCorrect()
    {
        DraggableLetter[] allLetters = FindObjectsOfType<DraggableLetter>();
        foreach (DraggableLetter letter in allLetters)
        {
            if (!letter.isCorrectlyPlaced) return;
        }
        
        if (soupManager != null)
        {
            soupManager.CheckForCompletion();
        }
    }

    public void ReturnToStartPosition()
    {
        transform.SetParent(startParent);
        rectTransform.anchoredPosition = startPosition;
        isCorrectlyPlaced = false;
    }
}