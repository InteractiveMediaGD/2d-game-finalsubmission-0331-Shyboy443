using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shared projectile logic for player and enemy bullets.
/// Supports penetration, explosions, and boss weak points.
/// </summary>
public class Projectile : MonoBehaviour
{
    public enum ProjectileOwner
    {
        Player,
        Enemy,
    }

    [HideInInspector] public float speed = 12f;
    [HideInInspector] public float lifetime = 2f;
    [HideInInspector] public Vector2 direction = Vector2.right;
    [HideInInspector] public ProjectileOwner owner = ProjectileOwner.Player;
    [HideInInspector] public int damage = 1;
    [HideInInspector] public int penetration = 1;
    [HideInInspector] public float explosionRadius;
    [HideInInspector] public Color impactColor = new Color(1f, 0.9f, 0.3f);

    float timer;
    readonly HashSet<int> hitTargets = new HashSet<int>();

    void Start()
    {
        timer = lifetime;
        if (direction.sqrMagnitude <= 0.0001f)
            direction = owner == ProjectileOwner.Player ? Vector2.right : Vector2.left;

        direction.Normalize();
    }

    void Update()
    {
        transform.position += (Vector3)(direction * (speed * Time.deltaTime));

        timer -= Time.deltaTime;
        if (timer <= 0f)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null)
            return;

        int id = other.gameObject.GetInstanceID();
        if (hitTargets.Contains(id))
            return;

        if (other.GetComponent<Projectile>() != null)
            return;
        if (GetComponentInSelfOrParent<ScoreTrigger>(other) != null)
            return;

        hitTargets.Add(id);

        if (owner == ProjectileOwner.Player)
            HandlePlayerProjectileHit(other);
        else
            HandleEnemyProjectileHit(other);
    }

    void HandlePlayerProjectileHit(Collider2D other)
    {
        if (GetComponentInSelfOrParent<PlayerController>(other) != null)
            return;
        if (GetComponentInSelfOrParent<HealthPack>(other) != null)
            return;
        if (GetComponentInSelfOrParent<PowerUpPickup>(other) != null)
            return;

        BossWeakPoint weakPoint = GetComponentInSelfOrParent<BossWeakPoint>(other);
        if (weakPoint != null)
        {
            weakPoint.ApplyHit(damage);
            ResolveAfterHit(other.transform.position, false);
            return;
        }

        Enemy enemy = GetComponentInSelfOrParent<Enemy>(other);
        if (enemy != null)
        {
            enemy.TakeProjectileHit(damage);
            ResolveAfterHit(enemy.transform.position, false);
            return;
        }

        BossEnemy boss = GetComponentInSelfOrParent<BossEnemy>(other);
        if (boss != null)
        {
            boss.TakeProjectileHit(damage);
            ResolveAfterHit(boss.transform.position, false);
            return;
        }

        if (GetComponentInSelfOrParent<ObstacleWall>(other) != null)
        {
            VFXHelper.SpawnImpact(transform.position, impactColor);
            ResolveAfterHit(transform.position, true);
            return;
        }

        Destroy(gameObject);
    }

    void HandleEnemyProjectileHit(Collider2D other)
    {
        if (GetComponentInSelfOrParent<Enemy>(other) != null) return;
        if (GetComponentInSelfOrParent<BossEnemy>(other) != null) return;
        if (GetComponentInSelfOrParent<HealthPack>(other) != null) return;
        if (GetComponentInSelfOrParent<PowerUpPickup>(other) != null) return;

        PlayerController player = GetComponentInSelfOrParent<PlayerController>(other);
        if (player != null)
        {
            player.TakeDamage(damage);
            VFXHelper.SpawnImpact(transform.position, new Color(1f, 0.35f, 0.35f));
            ResolveAfterHit(player.transform.position, false);
            return;
        }

        if (GetComponentInSelfOrParent<ObstacleWall>(other) != null)
        {
            VFXHelper.SpawnImpact(transform.position, new Color(1f, 0.4f, 0.4f));
            ResolveAfterHit(transform.position, true);
            return;
        }

        Destroy(gameObject);
    }

    void ResolveAfterHit(Vector3 hitPosition, bool forceDestroy)
    {
        if (explosionRadius > 0f)
        {
            Explode(hitPosition);
            Destroy(gameObject);
            return;
        }

        if (forceDestroy)
        {
            Destroy(gameObject);
            return;
        }

        penetration--;
        if (penetration <= 0)
            Destroy(gameObject);
    }

    void Explode(Vector3 center)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, explosionRadius);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null)
                continue;

            if (owner == ProjectileOwner.Player)
            {
                Enemy enemy = GetComponentInSelfOrParent<Enemy>(hit);
                if (enemy != null)
                    enemy.TakeProjectileHit(damage);

                BossWeakPoint weakPoint = GetComponentInSelfOrParent<BossWeakPoint>(hit);
                if (weakPoint != null)
                    weakPoint.ApplyHit(damage);
                else
                {
                    BossEnemy boss = GetComponentInSelfOrParent<BossEnemy>(hit);
                    if (boss != null)
                        boss.TakeProjectileHit(damage);
                }
            }
            else
            {
                PlayerController player = GetComponentInSelfOrParent<PlayerController>(hit);
                if (player != null)
                    player.TakeDamage(damage);
            }
        }

        VFXHelper.SpawnExplosion(center, impactColor, 10, explosionRadius * 2.5f);
    }

    static T GetComponentInSelfOrParent<T>(Collider2D other) where T : Component
    {
        if (other == null)
            return null;

        return other.GetComponent<T>() ?? other.GetComponentInParent<T>();
    }
}
