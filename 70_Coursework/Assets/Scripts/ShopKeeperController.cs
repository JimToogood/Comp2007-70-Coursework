using UnityEngine;

public class ShopKeeperController : MonoBehaviour {
    [SerializeField] private GameObject combatUI;
    [SerializeField] private GameObject shopUI;
    [SerializeField] private Transform modelBody;
    
    private Transform playerTransform;
    private Vector3 lookDirection;
    private bool shopIsOpen = false;

    private void Awake() {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        combatUI.SetActive(true);
        shopUI.SetActive(false);
    }

    private void FixedUpdate() {
        // If within 10 units of the player
        // Player cant move when shop is open so no need to keep changing lookDirection
        if (Vector3.Distance(transform.position, playerTransform.position) < 10f && !shopIsOpen) {
            lookDirection = playerTransform.position - modelBody.transform.position;
            lookDirection.y = 0f;

            modelBody.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    public void ToggleShop() {
        shopIsOpen = !shopIsOpen;
        combatUI.SetActive(!shopIsOpen);
        shopUI.SetActive(shopIsOpen);
    }
}
