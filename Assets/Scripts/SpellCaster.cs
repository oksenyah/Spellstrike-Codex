using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpellCaster : MonoBehaviour {

    [Header("Prefabs")]
    [SerializeField] GameObject spellPrefab;
    [SerializeField] Spell.SpellType spellType;

    [Header("Spells")]
    [SerializeField] float cooldownTime = .25f;
    [SerializeField] float projectileSpeed = 10.0f;
    [SerializeField] float projectileSize = 0.1f;
    [SerializeField] int projectileCount = 1;
    [SerializeField] int spellDamage = 1;
    [SerializeField] int spellCost = 1;

    Wizard wizard;

    void Awake(){
        wizard = GetComponent<Wizard>();
    }

    bool coolingDown = false;

    public int CastSpell(Vector3 aimPosition) {

        if (spellPrefab == null) {
            return 0;
        }

        if(wizard.CurrentMana() < spellCost) {
            return 0;
        }

        if(coolingDown) {
            return 0;
        }
        Cooldown();
        Quaternion goalRotation = Quaternion.LookRotation(Vector3.forward, aimPosition - wizard.transform.position);

        for(int i = 0; i < projectileCount; i++) {
            GameObject newSpell = Instantiate(spellPrefab, wizard.transform.position, goalRotation);
            newSpell.GetComponent<Spell>().ConjureSpell(wizard, spellDamage);
            newSpell.transform.localScale = Vector3.one * projectileSize;

            newSpell.GetComponent<Rigidbody2D>().velocity = newSpell.transform.up * projectileSpeed;
            Destroy(newSpell,20);
        }
        return spellCost;
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

    public void SetSpellPrefab(GameObject spellPrefab) {
        this.spellPrefab = spellPrefab;
    }

    public void SetSpellType(Spell.SpellType spellType) {
        this.spellType = spellType;
    }

    void SetSpellDamage(int spellDamage) {
        this.spellDamage = spellDamage;
    }

    void SetSpellCost(int spellCost) {
        this.spellCost = spellCost;
    }

    void SetCooldownTime(float cooldownTime) {
        this.cooldownTime = cooldownTime;
    }
    
    void SetProjectileSpeed(float projectileSpeed) {
        this.projectileSpeed = projectileSpeed;
    }

    void SetProjectileSize(float projectileSize) {
        this.projectileSize = projectileSize;
    }

    void SetProjectileCount(int projectileCount) {
        this.projectileCount = projectileCount;
    }

    public void LearnSpell(Spell spell) {
        SetSpellType(spell.GetSpellType());
        SetSpellPrefab(Resources.Load("Prefabs/Spells/" + spell.GetSpellType()).GameObject());
        SetSpellDamage(spell.GetDamage());
        SetCooldownTime(spell.GetCooldown());
        SetSpellCost(spell.GetCost());
        SetProjectileSpeed(spell.GetSpeed());
        SetProjectileSize(spell.GetSize());
        SetProjectileCount(spell.GetCount());
    }
}
