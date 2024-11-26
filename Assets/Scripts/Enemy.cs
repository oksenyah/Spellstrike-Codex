using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    bool isCoolingDown = false;
    AudioSource deathAudio;

    [Header("Expendables")]
    [SerializeField] int health = 10;

    [Header("Stats")]
    [SerializeField] float speed = 0.5f;
    [SerializeField] int damage = 1;
    [SerializeField] float attackIntervalInSeconds = 1.0f;
    [Header("Loot")]
    [SerializeField] List<GameObject> lootPrefabs;
    [SerializeField] float lootDropRate = 0.05f;
    


    void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Cooldown(){
        isCoolingDown = true;
        StartCoroutine(CoolingDownRoutine());
        IEnumerator CoolingDownRoutine(){
            yield return new WaitForSeconds(attackIntervalInSeconds);
            isCoolingDown = false;
        }
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

    public float CurrentHealth() {
        return health;
    }

    public void TakeDamage(int damage) {
        health -= damage;
        if (health <= 0) {
            Die();
        }
    }

    public void Die() {
        deathAudio.Play();
        Destroy(this.gameObject);
        RandomlyDropLoot();
    }

    void RandomlyDropLoot() {
        if (Random.Range(0.0f, 1f) <= lootDropRate ) {
            Instantiate(lootPrefabs[Random.Range(0, lootPrefabs.Count)], this.transform.position, Quaternion.identity);
        }
    }

    void Attack(Wizard wizard) {
        if (!isCoolingDown) {
            wizard.TakeDamage(damage);
            Cooldown();
        }
    }

    public void SetDeathAudio(AudioSource audioSource) {
        this.deathAudio = audioSource;
    }

    void OnCollisionStay2D(Collision2D collision) { 
        if (collision.gameObject.CompareTag("Wizard")) {
            Attack(collision.gameObject.GetComponent<Wizard>());
        }
    }

    // Pathfinding

    float HeuristicCostEstimate(Vector2 start, Vector2 goal) {
        // AKA Manhattan Distance
        return Mathf.Abs(start.x -goal.x) + Mathf.Abs(start.y - goal.y);
    }

}
