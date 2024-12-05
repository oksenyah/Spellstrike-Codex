using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell : MonoBehaviour
{

    [Header("Audio")]
    [SerializeField] AudioSource castAudio;
    [SerializeField] AudioSource hitAudio;

    int damage;

    public void ConjureSpell(int damage) {
        this.damage = damage;
        castAudio.Play();
    }

    void OnTriggerEnter2D(Collider2D other){
        if(other.CompareTag("Enemy")){
            other.GetComponent<Enemy>().TakeDamage(this.damage);
            hitAudio.Play();
            // Destroy(this.gameObject);
        } else if(other.CompareTag("Wall")){
            Destroy(this.gameObject);
        }
    }
}
