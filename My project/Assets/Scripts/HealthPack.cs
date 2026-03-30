using UnityEngine;

/// <summary>
/// Health pack that heals the player on contact with visual feedback.
/// Green circle with white cross and glow ring - visually distinct from enemies.
/// Destroyed on collision with player or when scrolled off-screen (via parent).
/// </summary>
public class HealthPack : MonoBehaviour
{
    public int healAmount = 1;
    private Transform glowRing;

    void Start()
    {
        // Find glow ring child for animation
        if (transform.childCount > 0)
            glowRing = transform.GetChild(0);
    }

    void Update()
    {
        // Pulsing animation
        float pulse = 1f + Mathf.Sin(Time.time * 4f) * 0.08f;
        transform.localScale = new Vector3(0.72f * pulse, 0.72f * pulse, 1f);

        // Animate glow ring
        if (glowRing != null)
        {
            float glowPulse = 1.25f + Mathf.Sin(Time.time * 3f) * 0.14f;
            glowRing.localScale = Vector3.one * glowPulse;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player) player.Heal(healAmount);
            GameAudio.Instance?.PlayPickup();
            VFXHelper.SpawnPickupEffect(transform.position, new Color(0.2f, 1f, 0.5f), 10);
            Destroy(gameObject);
        }
    }
}
