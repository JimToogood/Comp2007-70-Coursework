using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthbar : MonoBehaviour {
    [SerializeField] private Slider slider;
    [SerializeField] private Image image;
    [SerializeField] public AnimationCurve animationCurve;
    [SerializeField, Range(0f, 10f)] public float animationSpeed;

    private float lerpTimer;
    private float targetHealth;

    private void Update() {
        // Animation for smooth healthbar
        if (Mathf.Abs(slider.value - targetHealth) > 0.01f) {
            lerpTimer += Time.deltaTime;
        
            slider.value = Mathf.Lerp(
                slider.value,
                targetHealth,
                // Find current value on curve based on lerp timer
                animationCurve.Evaluate(Mathf.Clamp(lerpTimer / animationSpeed, 0f, 1f))
            );
        } else {
            // Set health to target health after animation is finished to ensure final value is an int
            slider.value = targetHealth;
        }
    }

    public void SetMaxHealth(int health, int currentHealth = -1) {
        slider.maxValue = health;
        targetHealth = health;

        // If currentHealth not provided, set currentHealth to maxHealth
        if (currentHealth == -1) {
            slider.value = health;
        } else {
            slider.value = currentHealth;
        }
    }

    public void SetHealth(int health) {
        targetHealth = Mathf.Clamp(health, 0f, slider.maxValue);
        // Reset lerp timer
        lerpTimer = 0f;

        // Make healthbar yellow when low health, red when very low health
        if (targetHealth < (slider.maxValue / 3f)) {
            image.color = Color.red;
        } else if (targetHealth <= (slider.maxValue / 1.75f)) {
            image.color = Color.yellow;
        } else {
            image.color = Color.green;
        }
    }
}
