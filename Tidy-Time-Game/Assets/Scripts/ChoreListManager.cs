using UnityEngine;

public class ChoresListManager : MonoBehaviour
{
    public GameObject choresListPanel;
    public PlayerScript playerMovement;
    public TimerScript timerScript;
    public AudioSource openSound;  // Sound when opening the chore list

    void Start()
    {
        if (timerScript == null)
        {
            timerScript = FindObjectOfType<TimerScript>();
        }
        
        // Initialize audio sources if they exist
        if (openSound != null)
        {
            openSound.playOnAwake = false;
        }
    }

    public void OpenChoreList()
    {
        if (choresListPanel != null)
        {
            choresListPanel.SetActive(true);
            
            // Play open sound if assigned
            if (openSound != null)
            {
                openSound.Play();
            }
            
            if (playerMovement != null)
            {
                playerMovement.enabled = false;
                playerMovement.StopMovement();
            }
            // Time continues running (don't pause timer)
        }
        else
        {
            Debug.LogError("Chores List Panel is not assigned!");
        }
    }

    public void CloseChoreList()
    {
        if (choresListPanel != null)
        {
            choresListPanel.SetActive(false);
            
            if (playerMovement != null)
            {
                playerMovement.enabled = true;
            }
        }
        else
        {
            Debug.LogError("Chores List Panel is not assigned!");
        }
    }
}