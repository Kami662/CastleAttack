using UnityEngine;

public class MonsterMover : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 3f;
    public int damage = 10;

    public int maxHP = 30;
    private int currentHP;

    private int currentWaypointIndex = 0;

    [Header("Attacking towers")]
    [Tooltip("How close the monster gets before it starts hitting a tower.")]
    public float attackRange = 1.5f;
    [Tooltip("Hits per second dealt to a tower it's attacking (uses 'damage' per hit).")]
    public float attacksPerSecond = 1f;

    // The prefab's authored stats, captured once in Awake. OnSpawn restores
    // these every spawn so a pooled monster never keeps a previous card's
    // override; ApplyStats re-applies a card override on top afterwards.
    private int defaultMaxHP;
    private int defaultDamage;
    private float defaultSpeed;
    private Vector3 defaultScale;

    // Attack-tower runtime state.
    private Tower targetTower;
    private float attackCooldown;

    // Per-card tower traits (Sapper / Stunner). Per-life state: reset in OnSpawn.
    private float towerDamageMultiplier = 1f;
    private float stunSeconds;

    [HideInInspector] public GameObject SourcePrefab;
    public bool IsAlive { get; private set; }

    // Which card summoned this monster. Not read anywhere yet — prep for
    // per-card-group unit orders (GDD §8), so that system doesn't need to
    // retrofit an identity tag onto every already-alive monster later.
    public CardDefinition SourceCard { get; private set; }

    /// <summary>The group (one card play) this monster follows orders with. Null = none, so
    /// the default order applies. Per-life state: reset in OnSpawn.</summary>
    public UnitGroup Group { get; private set; }

    void OnEnable() { MonsterSpawner.Register(this); }
    void OnDisable() { MonsterSpawner.Unregister(this); }

    void Awake()
    {
        defaultMaxHP = maxHP;
        defaultDamage = damage;
        defaultSpeed = speed;
        defaultScale = transform.localScale;
        currentHP = maxHP;
    }

    /// <summary>
    /// Reset all per-life state. Called by MonsterSpawner on EVERY spawn (fresh
    /// or pooled) — Start()/field initializers don't run on a reused object, so
    /// this is where per-life state must be cleared.
    /// </summary>
    public void OnSpawn(Transform[] wp)
    {
        waypoints = wp;

        maxHP = defaultMaxHP;
        damage = defaultDamage;
        speed = defaultSpeed;
        transform.localScale = defaultScale;

        currentHP = maxHP;
        currentWaypointIndex = 0;
        targetTower = null;
        attackCooldown = 0f;
        towerDamageMultiplier = 1f;
        stunSeconds = 0f;
        IsAlive = true;
        SourceCard = null;
        Group = null;
        enabled = true;
    }

    /// <summary>Per-card stat override (a "boss"/"elite" from the same prefab).</summary>
    public void ApplyStats(int hp, int dmg, float spd, float scale)
    {
        maxHP = hp;
        damage = dmg;
        speed = spd;
        currentHP = hp;
        transform.localScale = defaultScale * scale;
    }

    /// <summary>Per-card tower traits: a damage multiplier against towers/towns
    /// (Sapper) and a stun applied to a tower on each hit (Stunner).</summary>
    public void ApplyTowerTraits(float damageMultiplier, float stun)
    {
        towerDamageMultiplier = damageMultiplier;
        stunSeconds = stun;
    }

    /// <summary>Called by MonsterSpawner right after OnSpawn, so a reused
    /// monster never keeps a previous card's identity.</summary>
    public void SetSourceCard(CardDefinition card)
    {
        SourceCard = card;
    }

    /// <summary>Called by MonsterSpawner right after OnSpawn: which group's orders this monster follows.</summary>
    public void SetGroup(UnitGroup group)
    {
        Group = group;
    }

    void Update()
    {
        // A defender in reach comes first: the monster stops and fights it, which
        // is what makes footmen real blockers (GDD §3 "Defenders").
        if (FightNearestDefender()) return;

        // Each monster follows its group's order (so a card's units can be sent
        // to different targets); no group means the default order.
        UnitCommander.Order order = Group != null ? Group.Order : UnitCommander.DefaultOrder;

        // Halt: stand still. (A defender in reach is still fought, above.)
        if (order == UnitCommander.Order.Halt) return;

        // Attack towers: go after the nearest standing one. If none are left,
        // fall through to normal path-following.
        if (order == UnitCommander.Order.AttackTowers && AttackNearestTower())
            return;

        FollowPath();
    }

    // Returns true if a defender was in reach this frame. Shares attackRange,
    // attacksPerSecond and attackCooldown with tower attacks (all reset in OnSpawn).
    bool FightNearestDefender()
    {
        var defenders = Defender.Active;
        if (defenders.Count == 0) return false;

        Defender foe = null;
        float bestSqr = attackRange * attackRange;
        Vector3 pos = transform.position;
        for (int i = 0; i < defenders.Count; i++)
        {
            Defender d = defenders[i];
            if (d == null || !d.IsAlive) continue;

            // Ground-plane distance: soldiers stand at a different height than monsters.
            float dx = d.transform.position.x - pos.x;
            float dz = d.transform.position.z - pos.z;
            float sqr = dx * dx + dz * dz;
            if (sqr <= bestSqr) { bestSqr = sqr; foe = d; }
        }
        if (foe == null) return false;

        attackCooldown -= Time.deltaTime;
        if (attackCooldown <= 0f)
        {
            foe.TakeDamage(damage);
            attackCooldown = 1f / Mathf.Max(0.01f, attacksPerSecond);
        }
        return true;
    }

    // Returns true if there was a tower to deal with this frame.
    bool AttackNearestTower()
    {
        if (targetTower == null || targetTower.IsDestroyed)
            targetTower = FindNearestTower();

        if (targetTower == null) return false;

        Vector3 towerPos = targetTower.transform.position;
        if ((towerPos - transform.position).sqrMagnitude > attackRange * attackRange)
        {
            transform.position = Vector3.MoveTowards(transform.position, towerPos, speed * Time.deltaTime);
        }
        else
        {
            attackCooldown -= Time.deltaTime;
            if (attackCooldown <= 0f)
            {
                targetTower.TakeDamage(Mathf.RoundToInt(damage * towerDamageMultiplier));
                if (stunSeconds > 0f) targetTower.Stun(stunSeconds);
                attackCooldown = 1f / Mathf.Max(0.01f, attacksPerSecond);
            }
        }
        return true;
    }

    void FollowPath()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypointIndex];
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
                OnReachedCastle();
                enabled = false;
            }
        }
    }

    Tower FindNearestTower()
    {
        Tower closest = null;
        float bestSqr = Mathf.Infinity;
        var towers = Tower.StandingTowers;
        for (int i = 0; i < towers.Count; i++)
        {
            Tower t = towers[i];
            if (t == null || t.IsDestroyed) continue;
            float sqrDist = (t.transform.position - transform.position).sqrMagnitude;
            if (sqrDist < bestSqr) { bestSqr = sqrDist; closest = t; }
        }
        return closest;
    }

    void OnReachedCastle()
    {
        Castle castle = FindAnyObjectByType<Castle>();
        if (castle != null) castle.TakeDamage(damage);
        Die();
    }

    public void TakeDamage(int amount)
    {
        if (!IsAlive) return;
        currentHP -= amount;
        if (currentHP <= 0) Die();
    }

    void Die()
    {
        IsAlive = false;
        if (MonsterSpawner.Instance != null)
            MonsterSpawner.Instance.Despawn(this);
        else
            Destroy(gameObject);
    }
}