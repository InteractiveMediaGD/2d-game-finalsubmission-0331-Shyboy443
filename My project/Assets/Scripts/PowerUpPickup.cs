using UnityEngine;

/// <summary>
/// Pickup that grants the player a temporary combat buff.
/// </summary>
public class PowerUpPickup : MonoBehaviour
{
    public PlayerPowerUps.PowerUpType powerUpType;

    Transform glowRing;
    Transform iconTransform;
    SpriteRenderer pickupRenderer;
    SpriteRenderer iconRenderer;

    void Start()
    {
        glowRing = transform.Find("Glow");
        pickupRenderer = GetComponent<SpriteRenderer>();
        RefreshVisuals();
    }

    void Update()
    {
        float pulse = 1f + Mathf.Sin(Time.time * 5f) * 0.1f;
        transform.localScale = new Vector3(0.7f * pulse, 0.7f * pulse, 1f);

        if (glowRing != null)
        {
            float glowPulse = 1.3f + Mathf.Sin(Time.time * 4f) * 0.12f;
            glowRing.localScale = Vector3.one * glowPulse;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerPowerUps powerUps = other.GetComponent<PlayerPowerUps>() ?? other.GetComponentInParent<PlayerPowerUps>();
        if (powerUps != null)
            powerUps.ApplyPowerUp(powerUpType);

        GameAudio.Instance?.PlayPickup();
        VFXHelper.SpawnPickupEffect(transform.position, GetGlowColor(), 14);
        Destroy(gameObject);
    }

    public void RefreshVisuals()
    {
        if (pickupRenderer == null)
            pickupRenderer = GetComponent<SpriteRenderer>();

        if (glowRing == null)
            glowRing = transform.Find("Glow");

        EnsureIconRenderer();

        if (pickupRenderer != null)
        {
            pickupRenderer.sprite = GetBadgeSprite();
            pickupRenderer.color = Color.white;
        }

        if (iconRenderer != null)
        {
            iconRenderer.sprite = GetIconSprite();
            iconRenderer.color = Color.white;
        }

        if (glowRing != null)
        {
            SpriteRenderer glowRenderer = glowRing.GetComponent<SpriteRenderer>();
            if (glowRenderer != null)
                glowRenderer.color = GetGlowColor();
        }
    }

    void EnsureIconRenderer()
    {
        if (iconTransform == null)
            iconTransform = transform.Find("Icon");

        if (iconTransform == null)
        {
            iconTransform = new GameObject("Icon").transform;
            iconTransform.SetParent(transform, false);
        }

        iconTransform.localPosition = Vector3.zero;
        iconTransform.localScale = Vector3.one * 0.56f;

        iconRenderer = iconTransform.GetComponent<SpriteRenderer>();
        if (iconRenderer == null)
            iconRenderer = iconTransform.gameObject.AddComponent<SpriteRenderer>();

        iconRenderer.sortingOrder = 5;
    }

    Sprite GetBadgeSprite()
    {
        return powerUpType switch
        {
            PlayerPowerUps.PowerUpType.Shield => KenneyAssets.ShieldBadge,
            PlayerPowerUps.PowerUpType.RapidFire => KenneyAssets.RapidFireBadge,
            PlayerPowerUps.PowerUpType.SpreadShot => KenneyAssets.SpreadShotBadge,
            _ => SpriteHelper.Circle,
        };
    }

    Sprite GetIconSprite()
    {
        return powerUpType switch
        {
            PlayerPowerUps.PowerUpType.Shield => KenneyAssets.ShieldIcon,
            PlayerPowerUps.PowerUpType.RapidFire => KenneyAssets.RapidFireIcon,
            PlayerPowerUps.PowerUpType.SpreadShot => KenneyAssets.SpreadShotIcon,
            _ => SpriteHelper.Circle,
        };
    }

    Color GetGlowColor()
    {
        return powerUpType switch
        {
            PlayerPowerUps.PowerUpType.Shield => new Color(0.35f, 0.85f, 1f, 0.22f),
            PlayerPowerUps.PowerUpType.RapidFire => new Color(1f, 0.82f, 0.25f, 0.22f),
            PlayerPowerUps.PowerUpType.SpreadShot => new Color(1f, 0.45f, 0.35f, 0.22f),
            _ => new Color(1f, 1f, 1f, 0.2f),
        };
    }
}
