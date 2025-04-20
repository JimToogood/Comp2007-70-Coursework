using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthbar : MonoBehaviour {
    [SerializeField] public Slider slider;
    [SerializeField] public Image image;

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
