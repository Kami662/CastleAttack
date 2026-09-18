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

        if (MonsterSpawner.Instance == null)
        {
            Debug.LogError("No MonsterSpawner in the scene — add one before spawning.");
            return;
        }

        currency -= spawnCost;
        MonsterSpawner.Instance.Spawn(monsterPrefab, spawnPoint.position, waypoints);

        Debug.Log($"Spawned monster. Currency left: {currency}");
    }
}
