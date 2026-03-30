using UnityEngine;

/// <summary>
/// Multi-phase boss with weak point and alternating boss variants.
/// </summary>
public class BossEnemy : MonoBehaviour
{
    public enum BossType
    {
        DreadCarrier,
        AegisSwarm,
    }

    public BossType bossType = BossType.DreadCarrier;
    public int bossTier = 1;

    int maxHealth;
    int currentHealth;
    int contactDamage;
    int rewardScore;
    int baseShotCount;
    float fireInterval;
    float projectileSpeed;
    float projectileLifetime;
    float moveAmplitude;
    float moveFrequency;
    float entryTargetX;
    float entrySpeed;
    float hoverSeed;
    float fireTimer;
    int volleyIndex;
    bool defeated;
    bool initialized;

    PlayerController player;
    Transform bossVisual;
    Transform bossGlow;
    Transform weakPoint;
    SpriteRenderer coreRenderer;
    SpriteRenderer wingTopRenderer;
    SpriteRenderer wingBottomRenderer;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public float HealthRatio => maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
    public string BossDisplayName => GetDisplayName(bossType);

    public static string GetDisplayName(BossType type)
    {
        return type switch
        {
            BossType.AegisSwarm => "AEGIS SWARM",
            _ => "DREAD CARRIER",
        };
    }

    void Start()
    {
        InitializeBoss();
        GameManager.Instance?.RegisterBoss(this);
    }

    void OnDestroy()
    {
        GameManager.Instance?.UnregisterBoss(this);
    }

    void Update()
    {
        if (!initialized)
            InitializeBoss();

        if (GameManager.Instance != null && !GameManager.Instance.IsGameplayActive)
            return;

        if (player == null)
            player = FindAnyObjectByType<PlayerController>();

        HandleMovement();
        AnimateVisuals();
        HandleFiring();
    }

    public void TakeProjectileHit(int damage)
    {
        if (defeated)
            return;

        int reducedDamage = Mathf.Max(1, Mathf.FloorToInt(damage * 0.55f));
        ApplyDamage(reducedDamage, new Color(1f, 0.55f, 0.18f));
    }

    public void TakeWeakPointHit(int damage)
    {
        if (defeated)
            return;

        ApplyDamage(Mathf.Max(1, damage), new Color(1f, 0.96f, 0.45f));
    }

    void InitializeBoss()
    {
        if (initialized)
            return;

        ApplyDifficultyTuning();
        BuildVisuals();
        hoverSeed = Random.Range(0f, 100f);
        fireTimer = fireInterval;
        initialized = true;
    }

    void ApplyDamage(int damage, Color impactColor)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        VFXHelper.SpawnImpact(transform.position + new Vector3(Random.Range(-0.45f, 0.45f), Random.Range(-0.55f, 0.55f), 0f), impactColor);

        if (currentHealth <= 0)
            DefeatBoss();
    }

    void HandleMovement()
    {
        Vector3 position = transform.position;
        float phaseBoost = GetPhaseSpeedMultiplier();
        if (position.x > entryTargetX)
        {
            position.x = Mathf.MoveTowards(position.x, entryTargetX, entrySpeed * phaseBoost * Time.deltaTime);
        }
        else
        {
            position.x = entryTargetX + Mathf.Sin(Time.time * 0.7f + hoverSeed) * 0.25f * phaseBoost;
            position.y = Mathf.Sin(Time.time * moveFrequency * phaseBoost + hoverSeed) * moveAmplitude * phaseBoost;
        }

        transform.position = position;
    }

    void AnimateVisuals()
    {
        if (bossGlow != null)
        {
            float pulse = 2.2f + Mathf.Sin(Time.time * 4.6f + hoverSeed) * 0.18f;
            bossGlow.localScale = Vector3.one * pulse;
        }

        if (bossVisual != null)
            bossVisual.localPosition = new Vector3(Mathf.Sin(Time.time * 2.2f + hoverSeed) * 0.04f, 0f, 0f);

        if (weakPoint != null)
        {
            float yOffset = Mathf.Sin(Time.time * 2.6f + hoverSeed) * 0.42f;
            weakPoint.localPosition = new Vector3(-0.75f, yOffset, 0f);
        }

        if (coreRenderer != null)
            coreRenderer.color = HealthRatio < 0.34f ? new Color(1f, 0.85f, 0.72f) : Color.white;
    }

    void HandleFiring()
    {
        if (player == null || defeated || transform.position.x > entryTargetX + 0.1f)
            return;

        fireTimer -= Time.deltaTime;
        if (fireTimer > 0f)
            return;

        Vector2 baseDirection = player.transform.position - transform.position;
        if (baseDirection.sqrMagnitude <= 0.001f)
            baseDirection = Vector2.left;
        baseDirection.Normalize();

        if (bossType == BossType.DreadCarrier)
            FireCarrierPattern(baseDirection);
        else
            FireSwarmPattern(baseDirection);

        volleyIndex++;
        fireTimer = Mathf.Max(0.35f, fireInterval * (HealthRatio < 0.35f ? 0.78f : HealthRatio < 0.7f ? 0.9f : 1f));
    }

    void FireCarrierPattern(Vector2 baseDirection)
    {
        int shotCount = GetCurrentShotCount();
        float spread = shotCount >= 7 ? 58f : shotCount >= 5 ? 42f : 24f;
        FireSpread(baseDirection, shotCount, spread, 1f, 1);

        if (HealthRatio < 0.7f)
        {
            FireSpread(Rotate(baseDirection, 16f), 3, 18f, 0.86f, 1);
            FireSpread(Rotate(baseDirection, -16f), 3, 18f, 0.86f, 1);
        }

        if (HealthRatio < 0.35f && volleyIndex % 2 == 0)
            FireSpread(baseDirection, 1, 0f, 1.45f, 2);
    }

    void FireSwarmPattern(Vector2 baseDirection)
    {
        float spinOffset = volleyIndex * 9f;
        FireSpread(Rotate(baseDirection, spinOffset), 5, 44f, 1f, 1);
        FireSpread(Rotate(baseDirection, -spinOffset), 5, 44f, 1f, 1);

        if (HealthRatio < 0.68f)
            FireSpread(baseDirection, 3, 18f, 1.28f, 2);

        if (HealthRatio < 0.36f)
        {
            FireSpread(Vector2.left, 6, 150f, 0.9f, 1);
            FireSpread(Vector2.left, 1, 0f, 1.55f, 3);
        }
    }

    void FireSpread(Vector2 baseDirection, int shotCount, float spreadAngle, float speedScale, int penetration)
    {
        if (shotCount <= 1)
        {
            FireProjectile(baseDirection, speedScale, penetration);
            return;
        }

        for (int i = 0; i < shotCount; i++)
        {
            float t = i / (float)(shotCount - 1);
            float angleOffset = Mathf.Lerp(-spreadAngle * 0.5f, spreadAngle * 0.5f, t);
            FireProjectile(Rotate(baseDirection, angleOffset), speedScale, penetration);
        }
    }

    void FireProjectile(Vector2 direction, float speedScale, int penetration)
    {
        GameObject projectileObject = new GameObject("BossProjectile");
        projectileObject.transform.position = transform.position + (Vector3)(direction * 1.1f);
        projectileObject.transform.localScale = new Vector3(0.65f, 0.65f, 1f);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        projectileObject.transform.eulerAngles = new Vector3(0f, 0f, angle);

        SpriteRenderer renderer = projectileObject.AddComponent<SpriteRenderer>();
        renderer.sprite = bossType == BossType.AegisSwarm ? KenneyAssets.SnipedProjectile : KenneyAssets.EnemyLaser;
        renderer.color = Color.white;
        renderer.sortingOrder = 5;

        BoxCollider2D collider = projectileObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.28f, 0.8f);

        Rigidbody2D rigidbody = projectileObject.AddComponent<Rigidbody2D>();
        rigidbody.bodyType = RigidbodyType2D.Kinematic;
        rigidbody.gravityScale = 0f;

        TrailRenderer trail = projectileObject.AddComponent<TrailRenderer>();
        trail.time = 0.22f;
        trail.startWidth = 0.18f;
        trail.endWidth = 0f;
        trail.startColor = bossType == BossType.AegisSwarm ? new Color(1f, 0.92f, 0.38f, 0.95f) : new Color(1f, 0.55f, 0.35f, 0.95f);
        trail.endColor = bossType == BossType.AegisSwarm ? new Color(1f, 0.45f, 0.1f, 0f) : new Color(1f, 0.15f, 0.15f, 0f);
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.sortingOrder = 4;
        trail.numCapVertices = 2;
        trail.minVertexDistance = 0.05f;

        Projectile projectile = projectileObject.AddComponent<Projectile>();
        projectile.owner = Projectile.ProjectileOwner.Enemy;
        projectile.direction = direction;
        projectile.speed = projectileSpeed * speedScale;
        projectile.lifetime = projectileLifetime;
        projectile.damage = HealthRatio < 0.35f ? 2 : 1;
        projectile.penetration = penetration;
        projectile.impactColor = bossType == BossType.AegisSwarm ? new Color(1f, 0.82f, 0.22f) : new Color(1f, 0.5f, 0.28f);

        GameAudio.Instance?.PlayEnemyShoot();
    }

    void ApplyDifficultyTuning()
    {
        GameManager.GameDifficulty difficulty = GameManager.Instance != null
            ? GameManager.Instance.SelectedDifficulty
            : GameManager.GameDifficulty.Medium;

        float tierBoost = Mathf.Max(0f, bossTier - 1) * 0.12f;
        switch (difficulty)
        {
            case GameManager.GameDifficulty.Easy:
                maxHealth = Mathf.RoundToInt(36 * (1f + tierBoost));
                contactDamage = 1;
                rewardScore = 18 + bossTier * 2;
                baseShotCount = 3;
                fireInterval = 1.55f;
                projectileSpeed = 8.4f;
                projectileLifetime = 3.2f;
                moveAmplitude = 2.0f;
                moveFrequency = 1.2f;
                entryTargetX = 5.8f;
                entrySpeed = 2.8f;
                break;
            case GameManager.GameDifficulty.Hard:
                maxHealth = Mathf.RoundToInt(70 * (1f + tierBoost));
                contactDamage = 2;
                rewardScore = 30 + bossTier * 3;
                baseShotCount = 5;
                fireInterval = 0.92f;
                projectileSpeed = 11.2f;
                projectileLifetime = 3.4f;
                moveAmplitude = 3.05f;
                moveFrequency = 1.95f;
                entryTargetX = 5.95f;
                entrySpeed = 3.8f;
                break;
            default:
                maxHealth = Mathf.RoundToInt(52 * (1f + tierBoost));
                contactDamage = 1;
                rewardScore = 24 + bossTier * 3;
                baseShotCount = 5;
                fireInterval = 1.16f;
                projectileSpeed = 9.8f;
                projectileLifetime = 3.3f;
                moveAmplitude = 2.55f;
                moveFrequency = 1.55f;
                entryTargetX = 5.9f;
                entrySpeed = 3.2f;
                break;
        }

        currentHealth = maxHealth;
    }

    void BuildVisuals()
    {
        bossGlow = CreateSpriteChild("BossGlow", SpriteHelper.Circle, new Vector3(-0.05f, 0f, 0f), Vector3.one * 2.25f, 2,
            bossType == BossType.AegisSwarm ? new Color(1f, 0.85f, 0.2f, 0.2f) : new Color(1f, 0.18f, 0.15f, 0.22f), 0f);

        Sprite coreSprite = bossType == BossType.AegisSwarm ? KenneyAssets.BossCoreAlt : KenneyAssets.BossCore;
        Sprite wingSprite = bossType == BossType.AegisSwarm ? KenneyAssets.BossWingAlt : KenneyAssets.BossWing;
        bossVisual = CreateSpriteChild("BossVisual", coreSprite, Vector3.zero, new Vector3(1.8f, 1.8f, 1f), 5, Color.white, -90f);
        Transform topWing = CreateSpriteChild("LeftWing", wingSprite, new Vector3(-0.3f, 0.9f, 0f), new Vector3(0.9f, 0.9f, 1f), 4, Color.white, -102f);
        Transform bottomWing = CreateSpriteChild("RightWing", wingSprite, new Vector3(-0.3f, -0.9f, 0f), new Vector3(0.9f, 0.9f, 1f), 4, Color.white, -78f);
        CreateSpriteChild("CoreLight", SpriteHelper.Circle, new Vector3(0.25f, 0f, 0f), new Vector3(0.55f, 0.55f, 1f), 6,
            bossType == BossType.AegisSwarm ? new Color(1f, 0.88f, 0.25f, 0.34f) : new Color(1f, 0.75f, 0.22f, 0.3f), 0f);

        coreRenderer = bossVisual.GetComponent<SpriteRenderer>();
        wingTopRenderer = topWing.GetComponent<SpriteRenderer>();
        wingBottomRenderer = bottomWing.GetComponent<SpriteRenderer>();

        BuildWeakPoint();

        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider == null)
            collider = gameObject.AddComponent<BoxCollider2D>();

        collider.isTrigger = true;
        collider.size = new Vector2(1.65f, 1.45f);
    }

    void BuildWeakPoint()
    {
        weakPoint = transform.Find("WeakPoint");
        if (weakPoint == null)
        {
            weakPoint = new GameObject("WeakPoint").transform;
            weakPoint.SetParent(transform, false);
        }

        weakPoint.localPosition = new Vector3(-0.75f, 0f, 0f);
        weakPoint.localScale = Vector3.one * 0.55f;

        SpriteRenderer renderer = weakPoint.GetComponent<SpriteRenderer>();
        if (renderer == null)
            renderer = weakPoint.gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = bossType == BossType.AegisSwarm ? KenneyAssets.ChargeBadge : KenneyAssets.MissileBadge;
        renderer.color = Color.white;
        renderer.sortingOrder = 7;

        CircleCollider2D collider = weakPoint.GetComponent<CircleCollider2D>();
        if (collider == null)
            collider = weakPoint.gameObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.52f;

        BossWeakPoint weakPointComponent = weakPoint.GetComponent<BossWeakPoint>();
        if (weakPointComponent == null)
            weakPointComponent = weakPoint.gameObject.AddComponent<BossWeakPoint>();
        weakPointComponent.owner = this;
        weakPointComponent.damageMultiplier = bossType == BossType.AegisSwarm ? 2f : 1.8f;
    }

    Transform CreateSpriteChild(string name, Sprite sprite, Vector3 localPosition, Vector3 localScale, int sortingOrder, Color color, float rotationZ)
    {
        Transform child = transform.Find(name);
        if (child == null)
        {
            child = new GameObject(name).transform;
            child.SetParent(transform, false);
        }

        child.localPosition = localPosition;
        child.localScale = localScale;
        child.localEulerAngles = new Vector3(0f, 0f, rotationZ);

        SpriteRenderer renderer = child.GetComponent<SpriteRenderer>();
        if (renderer == null)
            renderer = child.gameObject.AddComponent<SpriteRenderer>();

        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return child;
    }

    int GetCurrentShotCount()
    {
        if (HealthRatio < 0.35f)
            return baseShotCount + 2;
        if (HealthRatio < 0.7f)
            return baseShotCount + 1;
        return baseShotCount;
    }

    float GetPhaseSpeedMultiplier()
    {
        if (HealthRatio < 0.35f)
            return 1.25f;
        if (HealthRatio < 0.7f)
            return 1.1f;
        return 1f;
    }

    void DefeatBoss()
    {
        if (defeated)
            return;

        defeated = true;
        VFXHelper.SpawnExplosion(transform.position, bossType == BossType.AegisSwarm ? new Color(1f, 0.82f, 0.22f) : new Color(1f, 0.55f, 0.15f), 18, 5.5f);
        GameManager.Instance?.NotifyBossDefeated(rewardScore);
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (defeated || !other.CompareTag("Player"))
            return;

        PlayerController playerController = other.GetComponent<PlayerController>();
        if (playerController != null)
            playerController.TakeDamage(contactDamage);
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
