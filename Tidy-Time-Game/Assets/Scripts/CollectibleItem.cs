using UnityEngine;
using System.Collections;

public class CollectibleItem : MonoBehaviour
{
    public string itemID = "Item"; // Item tracking
    public FishManager fishManager;  // Reference to FishManager (for letters?)
    public AudioSource pickupAudioSource; // Reference to AudioSource component

    void Start()
    {
        // If no AudioSource is assigned, try to get one from this object
        if (pickupAudioSource == null)
        {
            pickupAudioSource = GetComponent<AudioSource>();
        }

        // Configure audio source if it exists
        if (pickupAudioSource != null)
        {
            pickupAudioSource.playOnAwake = false;
            pickupAudioSource.spatialBlend = 1f; // 3D sound
            pickupAudioSource.volume = 0.7f; // Default volume
            pickupAudioSource.loop = false;
            pickupAudioSource.priority = 128; // Normal priority
        }
    }

    void OnMouseDown()
    {
        Debug.Log(itemID + " collected");

        // Play pickup sound if available
        if (pickupAudioSource != null && pickupAudioSource.clip != null)
        {
            // Create a temporary GameObject to play the sound
            // This allows multiple sounds to overlap
            GameObject tempAudioObj = new GameObject("TempAudio");
            AudioSource tempAudioSource = tempAudioObj.AddComponent<AudioSource>();
            
            // Copy settings from our main AudioSource
            tempAudioSource.clip = pickupAudioSource.clip;
            tempAudioSource.volume = pickupAudioSource.volume;
            tempAudioSource.pitch = pickupAudioSource.pitch;
            tempAudioSource.spatialBlend = pickupAudioSource.spatialBlend;
            tempAudioSource.outputAudioMixerGroup = pickupAudioSource.outputAudioMixerGroup;
            
            tempAudioSource.Play();
            Destroy(tempAudioObj, pickupAudioSource.clip.length); // Clean up after sound finishes
        }

        ItemCollectionTracker.MarkCollected(itemID);

        // Notify FishManager that a letter was collected
        if (fishManager != null)
        {
            fishManager.CollectLetter(gameObject); // Pass the collected letter
        }
        
        // Immediately hide the object (sound will continue playing)
        gameObject.SetActive(false);
    }
}