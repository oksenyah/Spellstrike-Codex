using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell : MonoBehaviour
{

    public enum SpellType { Fireball, Snowball };

    [Header("Core")]
	[SerializeField] SpellType spellType = SpellType.Fireball;
    [SerializeField] bool isLoot = false;
    [SerializeField] int spellDamage = 1;
    [SerializeField] int spellCost = 1;
    [SerializeField] float cooldownTime = .25f;
    [SerializeField] float projectileSpeed = 10.0f;
    [SerializeField] float projectileSize = 0.1f;
    [SerializeField] int projectileCount = 1;
    [Header("Effects")]
    [SerializeField] bool canPassThroughEnemies = true;
    [SerializeField] float effectDurationInSeconds = float.MaxValue;
    [SerializeField] float effectScaleEnemyMovementPercentage = 1.0f;
    [SerializeField] Color effectEnemyColor;

    [Header("Audio")]
    [SerializeField] AudioSource castAudio;
    [SerializeField] AudioSource hitAudio;

    Wizard sourcerer;
    int damage;

    public void ConjureSpell(Wizard wizard, int damage) {
        this.sourcerer = wizard;
        this.damage = damage;
        castAudio.Play();
    }

    public SpellType GetSpellType() {
        return spellType;
    }

    public int GetDamage() {
        return spellDamage;
    }

    public int GetCost() {
        return spellCost;
    }

    public float GetCooldown() {
        return cooldownTime;
    }

    public float GetSpeed() {
        return projectileSpeed;
    }

    public float GetSize() {
        return projectileSize;
    }

    public int GetCount() {
        return projectileCount;
    }

    public float GetEffectDurationInSeconds() {
        return effectDurationInSeconds;
    }

    public float GetEffectScaleEnemyMovementPercentage() {
        return effectScaleEnemyMovementPercentage;
    }

    public Color GetEffectEnemyColor() {
        return effectEnemyColor;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Enemy")) {
            other.GetComponent<Enemy>().TakeDamage(this.damage, this.sourcerer);
            other.GetComponent<SpellEffect>().ActivateEffect(this);
            hitAudio.Play();
            if (!canPassThroughEnemies) {
                Destroy(this.gameObject);
            }
        } else if (other.CompareTag("Wizard")) {
            if (isLoot) {
                other.GetComponent<SpellCaster>().LearnSpell(this);
                Destroy(this.gameObject);
            } else {
                // if (other.GetComponent<Wizard>() != sourcerer) {
                if (!ReferenceEquals(other.GetComponent<Wizard>(), sourcerer)) {
                    other.GetComponent<Wizard>().TakeDamage(spellDamage);
                    Destroy(this.gameObject);
                }
            }
        } else if(other.CompareTag("Wall")) {
            Destroy(this.gameObject);
        }
    }
}
