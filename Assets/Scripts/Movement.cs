using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] float speed = 0.5f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Pathfinder pathfinder;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        pathfinder = GetComponent<Pathfinder>();
    }

    public void Move(Vector2 offset) {
        if (offset != Vector2.zero) {
            offset.Normalize();
            Vector2 vel = offset *= speed;

            rb.velocity = vel;

            if (offset.x < 0) {
                spriteRenderer.flipX = true;
            } else {
                spriteRenderer.flipX = false;
            }
        } else {
            Stop();
        }
    }

    public void Stop() {
        rb.velocity = Vector2.zero;
    }

    public void MoveToward(Vector3 position) {
        Move(position - transform.position);
    }

    public float GetMovementSpeed() {
        return this.speed;
    }

    public void SetMovementSpeed(float movementSpeed) {
        this.speed = movementSpeed;
    }

    void FixedUpdate() {
        MoveToward(pathfinder.GetPathPosition());
        // if (pathfinder.HasTarget()) {
        //     if (Vector3.Distance(transform.position, pathfinder.GetPathPosition()) < 0.05f) {
        //         Debug.Log("Getting next path position");
        //         pathfinder.NextPathPosition();
        //     } else {
        //         Debug.Log("Moving toward path position: " + pathfinder.GetPathPosition());
        //         MoveToward(pathfinder.GetPathPosition());
        //     }
        // } else {
        //     Debug.Log("Pathfinder does not have target...");
        // }
    }
}
