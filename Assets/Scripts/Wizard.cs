using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Wizard : MonoBehaviour
{

    [Header("Expendables")]
    [SerializeField] int health = 10;
    [SerializeField] float mana = 10f;

    [Header("Stats")]
    [SerializeField] int maxHealth = 10;
    [SerializeField] int maxMana = 10;
    [SerializeField] float manaRestoreAmount = 0.1f;
    [SerializeField] float movementSpeed = 1;

    [Header("Audio")]
    [SerializeField] AudioSource hitAudio;
    [SerializeField] AudioSource upgradeAudio;

    Rigidbody2D rb;
    SpellCaster spellCaster;
    SpriteRenderer spriteRenderer;
    UnityEngine.Vector2 movement = UnityEngine.Vector2.zero;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        spellCaster = GetComponent<SpellCaster>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate(){
        rb.MovePosition(rb.position + movementSpeed * Time.fixedDeltaTime * movement);
        RestoreMana();
    }

    public void Move(UnityEngine.Vector2 newMovement, bool flipX) {
        spriteRenderer.flipX = flipX;
        movement = newMovement;
    }

    public float CurrentHealth() {
        return health;
    }

    public float MaxHealth() {
        return maxHealth;
    }

    public float CurrentMana() {
        return mana;
    }

    public float MaxMana() {
        return maxMana;
    }

    public void CastSpell(UnityEngine.Vector3 aimPosition) {
        int manaCost = spellCaster.CastSpell(aimPosition);
        mana -= manaCost;
    }

    public void RestoreMana() {
        float newManaLevel = mana + manaRestoreAmount;
        if (newManaLevel > maxMana) {
            mana = maxMana;
        } else {
            mana = newManaLevel;
        }
    }

    public void UpgradeRandomStat(int upgradePoints) {
        upgradeAudio.Play();
        maxHealth += upgradePoints;
        health += upgradePoints;
        maxMana += upgradePoints;
        manaRestoreAmount += upgradePoints;
        movementSpeed += upgradePoints;
        spellCaster.UpgradeSpellDamage(upgradePoints);
    }

    public void TakeDamage(int damage) {
        health -= damage;
        hitAudio.Play();
        if (health <= 0) {
            Die();
        }
    }

    void Die() {
        SceneManager.LoadScene("MainMenu");
    }
}
