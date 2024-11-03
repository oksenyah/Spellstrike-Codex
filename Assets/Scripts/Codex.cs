using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Codex : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other){
        if(other.CompareTag("Wizard")){
            Destroy(this.gameObject);
            SceneManager.LoadScene("MainMenu");
        }
    }
}
