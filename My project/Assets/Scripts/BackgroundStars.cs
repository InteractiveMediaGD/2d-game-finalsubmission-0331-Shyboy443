using UnityEngine;

/// <summary>
/// Creates a parallax scrolling starfield background with multiple layers.
/// Stars wrap around when they scroll off the left edge.
/// </summary>
public class BackgroundStars : MonoBehaviour
{
    private struct Star
    {
        public Transform transform;
        public float speed;
    }

    private Star[] stars;
    private float xMin, xMax, yMin, yMax;

    public void Initialize(int starCount = 80)
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        xMin = -halfWidth - 1f;
        xMax = halfWidth + 1f;
        yMin = -halfHeight - 0.5f;
        yMax = halfHeight + 0.5f;

        stars = new Star[starCount];

        for (int i = 0; i < starCount; i++)
        {
            // 3 depth layers: far (dim/small/slow), mid, near (bright/large/fast)
            int layer = i % 3;
            float depth = (layer + 1) / 3f; // 0.33, 0.66, 1.0

            GameObject star = new GameObject("Star");
            star.transform.SetParent(transform);
            star.transform.position = new Vector3(
                Random.Range(xMin, xMax),
                Random.Range(yMin, yMax),
                0);

            float size = Mathf.Lerp(0.03f, 0.08f, depth) * Random.Range(0.7f, 1.3f);
            star.transform.localScale = Vector3.one * size;

            SpriteRenderer sr = star.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteHelper.Circle;
            float brightness = Mathf.Lerp(0.15f, 0.6f, depth) * Random.Range(0.7f, 1.0f);
            sr.color = new Color(brightness, brightness, brightness * 1.1f, brightness);
            sr.sortingOrder = -10 + layer;

            stars[i] = new Star
            {
                transform = star.transform,
                speed = Mathf.Lerp(0.3f, 1.2f, depth) * Random.Range(0.8f, 1.2f)
            };
        }
    }

    void Update()
    {
        if (stars == null) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        bool gameplayActive = GameManager.Instance != null && GameManager.Instance.IsGameplayActive;
        float dt = gameplayActive ? Time.deltaTime : Time.unscaledDeltaTime;
        float baseSpeed = 0.35f;
        if (GameManager.Instance != null && GameManager.Instance.HasStarted)
            baseSpeed = Mathf.Max(GameManager.Instance.ScrollSpeed * 0.3f, 0.45f);

        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i].transform == null) continue;

            Vector3 pos = stars[i].transform.position;
            pos.x -= stars[i].speed * baseSpeed * dt;

            // Wrap around
            if (pos.x < xMin)
            {
                pos.x = xMax;
                pos.y = Random.Range(yMin, yMax);
            }

            stars[i].transform.position = pos;
        }
    }
}
