using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void LaunchBedroom()
    {
        ResetChoreManager();
        PlushHeadSwap.MarkForReset();
        ResetTimer();

        if (DataManager.Instance != null)
        {
            DataManager.Instance.ClearPlayerPosition();
        }

        DestroyAllPersistentObjects();
        SceneManager.LoadSceneAsync(2);
    }

    public void PlayBedroom()
    {
        SetPlayerPosition();
        SaveTime();
        SceneManager.LoadSceneAsync(2);
    }

    public void PlayHomework()
    {
        SetPlayerPosition();
        SaveTime();
        SceneManager.LoadSceneAsync(3);
    }

    public void PlayFishbowl()
    {
        SetPlayerPosition();
        SaveTime();
        SceneManager.LoadSceneAsync(4);
    }

    public void PlayPlushies()
    {
        SetPlayerPosition();
        SaveTime();
        SceneManager.LoadSceneAsync(5);
    }

    public void PlaySoup()
    {
        SetPlayerPosition();
        SaveTime();
        SceneManager.LoadSceneAsync(6);
    }

    public void PlayCloset()
    {
        SetPlayerPosition();
        SaveTime();
        SceneManager.LoadSceneAsync(7);
    }

    public void PlayGarbage()
    {
        SetPlayerPosition();
        SaveTime();
        SceneManager.LoadSceneAsync(8);
    }

    public void PlayUnderBed()
    {
        SetPlayerPosition();
        SaveTime();
        SceneManager.LoadSceneAsync(9);
    }

    public void CallMom()
    {
        SetPlayerPosition();
        SaveTime();
        SceneManager.LoadSceneAsync(10);
    }

    public void LoadMonsterJumpscare()
    {
        SetPlayerPosition();
        SaveTime();
        SceneManager.LoadSceneAsync(12);
    }

    public void LoadCredits()
    {
        SceneManager.LoadSceneAsync(11);
    }

    public void LoadMainMenu()
    {
        if (DataManager.Instance != null)
        {
            DataManager.Instance.ClearAllData();
        }

        PlushHeadSwap.MarkForReset();
        ResetChoreManager();
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void SetPlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && DataManager.Instance != null)
        {
            DataManager.Instance.SetPlayerPosition(player.transform.position);
        }
    }

    private void SaveTime()
    {
        TimerScript timer = FindObjectOfType<TimerScript>();
        if (timer != null && DataManager.Instance != null)
        {
            DataManager.Instance.SetTime(timer.GetCurrentHour(), timer.GetCurrentMinute(), timer.GetCurrentSecond());
        }
    }

    private void ResetChoreManager()
    {
        ChoreManager choreManager = ChoreManager.Instance;
        if (choreManager != null)
        {
            choreManager.isAlphabetSoupCompleted = false;
            choreManager.isSwapPlushiesCompleted = false;
            choreManager.isMathHomeworkCompleted = false;
            choreManager.isGarbageCompleted = false;
            choreManager.isOrganizeClosetCompleted = false;
            choreManager.isFeedFishCompleted = false;
        }
    }

    private void ResetTimer()
    {
        TimerScript timer = FindObjectOfType<TimerScript>();
        if (timer != null)
        {
            timer.SetTime(6, 0, 0);
            timer.RestartTimer();
        }
    }

    private void DestroyAllPersistentObjects()
    {
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj == gameObject) continue;
            if (obj.GetComponent<DataManager>() != null) continue;
            if (obj.GetComponent<ChoreManager>() != null) continue;
            if (obj.GetComponent<TimerScript>() != null) continue;

            if (obj.scene.buildIndex == -1)
            {
                Destroy(obj);
            }
        }
    }
}