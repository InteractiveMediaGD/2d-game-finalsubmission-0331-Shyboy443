using UnityEngine;

/// <summary>
/// Camera screen shake effect triggered on player damage.
/// Adds impactful visual feedback on collision with obstacles and enemies.
/// </summary>
public class ScreenShake : MonoBehaviour
{
    [Header("Default Shake Settings")]
    public float defaultDuration = 0.25f;
    public float defaultMagnitude = 0.3f;

    private float timer;
    private float currentMagnitude;
    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    void LateUpdate()
    {
        if (timer > 0)
        {
            float offsetX = Random.Range(-1f, 1f) * currentMagnitude;
            float offsetY = Random.Range(-1f, 1f) * currentMagnitude;
            transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0);
            timer -= Time.deltaTime;
        }
        else
        {
            transform.localPosition = originalPosition;
        }
    }

    public void TriggerShake(float duration = -1f, float magnitude = -1f)
    {
        timer = duration > 0 ? duration : defaultDuration;
        currentMagnitude = magnitude > 0 ? magnitude : defaultMagnitude;
    }
}
