using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlushGameManager : MonoBehaviour
{
    public static PlushGameManager Instance;
    public Transform[] correctHeadPositions;
    public Transform[] heads;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    
    public void CheckTaskCompletion()
    {
        bool allCorrect = true;

        for (int i = 0; i < heads.Length; i++)
        {
            if (heads[i].position != correctHeadPositions[i].position)
            {
                allCorrect = false;
                break;
            }
        }

        if (allCorrect)
        {
            Debug.Log("All plushies matched correctly");
            Invoke("ReturnToBedroom", 2f);
        }
    }

    void ReturnToBedroom()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Bedroom");
    }
   
}
