using UnityEngine;

//manages fish feeding mechanics and letter collection
public class FishFeedingChore : MonoBehaviour
{
    public enum ObjectType { Fish, Letter }

    [Tooltip("Specify the type of this object")]
    [SerializeField] private ObjectType objectType = ObjectType.Fish;

    [Header("Fish Settings")]
    [Tooltip("Next fish stage game object")]
    [SerializeField] private GameObject nextFishStage;

    [Tooltip("Initial fish to reactivate when resetting")]
    [SerializeField] private GameObject initialFish;

    [Tooltip("Letters to spawn when fish is fully grown")]
    [SerializeField] private GameObject[] lettersToSpawn;

    [SerializeField] private Item letterItem;


    //handles click behavior for fish objects 
    private void HandleFishClick()
    {
        if (nextFishStage != null)
        {
            //progress to next growth stage
            nextFishStage.SetActive(true);
            gameObject.SetActive(false);
        }
        else
        {
            //fish is fully grown - spit letters and reset
            SpitLetters();
            ResetFish();
        }
    }


    //activates all assigned letter objects
    private void SpitLetters()
    {
        if (lettersToSpawn == null) return;

        foreach (GameObject letter in lettersToSpawn)
        {
            if (letter != null)
            {
                letter.SetActive(true);
            }
        }
    }

    //resets fish to initial state
    private void ResetFish()
    {
        if (initialFish != null)
        {
            initialFish.SetActive(true);
        }
        gameObject.SetActive(false);
    }

    //handles click behavior for letter objects
    private void HandleLetterClick()
    {
        //try to add the letter to the inventory
        if (InventoryManager.instance.AddItem(letterItem))
        {
            //successfully added to inventory; disable the letter object
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("Inventory is full or letter cannot be added.");
        }
    }


    private void OnMouseDown()
    {

        switch (objectType)
        {
            case ObjectType.Fish:
                HandleFishClick();
                break;
            case ObjectType.Letter:
                HandleLetterClick();
                break;
        }
    }

}