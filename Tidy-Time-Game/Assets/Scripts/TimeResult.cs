using UnityEngine;
using TMPro;

public class TimeResult : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI timeResultText; // Assign in inspector
    public TextMeshProUGUI rankDescriptionText; // Assign text object for rank description
    
    [Header("Rank Sprites")]
    public GameObject rankAPlusPlus; // A++ sprite object
    public GameObject rankAPlus;    // A+ sprite object
    public GameObject rankA;        // A sprite object
    public GameObject rankB;        // B sprite object
    public GameObject rankC;        // C sprite object
    public GameObject rankD;        // D sprite object

    [Header("Rank Descriptions")]
    [TextArea] public string aPlusPlusText;
    [TextArea] public string aPlusText;
    [TextArea] public string aText;
    [TextArea] public string bText;
    [TextArea] public string cText;
    [TextArea] public string dText;

    void Start()
    {
        // Verify text components exist
        if (timeResultText == null)
        {
            Debug.LogError("TimeResult: Missing time TextMeshProUGUI reference!");
            timeResultText = GetComponent<TextMeshProUGUI>();
        }

        if (rankDescriptionText == null)
        {
            Debug.LogError("TimeResult: Missing rank description TextMeshProUGUI reference!");
        }

        DisplayCompletionTime();
    }

    void DisplayCompletionTime()
    {
        if (DataManager.Instance == null)
        {
            timeResultText.text = "Time data not available";
            if (rankDescriptionText != null) rankDescriptionText.text = "";
            Debug.LogWarning("DataManager instance missing!");
            return;
        }

        // Get the saved completion time
        var (hour, minute, second) = DataManager.Instance.GetTime();
        
        // Format the time string (e.g., "6:30:15 PM")
        string formattedTime = $"{hour}:{minute:D2}:{second:D2} PM";
        
        // Display the result
        timeResultText.text = $"{formattedTime}";
        
        // Calculate total minutes for comparison
        float totalMinutes = hour * 60 + minute + second / 60f;
        
        // Hide all rank sprites first
        if (rankAPlusPlus != null) rankAPlusPlus.SetActive(false);
        if (rankAPlus != null) rankAPlus.SetActive(false);
        if (rankA != null) rankA.SetActive(false);
        if (rankB != null) rankB.SetActive(false);
        if (rankC != null) rankC.SetActive(false);
        if (rankD != null) rankD.SetActive(false);
        
        // Determine which rank sprite to show and set appropriate text
        if (totalMinutes < 7 * 60) // Before 7
        {
            if (rankAPlusPlus != null) rankAPlusPlus.SetActive(true);
            if (rankDescriptionText != null) rankDescriptionText.text = aPlusPlusText;
        }
        else if (totalMinutes < 7 * 60 + 30) // Between 7 and 7:30
        {
            if (rankAPlus != null) rankAPlus.SetActive(true);
            if (rankDescriptionText != null) rankDescriptionText.text = aPlusText;
        }
        else if (totalMinutes < 7 * 60 + 50) // Between 7:30 and 7:50
        {
            if (rankA != null) rankA.SetActive(true);
            if (rankDescriptionText != null) rankDescriptionText.text = aText;
        }
        else if (totalMinutes < 8 * 60 + 10) // Between 7:50 and 8:10
        {
            if (rankB != null) rankB.SetActive(true);
            if (rankDescriptionText != null) rankDescriptionText.text = bText;
        }
        else if (totalMinutes < 8 * 60 + 30) // Between 8:10 and 8:30
        {
            if (rankC != null) rankC.SetActive(true);
            if (rankDescriptionText != null) rankDescriptionText.text = cText;
        }
        else // Between 8:30 and 9:00
        {
            if (rankD != null) rankD.SetActive(true);
            if (rankDescriptionText != null) rankDescriptionText.text = dText;
        }
    }
}