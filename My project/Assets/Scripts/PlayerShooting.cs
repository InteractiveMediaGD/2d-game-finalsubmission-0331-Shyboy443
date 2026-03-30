using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles the player's weapon switching, charge shots, and projectile creation.
/// </summary>
public class PlayerShooting : MonoBehaviour
{
    [Header("Base Projectile Settings")]
    public float pulseSpeed = 16f;
    public float projectileLifetime = 2.5f;
    public float pulseCooldown = 0.12f;
    public float muzzleDistance = 0.75f;

    float cooldownTimer;
    float cooldownBoostMultiplier = 1f;
    float chargeHoldTimer;
    bool wasHoldingFire;
    int damageBoost;
    bool unlockedMissile;
    bool unlockedBeam;
    bool unlockedCharge;
    bool unlockedPiercer;
    WeaponType currentWeapon = WeaponType.Pulse;
    PlayerController playerController;
    PlayerPowerUps powerUps;

    public string CurrentWeaponLabel => currentWeapon switch
    {
        WeaponType.Missile => "Missile",
        WeaponType.Beam => "Beam",
        WeaponType.Charge => "Charge",
        WeaponType.Piercer => "Piercer",
        _ => "Pulse",
    };

    public int UnlockedWeaponCount
    {
        get
        {
            int count = 1;
            if (unlockedMissile) count++;
            if (unlockedBeam) count++;
            if (unlockedCharge) count++;
            if (unlockedPiercer) count++;
            return count;
        }
    }

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        powerUps = GetComponent<PlayerPowerUps>();
        ApplyPermanentUnlocks();
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameplayActive)
            return;

        cooldownTimer -= Time.deltaTime;
        HandleWeaponSwitchInput();

        Mouse mouse = Mouse.current;
        if (mouse == null)
            return;

        if (currentWeapon == WeaponType.Charge)
            HandleChargeWeapon(mouse);
        else if (mouse.leftButton.isPressed && cooldownTimer <= 0f)
            FireCurrentWeapon(1f);
    }

    public void ResetForNewRun()
    {
        cooldownTimer = 0f;
        cooldownBoostMultiplier = 1f;
        chargeHoldTimer = 0f;
        wasHoldingFire = false;
        damageBoost = 0;
        currentWeapon = WeaponType.Pulse;
        ApplyPermanentUnlocks();
    }

    public bool IsWeaponUnlocked(WeaponType weapon)
    {
        return weapon switch
        {
            WeaponType.Missile => unlockedMissile,
            WeaponType.Beam => unlockedBeam,
            WeaponType.Charge => unlockedCharge,
            WeaponType.Piercer => unlockedPiercer,
            _ => true,
        };
    }

    public void UnlockWeapon(WeaponType weapon, bool setCurrent)
    {
        switch (weapon)
        {
            case WeaponType.Missile:
                unlockedMissile = true;
                SaveProfile.AddPermanentWeaponFlag(1 << (int)WeaponType.Missile);
                break;
            case WeaponType.Beam:
                unlockedBeam = true;
                SaveProfile.AddPermanentWeaponFlag(1 << (int)WeaponType.Beam);
                break;
            case WeaponType.Charge:
                unlockedCharge = true;
                SaveProfile.AddPermanentWeaponFlag(1 << (int)WeaponType.Charge);
                break;
            case WeaponType.Piercer:
                unlockedPiercer = true;
                SaveProfile.AddPermanentWeaponFlag(1 << (int)WeaponType.Piercer);
                break;
        }

        if (setCurrent)
            currentWeapon = weapon;
    }

    public void ApplyDamageBoost(int amount)
    {
        damageBoost += Mathf.Max(1, amount);
    }

    public void ApplyCooldownBoost(float percent)
    {
        cooldownBoostMultiplier *= 1f - Mathf.Clamp(percent, 0.02f, 0.6f);
    }

    void ApplyPermanentUnlocks()
    {
        int flags = SaveProfile.PermanentWeaponFlags;
        unlockedMissile = (flags & (1 << (int)WeaponType.Missile)) != 0 || SaveProfile.TotalScrap >= 80;
        unlockedBeam = (flags & (1 << (int)WeaponType.Beam)) != 0 || SaveProfile.TotalScrap >= 180;
        unlockedCharge = (flags & (1 << (int)WeaponType.Charge)) != 0 || SaveProfile.TotalScrap >= 300;
        unlockedPiercer = (flags & (1 << (int)WeaponType.Piercer)) != 0 || SaveProfile.TotalScrap >= 420;
    }

    void HandleWeaponSwitchInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.digit1Key.wasPressedThisFrame)
            currentWeapon = WeaponType.Pulse;
        else if (keyboard.digit2Key.wasPressedThisFrame && unlockedMissile)
            currentWeapon = WeaponType.Missile;
        else if (keyboard.digit3Key.wasPressedThisFrame && unlockedBeam)
            currentWeapon = WeaponType.Beam;
        else if (keyboard.digit4Key.wasPressedThisFrame && unlockedCharge)
            currentWeapon = WeaponType.Charge;
        else if (keyboard.digit5Key.wasPressedThisFrame && unlockedPiercer)
            currentWeapon = WeaponType.Piercer;
    }

    void HandleChargeWeapon(Mouse mouse)
    {
        if (mouse.leftButton.isPressed)
        {
            chargeHoldTimer = Mathf.Min(1.4f, chargeHoldTimer + Time.deltaTime);
            wasHoldingFire = true;
            return;
        }

        if (!wasHoldingFire || cooldownTimer > 0f)
            return;

        float normalizedCharge = Mathf.Clamp01(chargeHoldTimer / 1.1f);
        FireCurrentWeapon(normalizedCharge);
        chargeHoldTimer = 0f;
        wasHoldingFire = false;
    }

    void FireCurrentWeapon(float power)
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
        if (powerUps == null)
            powerUps = GetComponent<PlayerPowerUps>();

        Vector2 direction = playerController != null && playerController.AimDirection.sqrMagnitude > 0.001f
            ? playerController.AimDirection.normalized
            : Vector2.right;
        Vector3 spawnPos = playerController != null
            ? playerController.GetProjectileSpawnPosition(muzzleDistance)
            : transform.position;

        switch (currentWeapon)
        {
            case WeaponType.Missile:
                SpawnProjectile(spawnPos, direction, KenneyAssets.MissileProjectile, 10.5f, 3.2f, 3 + damageBoost,
                    new Vector3(0.75f, 0.75f, 1f), new Color(1f, 0.76f, 0.34f), new Color(1f, 0.35f, 0.15f),
                    new Color(1f, 0.6f, 0.2f), 1, 1.15f);
                cooldownTimer = 0.34f * cooldownBoostMultiplier;
                break;
            case WeaponType.Beam:
                SpawnProjectile(spawnPos, direction, KenneyAssets.BeamProjectile, 22f, 1.2f, 1 + damageBoost,
                    new Vector3(0.72f, 0.72f, 1f), new Color(0.4f, 1f, 0.8f), new Color(0.1f, 0.7f, 0.5f),
                    new Color(0.4f, 1f, 0.8f), 3, 0f);
                cooldownTimer = 0.08f * cooldownBoostMultiplier;
                break;
            case WeaponType.Charge:
                SpawnProjectile(spawnPos, direction, KenneyAssets.ChargeProjectile, 18f + power * 4f, 2.2f, 2 + damageBoost + Mathf.RoundToInt(power * 3f),
                    Vector3.one * Mathf.Lerp(0.7f, 1.15f, power), new Color(0.72f, 0.84f, 1f), new Color(0.2f, 0.4f, 1f),
                    new Color(0.75f, 0.85f, 1f), 2 + Mathf.RoundToInt(power * 2f), 0.35f + power * 0.25f);
                cooldownTimer = 0.55f * cooldownBoostMultiplier;
                break;
            case WeaponType.Piercer:
                SpawnProjectile(spawnPos, direction, KenneyAssets.PierceProjectile, 18f, 2.4f, 2 + damageBoost,
                    new Vector3(0.68f, 0.68f, 1f), new Color(0.72f, 1f, 0.42f), new Color(0.2f, 0.8f, 0.2f),
                    new Color(0.75f, 1f, 0.35f), 5, 0f);
                cooldownTimer = 0.18f * cooldownBoostMultiplier;
                break;
            default:
                SpawnProjectile(spawnPos, direction, KenneyAssets.Laser, pulseSpeed, projectileLifetime, 1 + damageBoost,
                    new Vector3(0.55f, 0.55f, 1f), new Color(0.3f, 0.7f, 1f), new Color(0.1f, 0.4f, 1f),
                    new Color(0.2f, 0.8f, 1f), 1, 0f);
                cooldownTimer = pulseCooldown * cooldownBoostMultiplier;
                break;
        }

        if (powerUps != null && powerUps.ExtraShotPairs > 0 && currentWeapon != WeaponType.Charge)
        {
            for (int i = 0; i < powerUps.ExtraShotPairs; i++)
            {
                float angleOffset = 10f + i * 4f;
                SpawnProjectile(spawnPos, Rotate(direction, angleOffset), KenneyAssets.Laser, pulseSpeed, projectileLifetime, 1 + damageBoost,
                    new Vector3(0.48f, 0.48f, 1f), new Color(1f, 0.72f, 0.32f), new Color(1f, 0.38f, 0.18f),
                    new Color(1f, 0.68f, 0.22f), 1, 0f);
                SpawnProjectile(spawnPos, Rotate(direction, -angleOffset), KenneyAssets.Laser, pulseSpeed, projectileLifetime, 1 + damageBoost,
                    new Vector3(0.48f, 0.48f, 1f), new Color(1f, 0.72f, 0.32f), new Color(1f, 0.38f, 0.18f),
                    new Color(1f, 0.68f, 0.22f), 1, 0f);
            }
        }

        GameAudio.Instance?.PlayPlayerShoot();
    }

    void SpawnProjectile(Vector3 spawnPos, Vector2 direction, Sprite sprite, float speed, float lifetime, int damage,
        Vector3 scale, Color trailStartColor, Color trailEndColor, Color impactColor, int penetration, float explosionRadius)
    {
        GameObject projectileObject = new GameObject("Projectile");
        projectileObject.tag = "Projectile";
        projectileObject.transform.position = spawnPos;
        projectileObject.transform.localScale = scale;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        projectileObject.transform.eulerAngles = new Vector3(0f, 0f, angle);

        SpriteRenderer spriteRenderer = projectileObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = Color.white;
        spriteRenderer.sortingOrder = 4;

        BoxCollider2D collider = projectileObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.25f, 0.8f);

        Rigidbody2D rigidbody = projectileObject.AddComponent<Rigidbody2D>();
        rigidbody.bodyType = RigidbodyType2D.Kinematic;
        rigidbody.gravityScale = 0f;

        TrailRenderer trail = projectileObject.AddComponent<TrailRenderer>();
        trail.time = 0.18f;
        trail.startWidth = 0.18f;
        trail.endWidth = 0f;
        trail.startColor = trailStartColor;
        trail.endColor = new Color(trailEndColor.r, trailEndColor.g, trailEndColor.b, 0f);
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.sortingOrder = 3;
        trail.numCapVertices = 2;
        trail.minVertexDistance = 0.05f;

        Projectile projectile = projectileObject.AddComponent<Projectile>();
        projectile.speed = speed;
        projectile.lifetime = lifetime;
        projectile.direction = direction;
        projectile.damage = damage;
        projectile.owner = Projectile.ProjectileOwner.Player;
        projectile.impactColor = impactColor;
        projectile.penetration = penetration;
        projectile.explosionRadius = explosionRadius;
    }

    static Vector2 Rotate(Vector2 vector, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);
        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos).normalized;
    }
}
