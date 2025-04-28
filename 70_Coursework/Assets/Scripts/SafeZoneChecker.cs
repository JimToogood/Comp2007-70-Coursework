using UnityEngine;

public class SafeZoneChecker : MonoBehaviour {
    [SerializeField] private UITextController UITextController;
    private PlayerController player;

    private void Awake() {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }
    
    private void OnTriggerEnter(Collider other) {
        // If Player enters Safe Zone
        if (other.CompareTag("Player")) {
            UITextController.DisplayText("Entering Safe Zone...");
            player.inSafeZone = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        // If Player exits Safe Zone
        if (other.CompareTag("Player")) {
            UITextController.DisplayText("Exiting Safe Zone, be aware!");
            player.inSafeZone = false;
        }
    }
}
