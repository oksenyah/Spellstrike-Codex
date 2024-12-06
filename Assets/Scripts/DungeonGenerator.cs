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
    private List<Cell> pathways = new List<Cell>();

    [Header("Audio")]
    [SerializeField] AudioSource welcomeToTheDungeonAudio;
    [SerializeField] AudioSource enemyDeathAudio;
    [Header("Generator Settings")]
    [SerializeField] int numberOfCells = 1;
    [SerializeField] float separatorForce = 1.0f;
    [SerializeField] float widthMax = 10;
    [SerializeField] float widthMin = 2;
    [SerializeField] float heightMax = 10;
    [SerializeField] float heightMin = 2;
    [SerializeField] float playerThickness = 0.75f;
    [SerializeField] float roomSpawnsEnemiesPercentage = 0.35f;
    [SerializeField] int minStartingEnemiesPerRoom = 1;
    [SerializeField] int maxStartingEnemiesPerRoom = 20;
    [SerializeField] float minEnemySpawnIntervalInSeconds = 1;
    [SerializeField] float maxEnemySpawnIntervalInSeconds = 60f;
    [Header("DungeonItems")]
    [SerializeField] GameObject dungeonBlockPrefab;
    [SerializeField] GameObject codexPrefab;
    [SerializeField] List<GameObject> staticRooms;

    void Awake() {
        spawnRadius = numberOfCells / ((float) Math.PI) * 0.3f;
        lehmerRandom = GetComponent<LehmerRandom>();
        dungeonBlocks.AddRange(staticRooms);
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
            dungeonBlock.GetComponent<Cell>().SetDimensions((float) lehmerRandom.GetNextDouble(widthMin, widthMax), (float) lehmerRandom.GetNextDouble(heightMin, heightMax));
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
                        dungeonBlock.GetComponent<SpriteRenderer>().color = Color.gray;
                    }
                    cell.ShiftPosition(separation);
                    // yield return new WaitForSeconds(0.01f);
                }

                if (overlapDetected) {
                    // Debug.Log("Overlaps detected. Sleeping...");
                    yield return new WaitForSeconds(0.0001f);
                }
            }

            List<IPoint> points = new List<IPoint>();
            foreach (GameObject dungeonBlock in dungeonBlocks) {
                Cell cell = dungeonBlock.GetComponent<Cell>();
                if (cell.IsRoomCandidate()) {
                    dungeonBlock.GetComponent<SpriteRenderer>().color = Color.grey; // Saturation could be increased based on difficulty of room
                    cell.BuildWalls();
                    rooms.Add(cell);
                    points.Add(new Point(cell.transform.position.x, cell.transform.position.y));
                } else {
                    // Clean up dungeon blocks that aren't rooms
                    Destroy(dungeonBlock);
                }
            }

            Delaunator delaunator = new Delaunator(points.ToArray());

            List<WeightedEdge> mstEdges = CalculateMST(delaunator);
            foreach (WeightedEdge weightedEdge in mstEdges) {
                if (weightedEdge != null) {
                    // Debug.Log("Weighted Edge: " + weightedEdge);
                    // Debug.Log("Source: " + weightedEdge.src + ", Destination: " + weightedEdge.dest);

                    // Generate Pathway Between Dungeon Cells
                    Cell sourceCell = GetRoomAtPoint(weightedEdge.src);
                    Cell destinationCell = GetRoomAtPoint(weightedEdge.dest);

                    Vector3 sourceTopBorder = sourceCell.GetBorderVector3("top");
                    Vector3 sourceBottomBorder = sourceCell.GetBorderVector3("bottom");
                    Vector3 sourceLeftBorder = sourceCell.GetBorderVector3("left");
                    Vector3 sourceRightBorder = sourceCell.GetBorderVector3("right");

                    Vector3 destinationTopBorder = destinationCell.GetBorderVector3("top");
                    Vector3 destinationBottomBorder = destinationCell.GetBorderVector3("bottom");
                    Vector3 destinationLeftBorder = destinationCell.GetBorderVector3("left");
                    Vector3 destinationRightBorder = destinationCell.GetBorderVector3("right");

                    if (sourceCell.IsOverlappingXAxisWith(destinationCell)) {
                        // Get Points Between XAxis Range and Connect Vertically
                        Vector3 sourceBorderVector = sourceCell.GetBorderConnectionVector3(destinationCell);
                        Vector3 destinationBorderVector = new Vector3(sourceBorderVector.x, destinationCell.GetBorderConnectionVector3(sourceCell).y);
                        float borderDistance = Vector3.Distance(sourceBorderVector, destinationBorderVector);
                        Debug.DrawLine(sourceBorderVector, destinationBorderVector, Color.blue, float.PositiveInfinity);

                        // Add Vertical Pathway
                        float minX = Math.Max(sourceLeftBorder.x, destinationLeftBorder.x);
                        float maxX = Math.Min(sourceRightBorder.x, destinationRightBorder.x);
                        float minY = Math.Min(sourceTopBorder.y, destinationTopBorder.y);
                        float maxY = Math.Max(sourceBottomBorder.y, destinationBottomBorder.y);

                        float xMidPoint = minX + ((maxX - minX) / 2);
                        float yMidPoint = minY + ((maxY - minY) / 2);
                        float width = maxX - minX;
                        float length = maxY - minY;

                        float pathThickness = playerThickness;
                        // Debug.Log("Comparing borderDistance " + borderDistance + " with desired thickness: " + pathThickness);
                        if (borderDistance <= pathThickness) {
                            // Tiny corridor, clear the entire thing out
                            // Debug.Log("borderDistance " + borderDistance + " is less than desired thickness. Expanding to width: " + width);
                            pathThickness = width - (0.05f); // Account for walls
                        } else {
                            width = pathThickness + (0.05f); // Account for walls
                        }

                        // Debug.Log("Pathway Dimensions, Width: " + width + ", Length: " + length + ", X: " + xMidPoint + ", Y: " + yMidPoint);

                        GameObject dungeonBlock = Instantiate(dungeonBlockPrefab, new Vector3(xMidPoint, yMidPoint, 0), Quaternion.identity);
                        dungeonBlock.GetComponent<SpriteRenderer>().color = Color.grey;
                        Cell cell = dungeonBlock.GetComponent<Cell>();
                        cell.transform.localScale = Vector3.zero; // Zero out the scale since we already have ref
                        cell.SetDimensions(width, length);
                        cell.BuildWalls();

                        // Punch out paths
                        sourceCell.AddDoor(sourceBorderVector, pathThickness);
                        destinationCell.AddDoor(destinationBorderVector, pathThickness);
                        cell.AddDoor(sourceBorderVector, pathThickness);
                        cell.AddDoor(destinationBorderVector, pathThickness);

                        pathways.Add(cell);
                    } else if (sourceCell.IsOverlappingYAxisWith(destinationCell)) {
                        // Get Random Points Between YAxis Range and Connect Horizontally
                        Vector3 sourceBorderVector = sourceCell.GetBorderConnectionVector3(destinationCell);
                        Vector3 destinationBorderVector = new Vector3(destinationCell.GetBorderConnectionVector3(sourceCell).x, sourceBorderVector.y);
                        float borderDistance = Vector3.Distance(sourceBorderVector, destinationBorderVector);
                        Debug.DrawLine(sourceBorderVector, destinationBorderVector, Color.blue, float.PositiveInfinity);

                        // Add Horizontal Pathway
                        float minX = Math.Max(sourceLeftBorder.x, destinationLeftBorder.x);
                        float maxX = Math.Min(sourceRightBorder.x, destinationRightBorder.x);
                        float minY = Math.Max(sourceBottomBorder.y, destinationBottomBorder.y);
                        float maxY = Math.Min(sourceTopBorder.y, destinationTopBorder.y);

                        float xMidPoint = minX + ((maxX - minX) / 2);
                        float yMidPoint = minY + ((maxY - minY) / 2);
                        float width = maxX - minX;
                        float length = maxY - minY;

                        float pathThickness = playerThickness;
                        
                        // Debug.Log("Comparing borderDistance " + borderDistance + " with desired thickness: " + pathThickness);
                        if (borderDistance <= pathThickness) {
                            // Tiny corridor, clear the entire thing out
                            // Debug.Log("borderDistance " + borderDistance + " is less than desired thickness. Expanding to length: " + length);
                            pathThickness = length - (0.05f); // Account for walls
                        } else {
                            length = pathThickness + (0.05f);
                        }

                        // Debug.Log("SOURCE Top: " + sourceTopBorder + ", Bottom: " + sourceBottomBorder + ", Left: " + sourceLeftBorder + ", Right: " + sourceRightBorder);
                        // Debug.Log("DESTIN Top: " + destinationTopBorder + ", Bottom: " + destinationBottomBorder + ", Left: " + destinationLeftBorder + ", Right: " + destinationRightBorder);
                        // Debug.Log("Pathway Dimensions, Width: " + width + ", Length: " + length + ", X: " + xMidPoint + ", Y: " + yMidPoint);

                        GameObject dungeonBlock = Instantiate(dungeonBlockPrefab, new Vector3(xMidPoint, yMidPoint, 0), Quaternion.identity);
                        dungeonBlock.GetComponent<SpriteRenderer>().color = Color.grey;
                        Cell cell = dungeonBlock.GetComponent<Cell>();
                        cell.transform.localScale = Vector3.zero; // Zero out the scale since we already have ref
                        cell.SetDimensions(width, length);
                        cell.BuildWalls();

                        // Punch out paths
                        // Debug.Log("Punching out door on source cell at Vector: " + sourceBorderVector);
                        sourceCell.AddDoor(sourceBorderVector, pathThickness);
                        // Debug.Log("Punching out door on destination cell at Vector: " + destinationBorderVector);
                        destinationCell.AddDoor(destinationBorderVector, pathThickness);
                        cell.AddDoor(sourceBorderVector, pathThickness);
                        cell.AddDoor(destinationBorderVector, pathThickness);

                        pathways.Add(cell);
                    } else {
                        // L-Connector Across Closest Corners
                        Debug.DrawLine(weightedEdge.src.ToVector3(), weightedEdge.dest.ToVector3(), Color.blue, float.PositiveInfinity);
                    }
                }
            }

            foreach (Cell room in rooms) {
                if (room.CanSpawnEnemies()) {

                    if (UnityEngine.Random.Range(0.0f, 1f) <= roomSpawnsEnemiesPercentage || room.MustSpawnEnemies()) {
                        // Debug.Log("Starting to spawn enemies in room: " + room.transform.position);
                        EnemySpawner enemySpawner = room.GetComponent<EnemySpawner>();
                        enemySpawner.SetEnemyDeathAudio(enemyDeathAudio);
                        enemySpawner.SetStartingEnemiesCount(UnityEngine.Random.Range(minStartingEnemiesPerRoom, maxStartingEnemiesPerRoom));
                        enemySpawner.SetSpawnIntervalInSeconds(UnityEngine.Random.Range(minEnemySpawnIntervalInSeconds, maxEnemySpawnIntervalInSeconds));
                        enemySpawner.SpawnStartingEnemies();
                        enemySpawner.SpawnEnemies();
                    }
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

    Cell GetRoomAtPoint(IPoint point) {
        for (int index = 0; index < rooms.Count; index++) {
            Cell currentRoom = rooms.ElementAt(index);
            if (currentRoom.transform.position.x == point.X && currentRoom.transform.position.y == point.Y) {
                return currentRoom;
            }
        }
        return null;
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
