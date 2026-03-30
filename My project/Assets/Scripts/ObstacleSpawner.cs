using UnityEngine;

/// <summary>
/// Spawns space obstacles with gaps, health packs, and enemies at intervals.
/// The blockers are rendered as asteroid barriers instead of flat corridor walls.
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public float spawnInterval = 2.5f;
    public float gapSize = 3.5f;
    public float minGapY = -3f;
    public float maxGapY = 3f;
    public float wallWidth = 1.5f;
    public float wallHeight = 15f;
    public float spawnX = 12f;

    [Header("Item Spawn Chances")]
    [Range(0, 1)] public float healthPackChance = 0.35f;
    [Range(0, 1)] public float powerUpChance = 0.3f;
    [Range(0, 1)] public float enemyChance = 0.5f;
    public int maxEnemiesPerObstacle = 2;

    private float timer;

    void Start()
    {
        timer = 1f;
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.AllowsWorldSpawns) return;

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            SpawnObstacle();
            timer = GetSpawnInterval();
        }
    }

    void SpawnObstacle()
    {
        float activeGapSize = GetGapSize();
        float gapCenter = Random.Range(minGapY, maxGapY);

        GameObject obstacle = new GameObject("Obstacle");
        obstacle.transform.position = new Vector3(spawnX, 0, 0);
        ScrollingObject scroll = obstacle.AddComponent<ScrollingObject>();
        scroll.destroyX = -15f;

        float topY = gapCenter + activeGapSize * 0.5f + wallHeight * 0.5f;
        CreateWall(obstacle.transform, topY, "TopWall");

        float bottomY = gapCenter - activeGapSize * 0.5f - wallHeight * 0.5f;
        CreateWall(obstacle.transform, bottomY, "BottomWall");

        GameObject scoreTrigger = new GameObject("ScoreTrigger");
        scoreTrigger.tag = "ScoreTrigger";
        scoreTrigger.transform.SetParent(obstacle.transform);
        scoreTrigger.transform.localPosition = new Vector3(0, gapCenter, 0);
        BoxCollider2D scoreCol = scoreTrigger.AddComponent<BoxCollider2D>();
        scoreCol.isTrigger = true;
        scoreCol.size = new Vector2(0.5f, activeGapSize);
        scoreTrigger.AddComponent<ScoreTrigger>();

        if (Random.value < GetHealthPackChance())
        {
            float packY = gapCenter + Random.Range(-activeGapSize * 0.2f, activeGapSize * 0.2f);
            CreateHealthPack(obstacle.transform, packY);
        }

        if (Random.value < GetPowerUpChance())
        {
            float pickupY = gapCenter + Random.Range(-activeGapSize * 0.22f, activeGapSize * 0.22f);
            CreatePowerUp(obstacle.transform, pickupY);
        }

        if (Random.value < GetEnemyChance())
        {
            int maxCount = GetMaxEnemiesPerObstacle();
            int count = Random.Range(1, maxCount + 1);
            for (int i = 0; i < count; i++)
            {
                float enemyY = gapCenter + Random.Range(-activeGapSize * 0.3f, activeGapSize * 0.3f);
                float enemyX = Random.Range(-0.5f, 0.5f);
                CreateEnemy(obstacle.transform, enemyX, enemyY, PickEnemyType());
            }
        }
    }

    void CreateWall(Transform parent, float yPos, string name)
    {
        GameObject wall = new GameObject(name);
        wall.tag = "Obstacle";
        wall.transform.SetParent(parent);
        wall.transform.localPosition = new Vector3(0, yPos, 0);
        wall.transform.localScale = Vector3.one;

        BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(wallWidth, wallHeight);

        wall.AddComponent<ObstacleWall>();
    }

    void CreateHealthPack(Transform parent, float yPos)
    {
        GameObject pack = new GameObject("HealthPack");
        pack.tag = "HealthPack";
        pack.transform.SetParent(parent);
        pack.transform.localPosition = new Vector3(0.5f, yPos, 0);
        pack.transform.localScale = new Vector3(0.72f, 0.72f, 1);

        GameObject glow = new GameObject("Glow");
        glow.transform.SetParent(pack.transform, false);
        glow.transform.localScale = Vector3.one * 1.32f;
        SpriteRenderer glowSr = glow.AddComponent<SpriteRenderer>();
        glowSr.sprite = SpriteHelper.Circle;
        glowSr.color = new Color(0.28f, 1f, 0.46f, 0.1f);
        glowSr.sortingOrder = 2;

        SpriteRenderer sr = pack.AddComponent<SpriteRenderer>();
        sr.sprite = KenneyAssets.HealthBadge;
        sr.color = Color.white;
        sr.sortingOrder = 3;

        CreatePickupOverlay(pack.transform, "Icon", KenneyAssets.HealthIcon, Vector3.zero,
            new Vector3(0.54f, 0.54f, 1f), 4, Color.white);

        CircleCollider2D col = pack.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.42f;

        pack.AddComponent<HealthPack>();
    }

    void CreatePowerUp(Transform parent, float yPos)
    {
        GameObject pickup = new GameObject("PowerUp");
        pickup.transform.SetParent(parent);
        pickup.transform.localPosition = new Vector3(-0.55f, yPos, 0);
        pickup.transform.localScale = new Vector3(0.7f, 0.7f, 1f);

        GameObject glow = new GameObject("Glow");
        glow.transform.SetParent(pickup.transform, false);
        glow.transform.localScale = Vector3.one * 1.26f;
        SpriteRenderer glowSr = glow.AddComponent<SpriteRenderer>();
        glowSr.sprite = SpriteHelper.Circle;
        glowSr.color = new Color(1f, 1f, 1f, 0.09f);
        glowSr.sortingOrder = 2;

        SpriteRenderer sr = pickup.AddComponent<SpriteRenderer>();
        sr.sprite = KenneyAssets.RapidFireBadge;
        sr.color = Color.white;
        sr.sortingOrder = 3;

        CircleCollider2D col = pickup.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.48f;

        PowerUpPickup powerUpPickup = pickup.AddComponent<PowerUpPickup>();
        powerUpPickup.powerUpType = PickRandomPowerUpType();
        powerUpPickup.RefreshVisuals();
    }

    void CreatePickupOverlay(Transform parent, string name, Sprite sprite, Vector3 localPosition, Vector3 localScale, int sortingOrder, Color color)
    {
        GameObject overlay = new GameObject(name);
        overlay.transform.SetParent(parent, false);
        overlay.transform.localPosition = localPosition;
        overlay.transform.localScale = localScale;

        SpriteRenderer renderer = overlay.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
    }

    void CreateEnemy(Transform parent, float xOff, float yPos, Enemy.EnemyType enemyType)
    {
        GameObject enemy = new GameObject("Enemy");
        enemy.tag = "Enemy";
        enemy.transform.SetParent(parent);
        enemy.transform.localPosition = new Vector3(xOff, yPos, 0);

        GameObject glow = new GameObject("DangerGlow");
        glow.transform.SetParent(enemy.transform, false);
        glow.transform.localScale = Vector3.one * 1.4f;
        SpriteRenderer glowSr = glow.AddComponent<SpriteRenderer>();
        glowSr.sprite = SpriteHelper.Circle;
        glowSr.color = new Color(1f, 0.15f, 0.15f, 0.18f);
        glowSr.sortingOrder = 2;

        GameObject enemyVisual = new GameObject("EnemyVisual");
        enemyVisual.transform.SetParent(enemy.transform, false);
        enemyVisual.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
        enemyVisual.transform.localEulerAngles = new Vector3(0f, 0f, -90f);

        SpriteRenderer sr = enemyVisual.AddComponent<SpriteRenderer>();
        sr.sprite = KenneyAssets.GetEnemySprite(enemyType);
        sr.color = Color.white;
        sr.sortingOrder = 3;

        BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.6f, 0.5f);

        Enemy enemyComponent = enemy.AddComponent<Enemy>();
        enemyComponent.Configure(enemyType, sr.sprite);
    }

    float GetSpawnInterval()
    {
        return GameManager.Instance != null
            ? GameManager.Instance.GetSpawnInterval(spawnInterval)
            : spawnInterval;
    }

    float GetGapSize()
    {
        return GameManager.Instance != null
            ? GameManager.Instance.GetGapSize(gapSize)
            : gapSize;
    }

    float GetHealthPackChance()
    {
        return GameManager.Instance != null
            ? GameManager.Instance.GetHealthPackChance(healthPackChance)
            : healthPackChance;
    }

    float GetEnemyChance()
    {
        return GameManager.Instance != null
            ? GameManager.Instance.GetEnemyChance(enemyChance)
            : enemyChance;
    }

    float GetPowerUpChance()
    {
        return GameManager.Instance != null
            ? GameManager.Instance.GetPowerUpChance(powerUpChance)
            : powerUpChance;
    }

    int GetMaxEnemiesPerObstacle()
    {
        return GameManager.Instance != null
            ? GameManager.Instance.GetMaxEnemiesPerObstacle(maxEnemiesPerObstacle)
            : maxEnemiesPerObstacle;
    }

    PlayerPowerUps.PowerUpType PickRandomPowerUpType()
    {
        GameManager.GameDifficulty difficulty = GameManager.Instance != null
            ? GameManager.Instance.SelectedDifficulty
            : GameManager.GameDifficulty.Medium;

        float roll = Random.value;
        return difficulty switch
        {
            GameManager.GameDifficulty.Easy => roll < 0.45f
                ? PlayerPowerUps.PowerUpType.Shield
                : roll < 0.75f
                    ? PlayerPowerUps.PowerUpType.RapidFire
                    : PlayerPowerUps.PowerUpType.SpreadShot,
            GameManager.GameDifficulty.Hard => roll < 0.25f
                ? PlayerPowerUps.PowerUpType.Shield
                : roll < 0.62f
                    ? PlayerPowerUps.PowerUpType.RapidFire
                    : PlayerPowerUps.PowerUpType.SpreadShot,
            _ => roll < 0.34f
                ? PlayerPowerUps.PowerUpType.Shield
                : roll < 0.67f
                    ? PlayerPowerUps.PowerUpType.RapidFire
                    : PlayerPowerUps.PowerUpType.SpreadShot,
        };
    }

    Enemy.EnemyType PickEnemyType()
    {
        GameManager gameManager = GameManager.Instance;
        float roll = Random.value;

        if (gameManager != null && gameManager.SelectedMode == GameManager.GameMode.Challenge)
        {
            if (roll < 0.24f) return Enemy.EnemyType.Kamikaze;
            if (roll < 0.48f) return Enemy.EnemyType.Shielded;
            if (roll < 0.72f) return Enemy.EnemyType.Turret;
            if (roll < 0.9f) return Enemy.EnemyType.Sniper;
            return Enemy.EnemyType.Standard;
        }

        if (gameManager != null && gameManager.CurrentStage == GameManager.RunStage.Level3)
        {
            if (roll < 0.2f) return Enemy.EnemyType.Kamikaze;
            if (roll < 0.42f) return Enemy.EnemyType.Shielded;
            if (roll < 0.62f) return Enemy.EnemyType.Turret;
            if (roll < 0.78f) return Enemy.EnemyType.Sniper;
            return Enemy.EnemyType.Standard;
        }

        if (gameManager != null && gameManager.CurrentStage == GameManager.RunStage.Level2)
        {
            if (roll < 0.18f) return Enemy.EnemyType.Kamikaze;
            if (roll < 0.36f) return Enemy.EnemyType.Turret;
            if (roll < 0.52f) return Enemy.EnemyType.Shielded;
            if (roll < 0.64f) return Enemy.EnemyType.Sniper;
            return Enemy.EnemyType.Standard;
        }

        if (roll < 0.18f) return Enemy.EnemyType.Kamikaze;
        if (roll < 0.3f) return Enemy.EnemyType.Turret;
        return Enemy.EnemyType.Standard;
    }
}
