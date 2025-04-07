using UnityEngine;

public class TrashCollisionDisabling : MonoBehaviour
{
    private Collider2D thisCollider;

    void Awake()
    {
        thisCollider = GetComponent<Collider2D>();
    }

    public void DisableCollisionWithOtherTrash()
    {
        GameObject[] allTrash = GameObject.FindGameObjectsWithTag("Trash");
        foreach (GameObject trash in allTrash)
        {
            if (trash != this.gameObject)
            {
                Collider2D otherCollider = trash.GetComponent<Collider2D>();
                if (otherCollider != null && thisCollider != null)
                {
                    Physics2D.IgnoreCollision(thisCollider, otherCollider);
                }
            }
        }
    }
}
