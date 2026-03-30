using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Full-screen red flash overlay when the player takes damage.
/// Provides visual feedback for damage events.
/// </summary>
public class DamageFlash : MonoBehaviour
{
    public Image flashImage;
    public float flashDuration = 0.3f;
    public float maxAlpha = 0.35f;

    private float timer;

    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            float alpha = Mathf.Lerp(0f, maxAlpha, timer / flashDuration);
            if (flashImage) flashImage.color = new Color(1f, 0f, 0f, alpha);
        }
        else if (flashImage != null && flashImage.color.a > 0)
        {
            flashImage.color = Color.clear;
        }
    }

    public void Flash()
    {
        timer = flashDuration;
    }
}
