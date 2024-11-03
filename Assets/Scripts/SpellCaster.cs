using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellCaster : MonoBehaviour
{

    [Header("Prefabs")]
    [SerializeField] GameObject spellPrefab;

    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [Range(0,1)]
    [SerializeField] float pitchRange = .2f;

    [Header("Spells")]
    [SerializeField] float cooldownTime = .25f;
    [SerializeField] float projectileSpeed = 10.0f;
    [SerializeField] float projectileSize = 0.1f;
    [SerializeField] int projectileCount = 1;
    [SerializeField] int spellDamage = 1;
    [SerializeField] int manaCost = 1;

    Wizard wizard;

    void Awake(){
        wizard = GetComponent<Wizard>();
    }

    bool coolingDown = false;

    public int CastSpell(Vector3 aimPosition){

        if(wizard.CurrentMana() < manaCost){
            return 0;
        }

        if(coolingDown){
            return 0;
        }
        Cooldown();
        Quaternion goalRotation = Quaternion.LookRotation(Vector3.forward, aimPosition - wizard.transform.position);

        for(int i = 0; i<projectileCount; i++){
            GameObject newSpell = Instantiate(spellPrefab, wizard.transform.position, goalRotation);
            newSpell.GetComponent<Spell>().ConjureSpell(spellDamage);
            newSpell.transform.localScale = Vector3.one * projectileSize;

            newSpell.GetComponent<Rigidbody2D>().velocity = newSpell.transform.up * projectileSpeed;
            Destroy(newSpell,20);
        }
        return manaCost;
    }

    void Cooldown(){
        coolingDown = true;
        StartCoroutine(CoolingDownRoutine());
        IEnumerator CoolingDownRoutine(){
            yield return new WaitForSeconds(cooldownTime);
            coolingDown = false;
        }
    }

    public void UpgradeSpellDamage(int upgradePoints) {
        spellDamage += upgradePoints;
    }

}
