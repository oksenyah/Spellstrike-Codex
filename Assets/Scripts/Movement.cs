using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    [SerializeField] float speed = 0.5f;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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

    public float GetMovementSpeed() {
        return this.speed;
    }

    public void SetMovementSpeed(float movementSpeed) {
        this.speed = movementSpeed;
    }
}
