using UnityEngine;
using UnityEngine.UI;

public class ShopHandler : MonoBehaviour {
    [SerializeField] private AudioClip buySound;
    [SerializeField] private UITextController UITextController;

    private AudioSource audioSource;
    private PlayerController player;

    private void Awake() {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        audioSource = player.GetComponent<AudioSource>();
    }

    public void MaxHealthButton() {
        if (player.ChangeGold(-25)) {
            UITextController.DisplayText("Bought +25 max health!");
            audioSource.PlayOneShot(buySound, 0.5f);
            player.ChangeMaxHealth(25);
        } else {
            UITextController.DisplayText("Not enough gold!");
        }
    }

    public void AmmoButton() {
        if (player.ChangeGold(-10)) {
            UITextController.DisplayText("Bought +10 bullets!");
            audioSource.PlayOneShot(buySound, 0.5f);
            player.ChangeMaxBullets(15);
        } else {
            UITextController.DisplayText("Not enough gold!");
        }
    }

    public void HealButton() {
        if (player.currentHealth < player.maxHealth) {
            if (player.ChangeGold(-15)) {
                UITextController.DisplayText("Bought a full heal!");
                audioSource.PlayOneShot(buySound, 0.5f);
                player.FullHeal();
            } else {
                UITextController.DisplayText("Not enough gold!");
            }
        } else {
            UITextController.DisplayText("You're already at full health!");
        }
    }
}
