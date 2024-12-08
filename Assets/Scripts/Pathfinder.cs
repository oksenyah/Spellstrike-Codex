using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{

    private class WeightedNode : System.IComparable<WeightedNode> { 
        public Vector3 position;
        public WeightedNode parent;
        public float weight; // heuristic

        public float movementCost;
        public float distanceToTarget;

        public WeightedNode(Vector3 position) {
            this.position = position;
            this.weight = 0;
        }
  
        // Comparator function used for sorting nodes based on their weight
        public int CompareTo(WeightedNode otherNode) {
            if (this.weight <  otherNode.weight) return -1;
            if (this.weight == otherNode.weight) return 0;
            return 1;
        } 
    }

    [Header("Core")]
    [SerializeField] float nodeSearchDistance = 0.1f;
    [SerializeField] int numberOfRays = 8;
    [SerializeField] float pathGenerationInteravalInSeconds = 0.25f;
    [SerializeField] float targetDistanceThreshold = 0.25f;
    [SerializeField] int limit = 10;

    private Vector3 targetPosition;
    private List<WeightedNode> nodesToTarget = new List<WeightedNode>();
    private LayerMask charactersLayerMask;
    private RaycastHit2D[] results;
    private bool isCoolingDown = false;
    private bool isGeneratingPath = false;

    void Awake() {
        charactersLayerMask = LayerMask.GetMask("Characters", "Walls");
        results = new RaycastHit2D[numberOfRays];
    }

    void PathfindToTarget(Vector3 target) {
        if (DirectPathIsBlocked(transform.position, target)) {
            // if (!isCoolingDown) {
            //     GeneratePathToTarget(target);
            // }
            GeneratePathToTarget(target);
        } else if (nodesToTarget.Count == 0 || nodesToTarget.Last().position != target) {
            nodesToTarget = new List<WeightedNode>
            {
                new WeightedNode(target)
            };
        }
    }

    void Cooldown() {
        isCoolingDown = true;
        StartCoroutine(CoolingDownRoutine());
        IEnumerator CoolingDownRoutine() {
            yield return new WaitForSeconds(pathGenerationInteravalInSeconds);
            isCoolingDown = false;
        }
    }

    private bool DirectPathIsBlocked(Vector3 from, Vector3 target) {
        
        // Vector3 direction = target - from;
        // int hitCount = Physics2D.LinecastNonAlloc(from, target, results, charactersLayerMask);
        // bool pathIsBlocked = 0 < hitCount;

        // if (pathIsBlocked) {
        //     // // Debug.Log("Path is blocked between " +  from + ", and target: " + target);
        //     // Debug.DrawLine(from, target, Color.cyan, 1);
        // } else {
        //     // // Debug.Log("Path is NOT blocked between " +  from + ", and target: " + target);
        //     // Debug.DrawRay(from, target, Color.blue, 3);
        // }

        bool pathIsBlocked;
        Vector3 direction = target - from;
        RaycastHit2D hit = Physics2D.Linecast(from, target, charactersLayerMask);
        if (hit.collider == null) {
            pathIsBlocked = false;
            // Debug.DrawLine(from, target, Color.blue, 5);
        } else {
            pathIsBlocked = true;
            // // Debug.DrawLine(from, hit.point, Color.cyan, 5);
        }

        return pathIsBlocked;
    }

    private void GeneratePathToTarget(Vector3 target) {
        // // Debug.Log("Generating Path to target: " + target);

        Cooldown();
        StartCoroutine(AStarRoutine());
        IEnumerator AStarRoutine() {
            
            isGeneratingPath = true;
            int count = 0;

            // A* search path to target, and store nodes to target in list.
            Dictionary<Vector3, WeightedNode> bestNodes = new Dictionary<Vector3, WeightedNode>();

            // Init Open/Closed node lists
            List<WeightedNode> openNodes = new List<WeightedNode>();
            List<Vector3> closedPositions = new List<Vector3>();

            // put the starting node on the open list (you can leave its weight at zero)
            WeightedNode startingNode = new WeightedNode(transform.position);
            startingNode.distanceToTarget = Vector3.Distance(transform.position, target);
            startingNode.movementCost = 0;
            startingNode.weight = startingNode.distanceToTarget + startingNode.movementCost;
            openNodes.Add(new WeightedNode(transform.position));

            bestNodes[startingNode.position] = startingNode;

            while (openNodes.Count > 0 && count < limit) {
                limit++;
                // While there are open nodes, process them
                // find the node with the least weight on the open list
                openNodes.Sort();
                WeightedNode currentNode = openNodes[0];
                openNodes.RemoveAt(0);

                // // Debug.Log("Processing Current Open Node: " + currentNode.position);

                // Current node is close enough to target
                // If close enough to target, we consider it reached
                if (Vector3.Distance(currentNode.position, target) < targetDistanceThreshold) {
                    // // Debug.Log("currentNode " + currentNode.position + " is close to the target!");
                    WeightedNode targetNode = new WeightedNode(target);
                    targetNode.parent = currentNode;

                    nodesToTarget = ReconstructPath(targetNode);
                    isGeneratingPath = false;
                    yield break;
                }

                closedPositions.Add(currentNode.position);

                // Search for available nodes around position
                List<Vector3> availableVectors = ScanForVectors(currentNode.position);
                foreach (Vector3 availableVector in availableVectors) {

                    if (closedPositions.Contains(availableVector)) {
                        continue;
                    }

                    WeightedNode availableNode = new WeightedNode(availableVector);
                    // Calculate movement cost by combining costs from parent plus current distance between node and parent
                    availableNode.movementCost = currentNode.movementCost + Vector3.Distance(currentNode.position, availableVector);
                    availableNode.distanceToTarget = Vector3.Distance(availableVector, target);
                    availableNode.weight = availableNode.movementCost + availableNode.distanceToTarget;
                    availableNode.parent = currentNode;

                    if (bestNodes.TryGetValue(availableVector, out WeightedNode existingNode)) {
                        if (existingNode.weight <= availableNode.weight) {
                            // We already have a better path to this node
                            continue;
                        }
                    }

                    bestNodes[availableVector] = availableNode;

                    // If not already in openNodes, add it
                    if (!openNodes.Any(n => n.position == availableVector)) {
                        openNodes.Add(availableNode);
                    }
                    
                }
                // // Debug.Log("Adding qNextNode " + currentNode.position + " to closed nodes");
                yield return null;
            }
            // If we reach here, no path was found
            nodesToTarget = ReconstructPath(new WeightedNode(target));
            isGeneratingPath = false;
            // // Debug.Log("No path to target found!");
        }
    }

    private List<Vector3> ScanForVectors(Vector3 startingVector) {
        // // // Debug.Log("Scanning for vectors from starting vector: " + startingVector);

        float deltaAngle = 360 / numberOfRays;
        List<Vector3> availableVectors = new List<Vector3>();

        for (int i = 0; i < numberOfRays; i++) {
            //  // // Debug.Log("Ray: " + i + ", Delta Angle: " + deltaAngle + ", Distance: " + nodeSearchDistance);
            // Vector3 raycastDirection = Quaternion.Euler(0, 0, i * deltaAngle) * startingVector * nodeSearchDistance;
            // Vector3 raycastDirection = Quaternion.Euler(0, 0, i * deltaAngle) * startingVector;
            Vector3 raycastDirection = Quaternion.Euler(0, 0, i * deltaAngle) * Vector3.right * nodeSearchDistance;
            Vector3 targetVector = startingVector + raycastDirection;
            // // // Debug.Log("Raycast direction: " + raycastDirection);
            // Debug.DrawRay(startingVector, raycastDirection, Color.gray, 1);

            // int hitCount = Physics2D.LinecastNonAlloc(startingVector, raycastDirection, results, charactersLayerMask);
            // if (hitCount == 0) {
            //     // Debug.DrawLine(startingVector, targetVector, Color.green);
            //     availableVectors.Add(targetVector);
            // } else {
            //     // Debug.DrawLine(startingVector, results[0].transform.position, Color.red);
            // }

            RaycastHit2D hit = Physics2D.Raycast(startingVector, raycastDirection, nodeSearchDistance, charactersLayerMask);
            if (hit.collider == null) {
                availableVectors.Add(targetVector);
                // Debug.DrawLine(startingVector, targetVector, Color.green, 1);
            } else {
                // // Debug.DrawLine(startingVector, hit.point, Color.blue, 1);
                // Debug.DrawLine(startingVector, targetVector, Color.blue, 1);
            }
        }

        return availableVectors;
    }

    private List<WeightedNode> ReconstructPath(WeightedNode node) {
        List<WeightedNode> path = new List<WeightedNode>();
        while (node != null) {
            path.Insert(0, node);
            node = node.parent;
        }
        // // Debug.Log("Path to target, Count: " + path.Count);
        foreach (WeightedNode weightedNode in path) {
            // // Debug.Log("Path Node: " + weightedNode.position);
        }
        return path;
    }

    public void SetPathTarget(Vector3 targetPosition) {
        // if (this.targetPosition != targetPosition) {
        //     this.targetPosition = targetPosition;
        //     PathfindToTarget(targetPosition);
        // }
        this.targetPosition = targetPosition;
        // PathfindToTarget(targetPosition);
    }

    public Vector3 GetPathTarget() {
        return this.targetPosition;
    }

    public bool HasTarget() {
        return nodesToTarget.Count > 0;
    }

    public Vector3 GetPathPosition() {
        if (nodesToTarget.Count > 0) {
            return nodesToTarget[0].position;
        }
        return targetPosition;
    }

    public void NextPathPosition() {
        if (nodesToTarget.Count > 0) {
            for (int i = 0; i < nodesToTarget.Count; i++) {
                // // Debug.Log("Node[" + i + "]: " + nodesToTarget[i].position);
            }
            // // Debug.Log("Removing Node at index 0. Current count: " + nodesToTarget.Count);
            nodesToTarget.RemoveAt(0);
            // // Debug.Log("Removed Node at index 0. New count: " + nodesToTarget.Count);
        }
    }

    void FixedUpdate() {
        if (Vector3.Distance(transform.position, GetPathPosition()) < 0.05f) {
            // // Debug.Log("Getting next path position");
            NextPathPosition();
        }

        if (!isGeneratingPath) {
            PathfindToTarget(targetPosition);
        }
    }

}
