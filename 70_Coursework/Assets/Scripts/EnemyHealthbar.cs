using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthbar : MonoBehaviour {
    [SerializeField] public Slider slider;
    [SerializeField] public Image image;
    private Transform cameraTransform;

    private void Awake() {
        cameraTransform = Camera.main.transform;
    }

    private void FixedUpdate() {
        // Make healthbar always face the camera, regardless of enemy rotation
        transform.rotation = Quaternion.LookRotation(transform.position - cameraTransform.position);
    }

    public void SetMaxHealth(int health) {
        slider.maxValue = health;
        slider.value = health;
    }

    public void SetHealth(int health) {
        slider.value = health;

        // Make healthbar yellow when low health, red when very low health
        if (slider.value < (slider.maxValue / 3f)) {
            image.color = Color.red;
        } else if (slider.value <= (slider.maxValue / 1.75f)) {
            image.color = Color.yellow;
        } else {
            image.color = Color.green;
        }
    }
}
