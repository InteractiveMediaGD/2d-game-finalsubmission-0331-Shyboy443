using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player movement, health, damage, and healing.
/// Uses new Input System for controls.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float horizontalMin = -7.2f;
    public float horizontalMax = 1.5f;
    public float verticalBound = 4.5f;
    public float aimTurnSpeed = 720f;

    [Header("Health")]
    public int maxHealth = 5;
    public float invincibilityTime = 1.5f;

    [Header("Visuals")]
    public Color playerColor = new Color(0.3f, 0.7f, 1f);

    private int currentHealth;
    private bool invincible;
    private float invincibleTimer;
    private SpriteRenderer[] visualRenderers;
    private HealthBarUI healthBar;
    private PlayerPowerUps powerUps;
    private Transform shipVisual;
    private Transform engineGlow;
    private Vector2 aimDirection = Vector2.right;
    private float baseMoveSpeed;
    private int baseMaxHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public Vector2 AimDirection => aimDirection;
    public Vector3 AimOrigin => shipVisual != null ? shipVisual.position : transform.position;

    void Start()
    {
        baseMoveSpeed = moveSpeed;
        baseMaxHealth = maxHealth;
        currentHealth = maxHealth;
        EnsurePlayerVisuals();
        EnsurePowerUps();
        visualRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        shipVisual = transform.Find("ShipVisual");
        engineGlow = transform.Find("EngineGlow");

        EnsureHealthBarReference();
        if (healthBar) healthBar.UpdateHealth(currentHealth, maxHealth);
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameplayActive) return;

        HandleMovement();
        HandleAim();
        HandleInvincibility();
    }

    void HandleMovement()
    {
        Vector2 input = Vector2.zero;
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) input.y += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) input.y -= 1f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) input.x -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input.x += 1f;
        }

        if (input.sqrMagnitude > 1f)
            input.Normalize();

        transform.position += (Vector3)(input * (moveSpeed * Time.deltaTime));

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, horizontalMin, horizontalMax);
        pos.y = Mathf.Clamp(pos.y, -verticalBound, verticalBound);
        transform.position = pos;
    }

    void HandleAim()
    {
        if (TryGetMouseWorldPosition(out Vector3 mouseWorld))
        {
            Vector2 mouseAim = mouseWorld - AimOrigin;
            if (mouseAim.sqrMagnitude > 0.001f)
                aimDirection = mouseAim.normalized;
        }

        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - 90f;
        if (shipVisual != null)
        {
            Quaternion target = Quaternion.Euler(0f, 0f, angle);
            shipVisual.rotation = Quaternion.RotateTowards(
                shipVisual.rotation,
                target,
                aimTurnSpeed * Time.deltaTime);
        }

        if (engineGlow != null)
        {
            Vector2 parentScale = GetSafeScale();
            engineGlow.localPosition = new Vector3(
                (-aimDirection.x * 0.32f) / parentScale.x,
                (-aimDirection.y * 0.32f) / parentScale.y,
                0f);
            float pulse = 0.72f + Mathf.Sin(Time.time * 18f) * 0.08f;
            engineGlow.localScale = new Vector3(
                pulse / parentScale.x,
                pulse / parentScale.y,
                1f);
        }
    }

    void HandleInvincibility()
    {
        if (!invincible) return;

        invincibleTimer -= Time.deltaTime;

        SetVisualAlpha(Mathf.Sin(Time.time * 20f) > 0 ? 1f : 0.3f);

        if (invincibleTimer <= 0)
        {
            invincible = false;
            SetVisualAlpha(1f);
        }
    }

    public void TakeDamage(int amount)
    {
        if (invincible) return;
        if (GameManager.Instance != null && !GameManager.Instance.IsGameplayActive) return;

        EnsurePowerUps();
        if (powerUps != null && powerUps.AbsorbHit())
            return;

        EnsureHealthBarReference();
        currentHealth = Mathf.Max(0, currentHealth - amount);
        if (healthBar) healthBar.UpdateHealth(currentHealth, maxHealth);
        RefreshShipDamageVisual();
        GameAudio.Instance?.PlayDamage();

        var shake = FindAnyObjectByType<ScreenShake>();
        if (shake) shake.TriggerShake();

        var flash = FindAnyObjectByType<DamageFlash>();
        if (flash) flash.Flash();

        if (currentHealth <= 0)
        {
            if (GameManager.Instance) GameManager.Instance.TriggerGameOver();
            return;
        }

        invincible = true;
        invincibleTimer = invincibilityTime;
    }

    public void Heal(int amount)
    {
        EnsureHealthBarReference();
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        if (healthBar) healthBar.UpdateHealth(currentHealth, maxHealth);
        RefreshShipDamageVisual();
    }

    public void IncreaseMaxHealth(int amount)
    {
        if (amount <= 0)
            return;

        maxHealth += amount;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        EnsureHealthBarReference();
        if (healthBar) healthBar.UpdateHealth(currentHealth, maxHealth);
        RefreshShipDamageVisual();
    }

    public void ApplyEngineBoost(float percent)
    {
        moveSpeed *= 1f + Mathf.Max(0f, percent);
    }

    public void ResetForNewRun(GameManager.GameMode mode)
    {
        moveSpeed = baseMoveSpeed;
        maxHealth = baseMaxHealth;

        if (mode == GameManager.GameMode.Challenge)
            moveSpeed *= 1.08f;

        currentHealth = maxHealth;
        invincible = false;
        invincibleTimer = 0f;
        transform.position = new Vector3(-6f, 0f, 0f);
        EnsureHealthBarReference();
        if (healthBar) healthBar.UpdateHealth(currentHealth, maxHealth);
        RefreshShipDamageVisual();
        SetVisualAlpha(1f);
    }

    public bool TryGetMouseWorldPosition(out Vector3 mouseWorld)
    {
        mouseWorld = transform.position;

        if (Camera.main == null || Mouse.current == null)
            return false;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        float planeDistance = -Camera.main.transform.position.z;
        mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, planeDistance));
        mouseWorld.z = 0f;
        return true;
    }

    public Vector3 GetProjectileSpawnPosition(float muzzleDistance = 0.7f)
    {
        return AimOrigin + (Vector3)(aimDirection * muzzleDistance);
    }

    void EnsurePlayerVisuals()
    {
        Vector2 parentScale = GetSafeScale();

        SpriteRenderer rootRenderer = GetComponent<SpriteRenderer>();
        if (rootRenderer != null)
        {
            rootRenderer.sprite = null;
            rootRenderer.enabled = false;
        }

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
            col.size = new Vector2(0.55f / parentScale.x, 0.35f / parentScale.y);

        EnsureTrail();
        EnsureGlow(parentScale);
        EnsureShipSprite(parentScale);
    }

    void EnsureShipSprite(Vector2 parentScale)
    {
        shipVisual = transform.Find("ShipVisual");
        if (shipVisual == null)
        {
            shipVisual = new GameObject("ShipVisual").transform;
            shipVisual.SetParent(transform, false);
        }

        shipVisual.localPosition = Vector3.zero;
        shipVisual.localScale = new Vector3(0.9f / parentScale.x, 0.9f / parentScale.y, 1f);
        shipVisual.localEulerAngles = new Vector3(0f, 0f, -90f);

        SpriteRenderer shipRenderer = shipVisual.GetComponent<SpriteRenderer>();
        if (shipRenderer == null)
            shipRenderer = shipVisual.gameObject.AddComponent<SpriteRenderer>();

        shipRenderer.sprite = KenneyAssets.Player;
        shipRenderer.color = shipRenderer.sprite == SpriteHelper.Square ? playerColor : Color.white;
        shipRenderer.sortingOrder = 5;
        shipRenderer.enabled = true;
        RefreshShipDamageVisual();
    }

    void EnsureGlow(Vector2 parentScale)
    {
        engineGlow = transform.Find("EngineGlow");
        if (engineGlow == null)
        {
            engineGlow = new GameObject("EngineGlow").transform;
            engineGlow.SetParent(transform, false);
        }

        engineGlow.localPosition = new Vector3(-0.32f / parentScale.x, 0f, 0f);
        engineGlow.localScale = new Vector3(0.78f / parentScale.x, 0.78f / parentScale.y, 1f);
        engineGlow.localEulerAngles = Vector3.zero;

        SpriteRenderer glowRenderer = engineGlow.GetComponent<SpriteRenderer>();
        if (glowRenderer == null)
            glowRenderer = engineGlow.gameObject.AddComponent<SpriteRenderer>();

        glowRenderer.sprite = SpriteHelper.Circle;
        glowRenderer.color = new Color(0.4f, 0.8f, 1f, 0.25f);
        glowRenderer.sortingOrder = 4;
        glowRenderer.enabled = true;
    }

    void EnsureTrail()
    {
        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail == null)
            trail = gameObject.AddComponent<TrailRenderer>();

        trail.time = 0.3f;
        trail.startWidth = 0.25f;
        trail.endWidth = 0f;
        trail.startColor = new Color(0.5f, 0.8f, 1f, 0.6f);
        trail.endColor = new Color(0.3f, 0.6f, 1f, 0f);
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.sortingOrder = 4;
        trail.numCapVertices = 3;
        trail.minVertexDistance = 0.05f;
    }

    Vector2 GetSafeScale()
    {
        Vector3 scale = transform.lossyScale;
        return new Vector2(
            Mathf.Max(Mathf.Abs(scale.x), 0.001f),
            Mathf.Max(Mathf.Abs(scale.y), 0.001f));
    }

    void SetVisualAlpha(float alpha)
    {
        if (visualRenderers == null) return;

        for (int i = 0; i < visualRenderers.Length; i++)
        {
            SpriteRenderer renderer = visualRenderers[i];
            if (renderer == null || !renderer.enabled) continue;

            Color color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }
    }

    void EnsureHealthBarReference()
    {
        if (healthBar == null)
            healthBar = FindAnyObjectByType<HealthBarUI>();
    }

    void EnsurePowerUps()
    {
        if (powerUps == null)
            powerUps = GetComponent<PlayerPowerUps>() ?? gameObject.AddComponent<PlayerPowerUps>();
    }

    void RefreshShipDamageVisual()
    {
        if (shipVisual == null)
            shipVisual = transform.Find("ShipVisual");

        if (shipVisual == null)
            return;

        SpriteRenderer shipRenderer = shipVisual.GetComponent<SpriteRenderer>();
        if (shipRenderer == null)
            return;

        float ratio = maxHealth > 0 ? (float)currentHealth / maxHealth : 1f;
        int damageLevel = ratio < 0.28f ? 3 : ratio < 0.5f ? 2 : ratio < 0.75f ? 1 : 0;
        if (damageLevel > 0)
            shipRenderer.sprite = KenneyAssets.GetPlayerDamageSprite(damageLevel);
        else
            shipRenderer.sprite = KenneyAssets.GetPlayerShipForUnlockCount(SaveProfile.ShipSkinUnlockCount);
    }
}
