using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject monsterPrefab;
    public Transform spawnPoint;
    public Transform[] waypoints;

    public int currency = 500;
    public int spawnCost = 20;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TrySpawnMonster();
        }
    }

    void TrySpawnMonster()
    {
        if (currency < spawnCost)
        {
            Debug.Log("Not enough currency!");
            return;
        }

        currency -= spawnCost;
        GameObject monster = Instantiate(monsterPrefab, spawnPoint.position, Quaternion.identity);
        MonsterMover mover = monster.GetComponent<MonsterMover>();
        mover.waypoints = waypoints;

        Debug.Log($"Spawned monster. Currency left: {currency}");
    }
}