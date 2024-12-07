using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{

    Movement movement;

    void Awake() {
        movement = GetComponent<Movement>();
    }

    public void MoveToward(Vector3 position) {
        movement.Move(position - transform.position);
    }

}
