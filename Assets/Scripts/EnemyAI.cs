using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{

    Enemy character;

    [SerializeField] string currentStateString = "IdleState";
    [SerializeField] Transform targetTransform;

    [Header("Config")]
    [SerializeField] float sightDistance = 5;

    delegate void AIState();
    AIState currentState;


    //trackers==================================================
    float stateTime = 0;
    bool justChangedState = false;
    Vector3 lastTargetPos;

    void Awake() {
        character = GetComponent<Enemy>();
    }


    // Start is called before the first frame update
    void Start()
    {
        ChangeState(IdleState);
    }

    public void SetTarget(Transform targetTransform) {
        this.targetTransform = targetTransform;
    }


    void ChangeState(AIState newAIState){
        currentState = newAIState;
        justChangedState = true;
    }

    bool CanSeeTarget(){
        if(targetTransform == null){
            return false;
        }

        return Vector3.Distance(character.transform.position, targetTransform.position) < sightDistance;
    }

    void IdleState(){
        if (stateTime == 0)
        {
            currentStateString = "IdleState";
        }

        if (CanSeeTarget())
        {
            ChangeState(AttackState);
            return;
        }
       
    }

    void AttackState(){
        if(Vector3.Distance(transform.position,targetTransform.position) > 0){
            character.MoveToward(targetTransform.position);
        }else{
            character.Stop();
        }
        
        if (stateTime == 0)
        {
            currentStateString = "AttackState";
        }

        if(!CanSeeTarget())
        {
            lastTargetPos = targetTransform.position;
            ChangeState(GetBackToTargetState);
            return;
        }
    }


    void GetBackToTargetState(){ //if we lose sight of the player, go back to the position where we last saw the player
        if (stateTime == 0)
        {
            currentStateString = "BackToTargetState";
        }

        character.MoveToward(lastTargetPos);

        if(stateTime < 2){
            return;
        }


        if (CanSeeTarget())
        {
            ChangeState(AttackState);
            return;
        }
        if(Vector3.Distance(character.transform.position, lastTargetPos) < 1){
            ChangeState(PatrolState);
            return;
        }
    }


    Vector3 patrolPos;
    Vector3 patrolPivot; //where we started Patrolling
    void PatrolState(){
        if(stateTime == 0){
            targetTransform = null;
            currentStateString = "PatrolState";
            patrolPivot = character.transform.position;
            patrolPos = character.transform.position + new Vector3(Random.Range(-sightDistance, sightDistance),Random.Range(-sightDistance, sightDistance));
        }

        character.MoveToward(patrolPos);

        if (CanSeeTarget())
        {
            ChangeState(AttackState);
            return;
        }

        if (Vector3.Distance(character.transform.position,patrolPos) < 1f){
            patrolPos = patrolPivot + new Vector3(Random.Range(-sightDistance, sightDistance), Random.Range(-sightDistance, sightDistance));
            return;
        }

    }

    void AITick(){
        if(justChangedState){
            stateTime = 0;
            justChangedState = false;
        }
        currentState();
        stateTime += Time.deltaTime;

    }

    void FixedUpdate(){

    }

    // Update is called once per frame
    void Update()
    {
        AITick();
    }

}
