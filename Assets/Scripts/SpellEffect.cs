using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellEffect : MonoBehaviour
{
    private class DurableEffect {
        public float startTimeInSeconds = 0;
        public float maxDurationInSeconds = 0;
        public float currentDurationInSeconds = 0;
        public float scaleMovement = 1f;
        public Color color;
    }

    [SerializeField] float intervalInSeconds = 0.25f;

    private List<DurableEffect> activeEffects = new List<DurableEffect>();
    private static int EFFECT_REMOVAL_CACHE_SIZE = 250;
    private DurableEffect[] effectsToRemove = new DurableEffect[EFFECT_REMOVAL_CACHE_SIZE];
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Movement movement;

    private Color originalColor;
    private float originalMovementSpeed;
    private bool isApplyingEffects = false;
    private float worldTime = 0f;

    void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<Movement>();

        originalColor = spriteRenderer.color;
        originalMovementSpeed = movement.GetMovementSpeed();
    }

    void ApplyEffects() {
        isApplyingEffects = true;
        StartCoroutine(ApplyEffectsRoutine());
        IEnumerator ApplyEffectsRoutine() {
            while (activeEffects.Count > 0) {
                int effectCleanupCount = 0;
                foreach (DurableEffect effect in activeEffects) {
                    if (!ApplyEffect(effect)) {
                        effectsToRemove[effectCleanupCount++] = effect;
                    }
                }
                for (int i = 0; i < effectCleanupCount; i++) {
                    activeEffects.Remove(effectsToRemove[i]);
                }
                yield return new WaitForSeconds(intervalInSeconds);
            }
            isApplyingEffects = false;
        }
    }

    bool ApplyEffect(DurableEffect effect) {
        bool isActive = true;
        effect.currentDurationInSeconds = worldTime - effect.startTimeInSeconds;
        Debug.Log("effect.currentDurationInSeconds: " + effect.currentDurationInSeconds);
        if (effect.currentDurationInSeconds < effect.maxDurationInSeconds) {
            // Apply effect
            if (effect.color != Color.clear) {
                spriteRenderer.color = effect.color;
            }
            if (movement != null) {
                movement.SetMovementSpeed(originalMovementSpeed * effect.scaleMovement);
            }
        } else {
            spriteRenderer.color = originalColor;
            if (movement != null) {
                movement.SetMovementSpeed(originalMovementSpeed);
            }
            Debug.Log("Effect is no longer active!");
            isActive = false;
        }
        return isActive;
    }

    public void ActivateEffect(Spell spell) {
        DurableEffect effect = new DurableEffect();
        effect.startTimeInSeconds = worldTime;
        effect.maxDurationInSeconds = spell.GetEffectDurationInSeconds();
        effect.scaleMovement = spell.GetEffectScaleEnemyMovementPercentage();
        effect.color = spell.GetEffectEnemyColor();

        activeEffects.Add(effect);
    }

    public void FixedUpdate() {
        worldTime += Time.fixedDeltaTime;
        if (!isApplyingEffects) {
            if (activeEffects.Count > 0) {
                Debug.Log("Applying effects.");
                ApplyEffects();
            }
        }
    }
}
