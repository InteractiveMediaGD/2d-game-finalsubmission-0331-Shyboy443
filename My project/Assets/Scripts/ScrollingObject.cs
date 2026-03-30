using UnityEngine;

/// <summary>
/// Moves the game object to the left at the current game scroll speed.
/// Destroys itself when it goes off-screen.
/// </summary>
public class ScrollingObject : MonoBehaviour
{
    public float destroyX = -15f;

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameplayActive) return;

        transform.Translate(Vector3.left * (GameManager.Instance.ScrollSpeed * Time.deltaTime));

        if (transform.position.x < destroyX)
            Destroy(gameObject);
    }
}
