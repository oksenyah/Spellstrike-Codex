using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DelaunatorSharp;
using DelaunatorSharp.Unity.Extensions;
using System.Linq;

public class DungeonGenerator : MonoBehaviour
{

    private LehmerRandom lehmerRandom;
    private float spawnRadius;
    private List<GameObject> dungeonBlocks = new List<GameObject>();
    private List<Cell> rooms = new List<Cell>();

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
                    yield return new WaitForSeconds(0.0001f);
                }
            }

            List<IPoint> points = new List<IPoint>();
            foreach (GameObject dungeonBlock in dungeonBlocks) {
                Cell cell = dungeonBlock.GetComponent<Cell>();
                if (cell.IsRoomCandidate()) {
                    dungeonBlock.GetComponent<SpriteRenderer>().color = Color.green;
                    cell.BuildWalls();
                    rooms.Add(cell);
                    points.Add(new Point(cell.transform.position.x, cell.transform.position.y));
                } else {
                    // Clean up dungeon blocks that aren't rooms
                    Destroy(dungeonBlock);
                }
            }

            Delaunator delaunator = new Delaunator(points.ToArray());
            // delaunator.ForEachTriangleEdge(edge => {
            //     Debug.DrawLine(edge.P.ToVector3(), edge.Q.ToVector3(), Color.blue, float.PositiveInfinity);
            // });

            List<WeightedEdge> mstEdges = CalculateMST(delaunator);
            foreach (WeightedEdge weightedEdge in mstEdges) {
                Debug.Log("Source: " + weightedEdge.src + ", Destination: " + weightedEdge.dest);
                Debug.DrawLine(weightedEdge.src.ToVector3(), weightedEdge.dest.ToVector3(), Color.blue, float.PositiveInfinity);

                
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

    class WeightedEdge : IComparable<WeightedEdge> { 
        public IPoint src, dest;
        public float weight;

        public WeightedEdge(IPoint src, IPoint dest) {
            this.src = src;
            this.dest = dest;
            this.weight = Vector3.Distance(new Vector3((float) src.X, (float) src.Y), new Vector3((float) dest.X, (float) dest.Y));
        }
  
        // Comparator function used for sorting edges based on weight
        public int CompareTo(WeightedEdge otherEdge) {
            if (this.weight <  otherEdge.weight) return -1;
            if (this.weight == otherEdge.weight) return 0;
            return 1;
        } 
    }

    // Union
    class Subset {
        public int parent, rank;
    };

    private int FindInSubset(Subset[] subsets, int i) {
        if (subsets[i].parent != i)
        subsets[i].parent = FindInSubset(subsets, subsets[i].parent);
        return subsets[i].parent;
    }

    void Union(Subset[] subsets, int x, int y) {
        int xroot = FindInSubset(subsets, x);
        int yroot = FindInSubset(subsets, y);

        if (subsets[xroot].rank < subsets[yroot].rank) {
            subsets[xroot].parent = yroot;
        } else if (subsets[xroot].rank > subsets[yroot].rank) {
            subsets[yroot].parent = xroot;
        } else {
            subsets[yroot].parent = xroot;
            subsets[xroot].rank++;
        }
    }

    int GetRoomIndex(IPoint point) {
        for (int index = 0; index < rooms.Count; index++) {
            Cell currentRoom = rooms.ElementAt(index);
            if (currentRoom.transform.position.x == point.X && currentRoom.transform.position.y == point.Y) {
                return index;
            }
        }
        return int.MaxValue;
    }

    List<WeightedEdge> CalculateMST(Delaunator delaunator) {
        List<WeightedEdge> allEdges = new List<WeightedEdge>();
        WeightedEdge[] mst = new WeightedEdge[rooms.Count];
        List<WeightedEdge> randomEdgesToInclude = new List<WeightedEdge>();
        Subset[] subsets = new Subset[rooms.Count];
        int mstIndex = 0;
        int i = 0;

        delaunator.ForEachTriangleEdge(edge => {
            allEdges.Add(new WeightedEdge(edge.P, edge.Q));
        });

        // Sort edges based on weight
        allEdges.Sort();

        // Init subsets
        for (int v = 0; v < rooms.Count; v++) {
            subsets[v] = new Subset {
                parent = v,
                rank = 0
            };
        }

        while (mstIndex < rooms.Count - 1) {
            WeightedEdge nextWeightedEdge = allEdges.ElementAt(i++);
            int x = FindInSubset(subsets, GetRoomIndex(nextWeightedEdge.src));
            int y = FindInSubset(subsets, GetRoomIndex(nextWeightedEdge.dest));
            if (x != y) {
                mst[mstIndex++] = nextWeightedEdge;
                Union(subsets, x, y);
            }
        }

        foreach (WeightedEdge weightedEdge in allEdges) {
            // Add some random edges to spice things up
            int randomValue = UnityEngine.Random.Range(0, 100);
            if (randomValue < 2 && !mst.Contains(weightedEdge)) {
                randomEdgesToInclude.Add(weightedEdge);
            }
        }

        randomEdgesToInclude.AddRange(mst);
        return randomEdgesToInclude;
    }

}
