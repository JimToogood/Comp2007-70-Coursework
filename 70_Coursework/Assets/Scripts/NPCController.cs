using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour {
    private PlayerController player;
    private NavMeshAgent navMeshAgent;
    private Animator animator;
    private float movementCooldown = -10f;
    private int randomInterval;

    private void Awake() {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }

    private void FixedUpdate() {
        // Turn NPC AI off when player exits safe zone
        if (player.inSafeZone) { 
            if (Time.time - movementCooldown >= randomInterval) {
                movementCooldown = Time.time;
                randomInterval = Random.Range(1, 11);

                navMeshAgent.destination = GetNewPosition(transform.position);
            }

            // If moving, play walking animation
            animator.SetBool("IsMoving", navMeshAgent.velocity.magnitude > 0.5f);
        }
    }

    private static Vector3 GetNewPosition(Vector3 currentLocation) {
        Vector3 randomDirection = Random.insideUnitSphere * 20f;    // Select random position within a 20 unit radius
        randomDirection.y = currentLocation.y;                      // Ensure y coord is not affected

        return currentLocation + randomDirection;
    }
}
