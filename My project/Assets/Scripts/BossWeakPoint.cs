using UnityEngine;

/// <summary>
/// Weak point hitbox for bosses. Player shots should prioritize this over the main hull.
/// </summary>
public class BossWeakPoint : MonoBehaviour
{
    public BossEnemy owner;
    public float damageMultiplier = 1.8f;

    public void ApplyHit(int damage)
    {
        if (owner == null)
            return;

        int scaledDamage = Mathf.Max(1, Mathf.RoundToInt(damage * damageMultiplier));
        owner.TakeWeakPointHit(scaledDamage);
    }
}
