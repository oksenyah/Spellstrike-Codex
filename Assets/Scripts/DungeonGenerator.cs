using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{

    [Header("Audio")]
    [SerializeField] AudioSource welcomeToTheDungeonAudio;

    [Header("DungeonItems")]
    [SerializeField] GameObject dungeonBlockPrefab;
    [SerializeField] GameObject codexPrefab;

    // Start is called before the first frame update
    void Start() {
        Generate(1);
    }

    public void Generate(int seed) {
        Random.InitState(seed);

        //Blocks
        int numBlocks = Random.Range(1,1000);
        float range = 0;
        float randomX = 0;
        float randomY = 0;

        for(int i = 0; i<numBlocks; i++){
            range = numBlocks / 10;
            randomX = Random.Range(-range, range);
            randomY = Random.Range(-range, range);
            Instantiate(dungeonBlockPrefab,new Vector3(randomX, randomY, 0),Quaternion.identity);
        }


        range = numBlocks / 20;
        randomX = Random.Range(-range, range);
        randomY = Random.Range(-range, range);
        Instantiate(codexPrefab,new Vector3(randomX, randomY, 0),Quaternion.identity);

        welcomeToTheDungeonAudio.Play();
    }
}
