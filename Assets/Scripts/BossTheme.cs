using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTheme : MonoBehaviour
{
    [SerializeField] AudioSource bossTheme;
    bool activated = false;

    void OnTriggerEnter2D(Collider2D other){
        if(other.CompareTag("Wizard") && !activated) {
            bossTheme.Play();
            activated = true;
        }
    }
}
