using UnityEngine;

/// <summary>
/// Trigger zone placed in the obstacle gap.
/// Awards score when the player passes through.
/// </summary>
public class ScoreTrigger : MonoBehaviour
{
    private bool scored;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (scored) return;
        if (other.CompareTag("Player"))
        {
            scored = true;
            if (GameManager.Instance) GameManager.Instance.AddScore(1);
        }
    }
}
