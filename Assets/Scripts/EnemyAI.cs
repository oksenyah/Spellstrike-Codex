using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{

    Enemy character;

    [SerializeField] Transform targetTransform;

    [Header("Config")]
    [SerializeField] float sightDistance = 5;
    [SerializeField] int numberOfRays = 45;
    [SerializeField] int numberOfDegrees = 360;
    private Vector3 spawnPosition;
    private Vector3 startingVector;
    private LayerMask wizardLayerMask;
    private RaycastHit2D[] results;

    delegate void AIState();
    AIState currentState;

    void Awake() {
        character = GetComponent<Enemy>();
	    wizardLayerMask = LayerMask.GetMask("Wizard");
        results = new RaycastHit2D[numberOfRays];
        startingVector = Quaternion.Euler(0, 0, Random.Range(0, 360)) * transform.up;
        spawnPosition = transform.position;
    }


    // Start is called before the first frame update
    void Start() {
        ChangeState(WanderState);
    }

    public void SetTarget(Transform targetTransform) {
        this.targetTransform = targetTransform;
    }

    void ChangeState(AIState newAIState){
        currentState = newAIState;
    }

     bool CanSeeTarget() {
        if (targetTransform == null) {
            return false;
        } else {
            float distanceToTarget = Vector3.Distance(character.transform.position, targetTransform.position);
            // Debug.Log("Distance to target: " + distanceToTarget);
            return distanceToTarget <= sightDistance;
        }
    }

    void IdleState(){
        if (CanSeeTarget()) {
            ChangeState(AttackState);
            return;
        }
    }

    void AttackState() {
        if (targetTransform != null) {
            character.MoveToward(targetTransform.position);
        } else {
            Debug.Log("Target is null. Wandering...");
            ChangeState(WanderState);
            return;
        }
        
    }

    void WanderState() {
        if (Vector3.Distance(character.transform.position, spawnPosition) < sightDistance) {
                character.MoveToward(character.transform.position + new Vector3(Random.Range(-sightDistance,sightDistance),Random.Range(-sightDistance,sightDistance),0));
        } else {
            character.MoveToward(spawnPosition); // Go towards home if we wander too far.
        }

        if (CanSeeTarget()) {
            // Debug.Log("Can see target, attacking!");
            ChangeState(AttackState);
            return;
        }
    }

    void AITick() {
        currentState();
    }

    // Update is called once per frame
    void Update() {
        AITick();
    }

    void FixedScanForTarget(){
        // TODO: What if there are multiple targets found?
        float deltaAngle = numberOfDegrees / numberOfRays;
        bool targetFound = false;

        if (targetTransform == null) {
            // Look for a new target
            for (int i = 0; i < numberOfRays; i++) {
                // Debug.Log("Ray: " + i + ", Delta Angle: " + deltaAngle + ", Distance: " + sightDistance);
                // var raycastDirection = Quaternion.Euler(0, 0, i * deltaAngle) * transform.up * sightDistance;
                var raycastDirection = Quaternion.Euler(0, 0, i * deltaAngle) * startingVector * sightDistance;
                // Debug.Log("Raycast direction: " + raycastDirection);
                Debug.DrawRay(transform.position, raycastDirection, Color.gray);
                int hitCount = Physics2D.LinecastNonAlloc(transform.position, raycastDirection, results, wizardLayerMask);
                if (hitCount > 0) {
                    foreach (RaycastHit2D hit in results) {
                        // Debug.Log("Collision detected!");
                        // Hit was detected!
                        if (hit) {
                            if (Vector3.Distance(character.transform.position, hit.collider.transform.position) <= sightDistance) {
                                if (hit.collider.CompareTag("Loot")) {
                                    // Check if loot is unclaimed
                                    Debug.Log("Loot found!");
                                    targetTransform = hit.collider.transform;
                                    targetFound = true;
                                    Debug.DrawLine(character.transform.position, hit.transform.position, Color.blue);
                                    break;
                                } else if (hit.collider.CompareTag("Wizard")) {
                                    // Debug.Log("Enemy nearby!!!");
                                    targetTransform = hit.collider.transform;
                                    targetFound = true;
                                    Debug.DrawLine(character.transform.position, hit.transform.position, Color.green);
                                    break;
                                }
                            }
                        }
                    }
                }

                if (targetFound) {
                    break;
                }
            }
        } else {
            targetFound = true;
        }
        
        if (!targetFound) {
            targetTransform = null;
        }
    }

    void FixedUpdate() {
        FixedScanForTarget();
    }

}
