using System.Collections;
using UnityEngine;

public class TrashThrow : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 startPos;
    private bool isDragging = false;
    private LineRenderer lineRenderer;

    public float forceMultiplier = 6f;
    public int lineSegmentCount = 15;
    public float timeStep = 0.1f;

    private Vector3 originalPosition;
    private bool hasLaunched = false;
    private int originalLayer; // Store original layer

    void Start()
    {
        originalPosition = transform.position;
        originalLayer = gameObject.layer; // Store the original layer
        rb = GetComponent<Rigidbody2D>();
        lineRenderer = GetComponent<LineRenderer>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        lineRenderer.enabled = false;
    }

    void OnMouseDown()
    {
        startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        isDragging = true;
        lineRenderer.enabled = true;
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 currentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentPos.z = transform.position.z;
            transform.position = currentPos;

            Vector2 dragVector = startPos - (Vector2)currentPos;
            DrawTrajectory(transform.position, dragVector * forceMultiplier);
        }
    }

    void OnMouseUp()
    {
        Vector2 endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 launchDir = startPos - endPos;

        hasLaunched = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;
        rb.AddForce(launchDir * forceMultiplier, ForceMode2D.Impulse);

        // Play sound effect when thrown
        TrashManager.Instance?.PlayTrashThrowSound();

        gameObject.layer = LayerMask.NameToLayer("TrashActive");
        isDragging = false;
        lineRenderer.enabled = false;
    }

    void DrawTrajectory(Vector2 startPoint, Vector2 velocity)
    {
        Vector3[] points = new Vector3[lineSegmentCount];
        for (int i = 0; i < lineSegmentCount; i++)
        {
            float t = i * timeStep;
            points[i] = new Vector3(
                startPoint.x + velocity.x * t,
                startPoint.y + velocity.y * t + 0.5f * Physics2D.gravity.y * t * t,
                transform.position.z
            );
        }
        lineRenderer.positionCount = lineSegmentCount;
        lineRenderer.SetPositions(points);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasLaunched && collision.collider.CompareTag("SceneBorder"))
        {
            StartCoroutine(ResetTrash());
        }
    }

    private IEnumerator ResetTrash()
    {
        yield return new WaitForSeconds(0.5f);
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        transform.position = originalPosition;
        hasLaunched = false;
        gameObject.layer = originalLayer; // Reset to original layer
    }
}