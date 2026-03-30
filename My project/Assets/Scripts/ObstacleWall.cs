using UnityEngine;

/// <summary>
/// Damage obstacle that auto-renders itself as a drifting asteroid barrier.
/// This also upgrades old square wall objects already saved in the scene.
/// </summary>
public class ObstacleWall : MonoBehaviour
{
    public int damage = 1;

    BoxCollider2D wallCollider;
    Vector2 wallSize;
    bool visualsBuilt;

    void Awake()
    {
        NormalizeLegacyGeometry();
        EnsureAsteroidBarrierVisuals();
    }

    void NormalizeLegacyGeometry()
    {
        wallCollider = GetComponent<BoxCollider2D>();

        Vector3 localScale = transform.localScale;
        Vector2 scale = new Vector2(
            Mathf.Max(Mathf.Abs(localScale.x), 0.01f),
            Mathf.Max(Mathf.Abs(localScale.y), 0.01f));

        if (wallCollider != null)
        {
            Vector2 colliderSize = wallCollider.size;
            if (colliderSize.x <= 0.001f || colliderSize.y <= 0.001f)
                colliderSize = Vector2.one;

            wallSize = Vector2.Scale(colliderSize, scale);
            wallCollider.size = wallSize;
            wallCollider.offset = Vector2.zero;
        }
        else
        {
            wallSize = scale;
        }

        transform.localScale = Vector3.one;
    }

    void EnsureAsteroidBarrierVisuals()
    {
        if (visualsBuilt)
            return;

        Transform visuals = transform.Find("AsteroidFieldVisuals");
        if (visuals != null)
        {
            visualsBuilt = true;
            return;
        }

        DisableLegacyVisuals();

        visuals = new GameObject("AsteroidFieldVisuals").transform;
        visuals.SetParent(transform, false);

        CreateMeteorBarrier(visuals);
        CreateInnerEdgeHazard(visuals);

        visualsBuilt = true;
    }

    void DisableLegacyVisuals()
    {
        SpriteRenderer rootRenderer = GetComponent<SpriteRenderer>();
        if (rootRenderer != null)
            rootRenderer.enabled = false;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child != null)
                child.gameObject.SetActive(false);
        }
    }

    void CreateMeteorBarrier(Transform parent)
    {
        float halfWidth = wallSize.x * 0.5f;
        float halfHeight = wallSize.y * 0.5f;
        int rows = Mathf.Clamp(Mathf.RoundToInt(wallSize.y / 1.05f), 8, 18);

        for (int row = 0; row < rows; row++)
        {
            float t = rows <= 1 ? 0.5f : row / (rows - 1f);
            float baseY = Mathf.Lerp(-halfHeight + 0.42f, halfHeight - 0.42f, t);
            float primaryX = Random.Range(-halfWidth * 0.14f, halfWidth * 0.14f);
            float primaryScale = Random.Range(0.8f, 1.14f);
            CreateMeteorPiece(parent, new Vector3(primaryX, baseY, 0f), primaryScale, 1);

            bool spawnSecondary = row < rows - 1 || Random.value < 0.8f;
            if (!spawnSecondary)
                continue;

            float side = Random.value < 0.5f ? -1f : 1f;
            float secondaryX = side * Random.Range(halfWidth * 0.16f, halfWidth * 0.32f);
            float secondaryY = baseY + Random.Range(-0.28f, 0.28f);
            float secondaryScale = Random.Range(0.44f, 0.82f);
            CreateMeteorPiece(parent, new Vector3(secondaryX, secondaryY, 0f), secondaryScale, 2);
        }
    }

    void CreateInnerEdgeHazard(Transform parent)
    {
        bool topWall = transform.localPosition.y > 0f;
        float edgeY = topWall
            ? -wallSize.y * 0.5f + 0.18f
            : wallSize.y * 0.5f - 0.18f;
        float halfWidth = wallSize.x * 0.5f;

        for (int i = 0; i < 3; i++)
        {
            float x = Random.Range(-halfWidth * 0.14f, halfWidth * 0.14f);
            GameObject glow = new GameObject("HazardGlow");
            glow.transform.SetParent(parent, false);
            glow.transform.localPosition = new Vector3(x, edgeY, 0f);
            glow.transform.localScale = new Vector3(Random.Range(0.58f, 0.78f), 0.16f, 1f);

            SpriteRenderer glowRenderer = glow.AddComponent<SpriteRenderer>();
            glowRenderer.sprite = SpriteHelper.Circle;
            glowRenderer.color = new Color(1f, 0.46f, 0.22f, 0.12f);
            glowRenderer.sortingOrder = 0;

            float meteorYOffset = topWall ? Random.Range(0.08f, 0.22f) : Random.Range(-0.22f, -0.08f);
            CreateMeteorPiece(parent, new Vector3(x + Random.Range(-0.12f, 0.12f), edgeY + meteorYOffset, 0f),
                Random.Range(0.36f, 0.58f), 2);
        }
    }

    void CreateMeteorPiece(Transform parent, Vector3 localPosition, float scale, int sortingOrder)
    {
        GameObject meteor = new GameObject("Meteor");
        meteor.transform.SetParent(parent, false);
        meteor.transform.localPosition = localPosition;
        meteor.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        meteor.transform.localScale = Vector3.one * scale;

        SpriteRenderer renderer = meteor.AddComponent<SpriteRenderer>();
        renderer.sprite = KenneyAssets.GetObstacleMeteorSprite(Random.Range(0, KenneyAssets.ObstacleMeteorCount));
        renderer.color = Color.Lerp(new Color(0.82f, 0.82f, 0.86f), Color.white, Random.Range(0.2f, 1f));
        renderer.sortingOrder = sortingOrder;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player) player.TakeDamage(damage);
        }
    }
}
