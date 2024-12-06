using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{

    [SerializeField] List<GameObject> enemyPrefabs;

    [SerializeField] float spawnTime = 5f;
    [SerializeField] float startEnemies = 3;
    [Header("Audio")]
    [SerializeField] AudioSource enemyDeathAudio;

    public float timer = 0;
    private Cell dungeonCell;

    // Start is called before the first frame update
    void Start() {
        this.dungeonCell = GetComponent<Cell>();
    }

    void SpawnEnemyInDungeon() {
        float halfWidth = dungeonCell.GetWidth() / 2;
        float halfLength = dungeonCell.GetLength() / 2;
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
        StartCoroutine(SpawnEnemiesRoutine());
        IEnumerator SpawnEnemiesRoutine() {
            while (true) {
                yield return new WaitForSeconds(spawnTime);
                SpawnEnemyInDungeon();
            }
        }
    }

    public void SetEnemyDeathAudio(AudioSource enemyDeathAudio) {
        this.enemyDeathAudio = enemyDeathAudio;
    }
}
