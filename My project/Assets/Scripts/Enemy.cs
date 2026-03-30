using UnityEngine;

/// <summary>
/// Enemy ship with multiple archetypes: standard, kamikaze, turret, shielded, and sniper.
/// </summary>
public class Enemy : MonoBehaviour
{
    public enum EnemyType
    {
        Standard,
        Kamikaze,
        Turret,
        Shielded,
        Sniper,
    }

    public EnemyType enemyType;
    public int damageToPlayer = 1;
    public int projectileDamage = 1;
    public int scoreValue = 2;
    public int scrapReward = 3;
    public float fireIntervalMin = 1.1f;
    public float fireIntervalMax = 2.1f;
    public float projectileSpeed = 8.5f;
    public float projectileLifetime = 2.8f;
    public float fireRange = 12f;

    float fireTimer;
    float hoverSeed;
    float diveSpeed;
    bool destroyed;
    bool isDetached;
    int health = 1;
    PlayerController player;
    Transform enemyVisual;
    Transform dangerGlow;
    Vector3 baseLocalPosition;
    Sprite assignedSprite;

    public void Configure(EnemyType type, Sprite sprite)
    {
        enemyType = type;
        assignedSprite = sprite;
    }

    void Start()
    {
        hoverSeed = Random.Range(0f, 100f);
        enemyVisual = transform.Find("EnemyVisual");
        dangerGlow = transform.Find("DangerGlow");
        baseLocalPosition = transform.localPosition;
        ApplyArchetypeSettings();
        ApplyVisuals();
        ResetFireTimer();
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameplayActive)
            return;

        if (player == null)
            player = FindAnyObjectByType<PlayerController>();

        HandleMovement();
        AnimateVisuals();
        AimAtPlayer();
        HandleFiring();
    }

    void ApplyArchetypeSettings()
    {
        switch (enemyType)
        {
            case EnemyType.Kamikaze:
                damageToPlayer = 2;
                scoreValue = 3;
                scrapReward = 5;
                fireRange = 0f;
                health = 1;
                diveSpeed = 5.8f;
                break;
            case EnemyType.Turret:
                scoreValue = 4;
                scrapReward = 5;
                fireIntervalMin = 1.6f;
                fireIntervalMax = 2.4f;
                projectileSpeed = 9.4f;
                fireRange = 14f;
                health = 2;
                break;
            case EnemyType.Shielded:
                scoreValue = 5;
                scrapReward = 6;
                fireIntervalMin = 1.0f;
                fireIntervalMax = 1.8f;
                health = 3;
                break;
            case EnemyType.Sniper:
                scoreValue = 4;
                scrapReward = 5;
                fireIntervalMin = 1.9f;
                fireIntervalMax = 2.9f;
                projectileSpeed = 12.5f;
                projectileLifetime = 3.6f;
                fireRange = 18f;
                health = 1;
                break;
            default:
                health = 1;
                break;
        }
    }

    void ApplyVisuals()
    {
        if (enemyVisual != null)
        {
            SpriteRenderer renderer = enemyVisual.GetComponent<SpriteRenderer>();
            if (renderer != null && assignedSprite != null)
                renderer.sprite = assignedSprite;

            if (enemyType == EnemyType.Kamikaze)
                enemyVisual.localScale = new Vector3(0.86f, 0.86f, 1f);
        }

        if (dangerGlow != null)
        {
            SpriteRenderer glowRenderer = dangerGlow.GetComponent<SpriteRenderer>();
            if (glowRenderer != null)
            {
                glowRenderer.color = enemyType switch
                {
                    EnemyType.Kamikaze => new Color(1f, 0.25f, 0.2f, 0.24f),
                    EnemyType.Turret => new Color(0.45f, 0.8f, 1f, 0.18f),
                    EnemyType.Shielded => new Color(0.25f, 1f, 0.45f, 0.2f),
                    EnemyType.Sniper => new Color(1f, 0.82f, 0.35f, 0.18f),
                    _ => new Color(1f, 0.15f, 0.15f, 0.18f),
                };
            }
        }
    }

    void HandleMovement()
    {
        if (enemyType == EnemyType.Kamikaze)
        {
            HandleKamikazeMovement();
            return;
        }

        GameManager gameManager = GameManager.Instance;
        if (gameManager == null || !gameManager.EnemiesCanStrafe || enemyType == EnemyType.Turret)
            return;

        float amplitude = gameManager.GetEnemyStrafeAmplitude();
        float speed = gameManager.GetEnemyStrafeSpeed();

        Vector3 position = transform.localPosition;
        position.y = baseLocalPosition.y + Mathf.Sin(Time.time * speed + hoverSeed) * amplitude;
        transform.localPosition = position;
    }

    void HandleKamikazeMovement()
    {
        if (player == null)
            return;

        if (!isDetached)
        {
            transform.SetParent(null, true);
            isDetached = true;
        }

        Vector3 target = player.transform.position;
        target.x -= 0.35f;
        transform.position = Vector3.MoveTowards(transform.position, target, diveSpeed * Time.deltaTime);
        transform.position += Vector3.left * (GameManager.Instance != null ? GameManager.Instance.ScrollSpeed * 0.12f * Time.deltaTime : 0f);
    }

    void AnimateVisuals()
    {
        if (enemyVisual != null)
        {
            float hover = Mathf.Sin(Time.time * 5f + hoverSeed) * 0.08f;
            enemyVisual.localPosition = new Vector3(0f, hover, 0f);
        }

        if (dangerGlow != null)
        {
            float pulse = 1.25f + Mathf.Sin(Time.time * 6f + hoverSeed) * 0.18f;
            if (enemyType == EnemyType.Shielded)
                pulse += 0.12f;
            dangerGlow.localScale = Vector3.one * pulse;
        }
    }

    void AimAtPlayer()
    {
        if (enemyVisual == null)
            return;

        Vector2 targetDirection = Vector2.left;
        if (player != null)
        {
            Vector2 toPlayer = player.transform.position - enemyVisual.position;
            if (toPlayer.sqrMagnitude > 0.001f)
                targetDirection = toPlayer.normalized;
        }

        float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg + 90f;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
        enemyVisual.rotation = Quaternion.RotateTowards(enemyVisual.rotation, targetRotation, 540f * Time.deltaTime);
    }

    void HandleFiring()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null && !gameManager.EnemiesCanShoot)
            return;
        if (enemyType == EnemyType.Kamikaze || player == null || destroyed)
            return;

        fireTimer -= Time.deltaTime;
        if (fireTimer > 0f)
            return;

        Vector2 toPlayer = player.transform.position - transform.position;
        if (toPlayer.sqrMagnitude > fireRange * fireRange)
        {
            fireTimer = 0.2f;
            return;
        }

        if (transform.position.x <= player.transform.position.x + 0.35f && enemyType != EnemyType.Turret)
        {
            fireTimer = 0.2f;
            return;
        }

        FireAtPlayer(toPlayer.normalized);
        ResetFireTimer();
    }

    void FireAtPlayer(Vector2 direction)
    {
        if (enemyType == EnemyType.Turret)
        {
            FireProjectile(direction);
            FireProjectile(Rotate(direction, 12f));
            FireProjectile(Rotate(direction, -12f));
            return;
        }

        if (enemyType == EnemyType.Sniper)
        {
            FireProjectile(direction, 1.25f, 2);
            return;
        }

        FireProjectile(direction);
    }

    void FireProjectile(Vector2 direction, float speedScale = 1f, int enemyPenetration = 1)
    {
        float speedMultiplier = GameManager.Instance != null
            ? GameManager.Instance.GetEnemyProjectileSpeedMultiplier()
            : 1f;

        GameObject projectileObject = new GameObject("EnemyProjectile");
        projectileObject.transform.position = transform.position + (Vector3)(direction * 0.7f);
        projectileObject.transform.localScale = enemyType == EnemyType.Sniper ? new Vector3(0.62f, 0.62f, 1f) : new Vector3(0.5f, 0.5f, 1f);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        projectileObject.transform.eulerAngles = new Vector3(0f, 0f, angle);

        SpriteRenderer renderer = projectileObject.AddComponent<SpriteRenderer>();
        renderer.sprite = enemyType == EnemyType.Sniper ? KenneyAssets.SnipedProjectile : KenneyAssets.EnemyLaser;
        renderer.color = Color.white;
        renderer.sortingOrder = 4;

        BoxCollider2D collider = projectileObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.25f, 0.7f);

        Rigidbody2D rigidbody = projectileObject.AddComponent<Rigidbody2D>();
        rigidbody.bodyType = RigidbodyType2D.Kinematic;
        rigidbody.gravityScale = 0f;

        TrailRenderer trail = projectileObject.AddComponent<TrailRenderer>();
        trail.time = 0.14f;
        trail.startWidth = 0.12f;
        trail.endWidth = 0f;
        trail.startColor = enemyType == EnemyType.Sniper ? new Color(1f, 0.9f, 0.4f, 0.95f) : new Color(1f, 0.45f, 0.35f, 0.9f);
        trail.endColor = enemyType == EnemyType.Sniper ? new Color(1f, 0.55f, 0.1f, 0f) : new Color(1f, 0.1f, 0.1f, 0f);
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.sortingOrder = 3;
        trail.numCapVertices = 2;
        trail.minVertexDistance = 0.05f;

        Projectile projectile = projectileObject.AddComponent<Projectile>();
        projectile.owner = Projectile.ProjectileOwner.Enemy;
        projectile.direction = direction;
        projectile.speed = projectileSpeed * speedMultiplier * speedScale;
        projectile.lifetime = projectileLifetime;
        projectile.damage = projectileDamage;
        projectile.penetration = enemyPenetration;
        projectile.impactColor = enemyType == EnemyType.Sniper ? new Color(1f, 0.8f, 0.2f) : new Color(1f, 0.45f, 0.35f);

        GameAudio.Instance?.PlayEnemyShoot();
    }

    public void TakeProjectileHit(int damage)
    {
        if (destroyed)
            return;

        health -= Mathf.Max(1, damage);
        VFXHelper.SpawnImpact(transform.position, enemyType == EnemyType.Shielded ? new Color(0.4f, 1f, 0.55f) : new Color(1f, 0.7f, 0.25f));

        if (health > 0)
            return;

        destroyed = true;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
            GameManager.Instance.RegisterEnemyDestroyed(scrapReward);
        }
        VFXHelper.SpawnExplosion(transform.position, new Color(1f, 0.6f, 0.1f), 10, 3.5f);
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (destroyed)
            return;

        if (!other.CompareTag("Player"))
            return;

        PlayerController playerController = other.GetComponent<PlayerController>();
        if (playerController != null)
            playerController.TakeDamage(damageToPlayer);

        destroyed = true;
        VFXHelper.SpawnExplosion(transform.position, new Color(1f, 0.3f, 0.3f), 6, 2.5f);
        Destroy(gameObject);
    }

    void ResetFireTimer()
    {
        float fireMultiplier = GameManager.Instance != null
            ? GameManager.Instance.GetEnemyFireIntervalMultiplier()
            : 1f;

        fireTimer = Random.Range(fireIntervalMin * fireMultiplier, fireIntervalMax * fireMultiplier);
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
