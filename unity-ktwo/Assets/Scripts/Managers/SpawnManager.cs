using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// Should exist primarily server-side
// to spawn zombies and players.
public enum SpawnZone
{
    North, South, East, West
}

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance;

    public List<GameObject> playerSpawns;
    public List<GameObject> zombieSpawns;

    public GameObject zombie;
    public SpawnZone StartSpawnZone;

    public bool SpawnOnStartup;

    public const float SPAWN_DELAY = 0.5f;

    void Awake()
    {
        playerSpawns = new List<GameObject>();
        zombieSpawns = new List<GameObject>();
        instance = this;
    }

    public void SpawnZombieAtPoint(SpawnZone sa)
    {
        var destination = GameObject.Find(string.Format("ZombieSpawnZone{0}", sa.ToString()));
        var spawn = destination
            .GetComponentsInChildren<Transform>()
            [Random.Range(0, destination.transform.childCount)];
        SpawnEnemy(zombie, spawn.position, spawn.rotation);
    }

    public void SpawnZombieAtRandomPoint()
    {
        var destination = zombieSpawns[Random.Range(0, zombieSpawns.Count)].transform;
        SpawnEnemy(zombie, destination.position, destination.rotation);
    }

    public void SpawnMultipleZombiesAtRandomPoints(int numberOfZombies)
    {
        StartCoroutine(TimedSpawn(numberOfZombies));
    }

    public IEnumerator TimedSpawn(int numberOfZombies)
    {
        var counter = 0;
        while (counter < numberOfZombies)
        {
            SpawnZombieAtRandomPoint();
            counter++;
            yield return new WaitForSeconds(SPAWN_DELAY);
        }
    }

    public void SpawnAtClosest(Transform t)
    {
        var shortest = Mathf.Infinity;
        GameObject destination = null;
        foreach (GameObject go in zombieSpawns)
        {
            var distance = Mathf.Abs(Vector3.Distance(t.transform.position, go.transform.position));

            if (distance < shortest)
            {
                destination = go;
                shortest = distance;
            }
        }

        SpawnEnemy(zombie, destination.transform.position, destination.transform.rotation);
    }

    void SpawnEnemy(GameObject enemy, Vector3 position, Quaternion rotation)
    {
        var go = Instantiate(enemy, position, rotation);
        NetworkServer.Spawn(go);
        EnemyManager.instance.zombies.Add(go);
    }

    public void SpawnPlayers(Dictionary<NetworkConnection, PlayerConnectionObject> connections)
    {
        foreach (var kvp in connections)
        {
            var targetString = kvp.Value.connectionNumber != 0 ? 
                string.Format("PlayerSpawnPoint ({0})", kvp.Value.connectionNumber):
                "PlayerSpawnPoint"; 

            var spawnDestination = GameObject.Find(targetString);

            GameObject go = Instantiate(
                CharacterManager.instance.characters[kvp.Value.chosenCharacter],
                spawnDestination.transform.position,
                spawnDestination.transform.rotation
            );

            Debug.Log(string.Format("connection number: {0}  spawning at: {1}", kvp.Value.connectionNumber, targetString));
            NetworkServer.Spawn(go);
            go.GetComponent<NetworkIdentity>().AssignClientAuthority(kvp.Key);
        }
    }
}
