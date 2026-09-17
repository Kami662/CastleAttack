using UnityEngine;

public class MonsterMover : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 3f;
    public int damage = 10;

    private int currentWaypointIndex = 0;

    void Update()
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

    void OnReachedCastle()
    {
        Castle castle = FindObjectOfType<Castle>();
        if (castle != null)
        {
            castle.TakeDamage(damage); // Example damage value
        }
        Destroy(gameObject);
    }

    public int maxHP = 30;
private int currentHP;

void Start()
{
    currentHP = maxHP;
}

public void TakeDamage(int amount)
{
    currentHP -= amount;
    if (currentHP <= 0)
    {
        Destroy(gameObject);
    }
}
}