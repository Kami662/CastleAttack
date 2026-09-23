using UnityEngine;

/// <summary>
/// A tower's shot. Placeholder visual for now (a small sphere) — swap the mesh
/// for an arrow later without touching this script. Flies to its target and
/// deals damage on arrival; if the target dies mid-flight it fizzles at the
/// last known spot. Not pooled yet (low volume); pool later if needed.
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

    /// <summary>Called by the tower right after it spawns the projectile.</summary>
    public void Launch(MonsterMover target, int damage)
    {
        this.target = target;
        this.damage = damage;
        if (target != null) lastKnownTargetPos = target.transform.position;
    }

    void Update()
    {
        age += Time.deltaTime;
        if (age >= maxLifetime) { Destroy(gameObject); return; }

        Vector3 destination = target != null ? target.transform.position : lastKnownTargetPos;
        if (target != null) lastKnownTargetPos = destination;

        transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, destination) <= hitDistance)
        {
            if (target != null) target.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}