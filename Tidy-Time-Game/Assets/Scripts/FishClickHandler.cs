using UnityEngine;

public class FishClickHandler : MonoBehaviour
{
    private FishManager manager;

    public GameObject burpHintText;

    private void Start()
    {
        manager = FindObjectOfType<FishManager>();

        if (burpHintText != null)
            burpHintText.SetActive(false);
    }

    private void OnMouseEnter()
    {
        if (burpHintText != null && !manager.HasFishBurped())
        {
            burpHintText.SetActive(true);
        }
    }

    private void OnMouseExit()
    {
        if (burpHintText != null)
        {
            burpHintText.SetActive(false);
        }
    }

    private void OnMouseDown()
    {
        if (manager != null)
        {
            manager.OnBigFishClicked();

            if (manager.HasFishBurped() && burpHintText != null)
            {
                burpHintText.SetActive(false);
            }
        }
    }
}
