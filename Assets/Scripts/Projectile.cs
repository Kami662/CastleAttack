using UnityEngine;

/// <summary>
/// A tower's shot. Placeholder visual for now (a small sphere) — swap the mesh
/// for an arrow later without touching this script. Flies to its target and
/// deals damage on arrival; if the target dies mid-flight it fizzles at the
/// last known spot. Pooled via ProjectilePool (mirrors MonsterSpawner) — a
/// projectile is reused, not destroyed, so every per-life field must reset in
/// Launch(), the same way MonsterMover.OnSpawn resets a reused monster.
/// </summary>
public class Projectile : MonoBehaviour
{
    public float speed = 20f;
    public float hitDistance = 0.3f;
    public float maxLifetime = 3f;

    private MonsterMover target;
    private int damage;
    private Vector3 lastKnownTargetPos;
    private float age;

    [HideInInspector] public GameObject SourcePrefab;

    /// <summary>Called by the tower right after it spawns/reuses the
    /// projectile. Doubles as the pooling reset point.</summary>
    public void Launch(MonsterMover target, int damage)
    {
        this.target = target;
        this.damage = damage;
        age = 0f;
        lastKnownTargetPos = target != null ? target.transform.position : transform.position;
    }

    void Update()
    {
        age += Time.deltaTime;
        if (age >= maxLifetime) { Despawn(); return; }

        // The target monster is pooled too — despawning deactivates it rather
        // than destroying it, so a plain null check never goes false. IsAlive
        // is the real "did it die" signal; without this the projectile can
        // chase (and damage) whatever fresh monster later reuses that instance.
        if (target != null && !target.IsAlive)
            target = null;

        Vector3 destination = target != null ? target.transform.position : lastKnownTargetPos;
        if (target != null) lastKnownTargetPos = destination;

        transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);

        if ((transform.position - destination).sqrMagnitude <= hitDistance * hitDistance)
        {
            if (target != null) target.TakeDamage(damage);
            Despawn();
        }
    }

    void Despawn()
    {
        target = null;
        if (ProjectilePool.Instance != null && SourcePrefab != null)
            ProjectilePool.Instance.Despawn(this);
        else
            Destroy(gameObject);
    }
}