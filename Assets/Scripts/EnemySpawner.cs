using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] List<GameObject> enemyPrefabs;

    [SerializeField] float spawnTime = 5f;
    [SerializeField] int startEnemies = 3;
    [Header("Audio")]
    [SerializeField] AudioSource enemyDeathAudio;
    [Header("Config")]
    [SerializeField] bool isConfigOverrideable = true;

    public float timer = 0;
    private Cell dungeonCell;

    // Start is called before the first frame update
    void Start() {
        this.dungeonCell = GetComponent<Cell>();
    }

    void SpawnEnemyInDungeon() {
        float halfWidth = dungeonCell.GetWidth() / 2 - 0.2f;
        float halfLength = dungeonCell.GetLength() / 2 - 0.2f;
        Vector3 spawnPos = new Vector3(dungeonCell.transform.position.x + Random.Range(-halfWidth, halfWidth), dungeonCell.transform.position.y + Random.Range(-halfLength, halfLength), 0);
        // Debug.Log("Spawning Enemy in Dungeon! Spawn Position: " + spawnPos + ", Cell Position: " + dungeonCell.transform.position);
        GameObject enemyObject = Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Count)], spawnPos, Quaternion.identity);
        enemyObject.GetComponent<Enemy>().SetDeathAudio(enemyDeathAudio);
    }

    public void SpawnStartingEnemies() {
        for(int i = 0; i < startEnemies; i++){
            SpawnEnemyInDungeon();
        }
    }

    public void SpawnEnemies() {
        if (spawnTime != 0) {
            StartCoroutine(SpawnEnemiesRoutine());
            IEnumerator SpawnEnemiesRoutine() {
                while (true) {
                    yield return new WaitForSeconds(spawnTime);
                    SpawnEnemyInDungeon();
                }
            }
        }
    }

    public void SetEnemyDeathAudio(AudioSource enemyDeathAudio) {
        if (isConfigOverrideable) {
            this.enemyDeathAudio = enemyDeathAudio;
        }
    }

    public void SetStartingEnemiesCount(int startingEnemiesCount) {
        if (isConfigOverrideable) {
            this.startEnemies = startingEnemiesCount;
        }
    }

    public void SetSpawnIntervalInSeconds(float spawnIntervalInSeconds) {
        if (isConfigOverrideable) {
            this.spawnTime = spawnIntervalInSeconds;
        }
    }
}
