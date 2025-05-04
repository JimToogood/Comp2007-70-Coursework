using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

public class EnemyController : MonoBehaviour {
    [SerializeField, Range(0, 250)] private int maxHealth = 100;

    public int droppedGold;
    public PlayerController playerController;
    public EnemyHealthbar healthbar;
    public NavMeshAgent navMeshAgent;

    private int currentHealth;
    private int damage = 5;
    private float attackCooldown = -10f;
    private GameObject player;
    private IObjectPool<EnemyController> enemyPool;
    private Animator animator;

    private void Awake() {
        player = GameObject.FindGameObjectWithTag("Player");
        playerController = player.GetComponent<PlayerController>();
        animator = GetComponentInChildren<Animator>();

        navMeshAgent = GetComponent<NavMeshAgent>();

        currentHealth = maxHealth;
        healthbar.SetMaxHealth(maxHealth);
    }

    private void FixedUpdate() {
        // Turn enemy AI off when player enters safe zone
        if (!playerController.inSafeZone) {
            // Change target destination to player position
            navMeshAgent.destination = player.transform.position;

            // If within distance of player, deal damage
            if (Vector3.Distance(transform.position, player.transform.position) <= 3f) {
                // Only deal damage once every second max
                if (Time.time >= attackCooldown + 1f) {
                    attackCooldown = Time.time;
                    playerController.TakeDamage(damage);
                }
            }
        }

        // If moving, play walking animation
        animator.SetBool("IsMoving", navMeshAgent.velocity.magnitude > 1f);
    }

    public void SetPool(IObjectPool<EnemyController> pool) {
        // Put enemy in pool on spawn
        enemyPool = pool;
    }

    public void TakeDamage(int damage) {
        currentHealth -= damage;
        healthbar.SetHealth(currentHealth);

        if (currentHealth <= 0) {
            enemyPool.Release(this);
        }
    }

    public void ResetHealth() {
        currentHealth = maxHealth;
        healthbar.SetHealth(currentHealth);
    }
}
