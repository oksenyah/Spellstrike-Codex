using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootBox : MonoBehaviour {

    [Header("Upgrade")]
    [SerializeField] int upgradePoints = 1;

    void OnTriggerEnter2D(Collider2D other){
        if(other.CompareTag("Wizard")){
            other.GetComponent<Wizard>().UpgradeRandomStat(this.upgradePoints);
            Destroy(this.gameObject);
        }
    }
}
