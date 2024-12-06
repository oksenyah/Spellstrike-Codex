using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{

    Enemy character;

    [SerializeField] Transform targetTransform;

    [Header("Config")]
    [SerializeField] int numberOfRays = 45;
    [SerializeField] int numberOfDegrees = 360;
    private Vector3 spawnPosition;
    private Vector3 startingVector;
    private LayerMask charactersLayerMask;
    private RaycastHit2D[] results;
    private bool isStalking;

    delegate void AIState();
    AIState currentState;

    void Awake() {
        character = GetComponent<Enemy>();
	    charactersLayerMask = LayerMask.GetMask("Characters", "Walls");
        results = new RaycastHit2D[numberOfRays];
        startingVector = Quaternion.Euler(0, 0, Random.Range(0, 360)) * transform.up;
        spawnPosition = transform.position;
    }


    // Start is called before the first frame update
    void Start() {
        ChangeState(WanderState);
    }

    public void StartStalkingTarget(Transform targetTransform, bool isStalking) {
        this.isStalking = isStalking;
        this.targetTransform = targetTransform;
        this.ChangeState(StalkingState);
    }

    void SetTarget(Transform targetTransform) {
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
            return distanceToTarget <= character.GetSightDistance();
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

    void StalkingState() {
        if (targetTransform != null) {
            // Debug.Log("stalking...");
            character.MoveToward(targetTransform.position);
        } else {
            Debug.Log("Target is null. Wandering...");
            ChangeState(WanderState);
            return;
        }
    }

    void WanderState() {
        if (Vector3.Distance(character.transform.position, spawnPosition) < character.GetSightDistance()) {
                character.MoveToward(character.transform.position + new Vector3(Random.Range(-character.GetSightDistance(),character.GetSightDistance()),Random.Range(-character.GetSightDistance(),character.GetSightDistance()),0));
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
                // Debug.Log("Ray: " + i + ", Delta Angle: " + deltaAngle + ", Distance: " + character.GetSightDistance());
                // var raycastDirection = Quaternion.Euler(0, 0, i * deltaAngle) * transform.up * character.GetSightDistance();
                var raycastDirection = Quaternion.Euler(0, 0, i * deltaAngle) * startingVector * character.GetSightDistance();
                // Debug.Log("Raycast direction: " + raycastDirection);
                Debug.DrawRay(transform.position, raycastDirection, Color.gray);
                int hitCount = Physics2D.LinecastNonAlloc(transform.position, raycastDirection, results, charactersLayerMask);
                if (hitCount > 0) {
                    foreach (RaycastHit2D hit in results) {
                        // Hit was detected!
                        if (hit) {
                            // Debug.Log("Found Tag: " + hit.collider.tag);
                            // Debug.Log("Distance Between Enemy and Wizard: " + Vector3.Distance(character.transform.position, hit.collider.transform.position));
                            if (Vector3.Distance(character.transform.position, hit.collider.transform.position) <= character.GetSightDistance()) {
                                if (hit.collider.CompareTag("Wizard")) {
                                    Debug.Log("Enemy nearby!!!");
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
            if (!isStalking) {
                Debug.Log("Null transform since we are NOT stalking.");
                targetTransform = null;
            } else {
                Debug.Log("Not Updating Transform since we are stalking...");
            }
        }
    }

    void FixedUpdate() {
        FixedScanForTarget();
    }

}
