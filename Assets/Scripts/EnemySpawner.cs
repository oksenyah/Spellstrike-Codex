using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{

    [SerializeField] List<GameObject> enemyPrefabs;

    [SerializeField] Wizard wizard;

    [SerializeField] float spawnTime = 1f;
    [SerializeField] float startEnemies = 5;
    [SerializeField] float spawnDistance = 1000;
    [Header("Audio")]
    [SerializeField] AudioSource minionDeathAudio;

    public float timer = 0;

    // Start is called before the first frame update
    void Start()
    {
        SpawnStartingEnemies();
        SpawnEnemies();
    }

    void SpawnEnemy(){
        Vector3 spawnPos = wizard.transform.position + new Vector3(Random.Range(-1f,1f),Random.Range(-1f,1f),0).normalized * spawnDistance;
        while (Vector2.Distance(spawnPos, wizard.transform.position) < spawnDistance) {
            spawnPos = wizard.transform.position + new Vector3(Random.Range(-1f,1f),Random.Range(-1f,1f),0).normalized * spawnDistance;
        }
        GameObject enemyObject = Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Count)], spawnPos, Quaternion.identity);
        enemyObject.GetComponent<Enemy>().SetDeathAudio(minionDeathAudio);
        EnemyAI enemyAI = enemyObject.GetComponent<EnemyAI>();
        enemyAI.SetTarget(wizard.transform);
    }

    void SpawnEnemyNearPlayer(){
        Vector3 spawnPos = wizard.transform.position + new Vector3(Random.Range(-spawnDistance, spawnDistance), Random.Range(-spawnDistance, spawnDistance), 0);
        GameObject enemyObject = Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Count)], spawnPos, Quaternion.identity);
        enemyObject.GetComponent<Enemy>().SetDeathAudio(minionDeathAudio);
        EnemyAI enemyAI = enemyObject.GetComponent<EnemyAI>();
        enemyAI.SetTarget(wizard.transform);
    }

    void SpawnStartingEnemies(){
        for(int i = 0; i<startEnemies; i++){
            SpawnEnemyNearPlayer();
        }
    }


    void SpawnEnemies(){
        StartCoroutine(SpawnEnemiesRoutine());
        IEnumerator SpawnEnemiesRoutine()
        {
            while (true) {
                yield return new WaitForSeconds(spawnTime);
                SpawnEnemy();
            }
        }
    }



}
