using UnityEngine;

/// <summary>
/// Manages temporary player power-up effects and their visuals.
/// </summary>
public class PlayerPowerUps : MonoBehaviour
{
    public enum PowerUpType
    {
        Shield,
        RapidFire,
        SpreadShot,
    }

    public float shieldDuration = 8f;
    public float rapidFireDuration = 8f;
    public float spreadShotDuration = 9f;

    float shieldTimer;
    float rapidFireTimer;
    float spreadShotTimer;
    Transform shieldVisual;
    SpriteRenderer shieldRenderer;

    public bool HasShield => shieldTimer > 0f;
    public bool HasRapidFire => rapidFireTimer > 0f;
    public bool HasSpreadShot => spreadShotTimer > 0f;
    public float ShieldTimeRemaining => shieldTimer;
    public float RapidFireTimeRemaining => rapidFireTimer;
    public float SpreadShotTimeRemaining => spreadShotTimer;
    public float CooldownMultiplier => HasRapidFire ? 0.55f : 1f;
    public int ExtraShotPairs => HasSpreadShot ? 1 : 0;

    void Awake()
    {
        EnsureShieldVisual();
        UpdateVisuals();
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameplayActive)
        {
            UpdateVisuals();
            return;
        }

        shieldTimer = Mathf.Max(0f, shieldTimer - Time.deltaTime);
        rapidFireTimer = Mathf.Max(0f, rapidFireTimer - Time.deltaTime);
        spreadShotTimer = Mathf.Max(0f, spreadShotTimer - Time.deltaTime);
        UpdateVisuals();
    }

    public void ApplyPowerUp(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.Shield:
                shieldTimer = shieldDuration;
                break;
            case PowerUpType.RapidFire:
                rapidFireTimer = rapidFireDuration;
                break;
            case PowerUpType.SpreadShot:
                spreadShotTimer = spreadShotDuration;
                break;
        }

        UpdateVisuals();
    }

    public bool AbsorbHit()
    {
        if (!HasShield)
            return false;

        shieldTimer = 0f;
        UpdateVisuals();
        VFXHelper.SpawnImpact(transform.position, new Color(0.35f, 0.85f, 1f));
        return true;
    }

    public string GetStatusText()
    {
        string status = string.Empty;
        AppendStatus(ref status, HasShield, "Shield", shieldTimer);
        AppendStatus(ref status, HasRapidFire, "Rapid Fire", rapidFireTimer);
        AppendStatus(ref status, HasSpreadShot, "Spread Shot", spreadShotTimer);
        return status;
    }

    public bool IsActive(PowerUpType type)
    {
        return type switch
        {
            PowerUpType.Shield => HasShield,
            PowerUpType.RapidFire => HasRapidFire,
            PowerUpType.SpreadShot => HasSpreadShot,
            _ => false,
        };
    }

    public float GetTimeRemaining(PowerUpType type)
    {
        return type switch
        {
            PowerUpType.Shield => shieldTimer,
            PowerUpType.RapidFire => rapidFireTimer,
            PowerUpType.SpreadShot => spreadShotTimer,
            _ => 0f,
        };
    }

    void EnsureShieldVisual()
    {
        shieldVisual = transform.Find("ShieldVisual");
        if (shieldVisual == null)
        {
            shieldVisual = new GameObject("ShieldVisual").transform;
            shieldVisual.SetParent(transform, false);
        }

        shieldVisual.localPosition = Vector3.zero;
        shieldVisual.localScale = Vector3.one * 1.45f;

        shieldRenderer = shieldVisual.GetComponent<SpriteRenderer>();
        if (shieldRenderer == null)
            shieldRenderer = shieldVisual.gameObject.AddComponent<SpriteRenderer>();

        shieldRenderer.sprite = SpriteHelper.Circle;
        shieldRenderer.sortingOrder = 3;
    }

    void UpdateVisuals()
    {
        EnsureShieldVisual();

        if (shieldRenderer != null)
        {
            shieldRenderer.enabled = HasShield;
            if (HasShield)
            {
                float pulse = 0.9f + Mathf.Sin(Time.time * 7f) * 0.08f;
                shieldVisual.localScale = Vector3.one * (1.45f * pulse);
                shieldRenderer.color = new Color(0.35f, 0.85f, 1f, 0.28f);
            }
        }
    }

    static void AppendStatus(ref string status, bool active, string label, float timer)
    {
        if (!active)
            return;

        if (!string.IsNullOrEmpty(status))
            status += "  |  ";

        status += label + " " + timer.ToString("F1") + "s";
    }
}
