using UnityEngine;

/// <summary>
/// Simple sprite-based visual effects that work reliably in URP.
/// Uses small SpriteRenderer GameObjects instead of ParticleSystem for reliability.
/// </summary>
public static class VFXHelper
{
    public static void SpawnExplosion(Vector3 position, Color color, int count = 8, float force = 3f)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject p = new GameObject("FX_Particle");
            p.transform.position = position;
            p.transform.localScale = Vector3.one * Random.Range(0.1f, 0.25f);

            SpriteRenderer sr = p.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteHelper.Circle;
            sr.color = color;
            sr.sortingOrder = 10;

            ParticleMover mover = p.AddComponent<ParticleMover>();
            float angle = (360f / count) * i + Random.Range(-20f, 20f);
            float rad = angle * Mathf.Deg2Rad;
            mover.velocity = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0)
                             * force * Random.Range(0.5f, 1.3f);
            mover.lifetime = Random.Range(0.3f, 0.6f);
            mover.startColor = color;
        }
    }

    public static void SpawnPickupEffect(Vector3 position, Color color, int count = 8)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject p = new GameObject("FX_Pickup");
            p.transform.position = position + (Vector3)Random.insideUnitCircle * 0.2f;
            p.transform.localScale = Vector3.one * Random.Range(0.06f, 0.14f);

            SpriteRenderer sr = p.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteHelper.Circle;
            sr.color = color;
            sr.sortingOrder = 10;

            ParticleMover mover = p.AddComponent<ParticleMover>();
            mover.velocity = new Vector3(Random.Range(-1f, 1f), Random.Range(1.5f, 3f), 0);
            mover.lifetime = Random.Range(0.4f, 0.8f);
            mover.startColor = color;
        }
    }

    public static void SpawnImpact(Vector3 position, Color color)
    {
        SpawnExplosion(position, color, 5, 2f);
    }
}

/// <summary>
/// Animates a single sprite particle: moves, fades, shrinks, then self-destructs.
/// </summary>
public class ParticleMover : MonoBehaviour
{
    public Vector3 velocity;
    public float lifetime = 0.5f;
    public Color startColor = Color.white;

    private float timer;
    private SpriteRenderer sr;
    private float initialLifetime;

    void Start()
    {
        timer = lifetime;
        initialLifetime = lifetime;
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f) { Destroy(gameObject); return; }

        transform.position += velocity * Time.deltaTime;
        velocity *= 0.94f; // drag

        float ratio = timer / initialLifetime;
        transform.localScale *= 0.97f;

        if (sr != null)
        {
            Color c = startColor;
            c.a = ratio;
            sr.color = c;
        }
    }
}
