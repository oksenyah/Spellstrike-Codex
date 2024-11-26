using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{

    private LehmerRandom lehmerRandom;
    private float spawnRadius;
    private List<GameObject> dungeonBlocks = new List<GameObject>();

    [Header("Audio")]
    [SerializeField] AudioSource welcomeToTheDungeonAudio;
    [Header("Generator Settings")]
    [SerializeField] int numberOfCells = 1;
    [SerializeField] float separatorForce = 1.0f;
    [SerializeField] float widthMax = 10;
    [SerializeField] float widthMin = 2;
    [SerializeField] float heightMax = 10;
    [SerializeField] float heightMin = 2;
    [Header("DungeonItems")]
    [SerializeField] GameObject dungeonBlockPrefab;
    [SerializeField] GameObject codexPrefab;

    void Awake() {
        spawnRadius = numberOfCells / ((float) Math.PI) * 0.3f;
        lehmerRandom = GetComponent<LehmerRandom>();
    }

    // Start is called before the first frame update
    void Start() {
        Generate(1);
    }

    public void Generate(int seed) {
        UnityEngine.Random.InitState(seed);

        // https://www.reddit.com/r/gamedev/comments/1dlwc4/procedural_dungeon_generation_algorithm_explained/?rdt=36321

        for(int i = 0; i<numberOfCells; i++){
            float randomX = UnityEngine.Random.Range(-spawnRadius, spawnRadius);
            float randomY = UnityEngine.Random.Range(-spawnRadius, spawnRadius);
            GameObject dungeonBlock = Instantiate(dungeonBlockPrefab,new Vector3(randomX, randomY, 0),Quaternion.identity);
            dungeonBlock.GetComponent<Cell>().SetDimensions((float) lehmerRandom.GetNextDouble(0, widthMax), (float) lehmerRandom.GetNextDouble(0, heightMax));
            dungeonBlocks.Add(dungeonBlock);
        }

        SeparateBlocks();

        welcomeToTheDungeonAudio.Play();
    }

    private bool IsOverlapping(Transform a, Transform b) {
        var bottomLeft = a.transform.position - new Vector3(a.transform.localScale.x / 2, a.transform.localScale.y / 2);
        var topRight = a.transform.position + new Vector3(a.transform.localScale.x / 2, a.transform.localScale.y / 2);

        var otherBottomLeft = b.transform.position - new Vector3(b.transform.localScale.x / 2, b.transform.localScale.y / 2);
        var otherTopRight = b.transform.position + new Vector3(b.transform.localScale.x / 2, b.transform.localScale.y / 2);

        // if (RectA.Left < RectB.Right && RectA.Right > RectB.Left &&
        // RectA.Top > RectB.Bottom && RectA.Bottom < RectB.Top ) 
        bool isOverlapping = (bottomLeft.x < otherTopRight.x && topRight.x > otherBottomLeft.x &&
        topRight.y > otherBottomLeft.y && bottomLeft.y < otherTopRight.y);

        return isOverlapping;
    }

    private void SeparateBlocks() {
        StartCoroutine(SeparateBlocksRoutine());
        IEnumerator SeparateBlocksRoutine(){
            bool overlapDetected = true;
            while(overlapDetected) {
                overlapDetected = false;

                foreach (GameObject dungeonBlock in dungeonBlocks) {
                    Cell cell = dungeonBlock.GetComponent<Cell>();
                    Vector3 separation = computeSeparation(dungeonBlock);
                    if (separation != Vector3.zero) {
                        overlapDetected = true;
                        dungeonBlock.GetComponent<SpriteRenderer>().color = Color.red;
                    } else {
                        dungeonBlock.GetComponent<SpriteRenderer>().color = Color.grey;
                    }
                    cell.ShiftPosition(separation);
                    // yield return new WaitForSeconds(0.01f);
                }

                if (overlapDetected) {
                    Debug.Log("Overlaps detected. Sleeping...");
                    yield return new WaitForSeconds(0.001f);
                }
            }

            foreach (GameObject dungeonBlock in dungeonBlocks) {
                Cell cell = dungeonBlock.GetComponent<Cell>();
                if (cell.IsRoomCandidate()) {
                    dungeonBlock.GetComponent<SpriteRenderer>().color = Color.green;
                }
            }
            yield return null;
        }
    }

    Vector3 computeSeparation(GameObject agent) {

        int neighborCount = 0;
        Vector3 v = Vector3.zero;

        foreach (GameObject dungeonBlock in dungeonBlocks) {
            if (dungeonBlock != agent) {
                if (IsOverlapping(agent.transform, dungeonBlock.transform)) {
                    v += agent.transform.position - dungeonBlock.transform.position;
                    neighborCount++;
                }
            }
        }

        if (neighborCount == 0)
            return Vector3.zero;

        v /= neighborCount;
        v.Normalize();
        v *= separatorForce;
        return v;
    }

}
