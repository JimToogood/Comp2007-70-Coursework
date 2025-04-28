using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

public class EnemyController : MonoBehaviour {
    [SerializeField, Range(0, 250)] private int maxHealth = 100;

    public int droppedGold;
    public PlayerController playerController;
    public EnemyHealthbar healthbar;

    private int currentHealth;
    private int damage = 5;
    private NavMeshAgent navMeshAgent;
    private float attackCooldown = -10f;
    private GameObject player;
    private IObjectPool<EnemyController> enemyPool;

    private void Awake() {
        player = GameObject.FindGameObjectWithTag("Player");
        playerController = player.GetComponent<PlayerController>();

        navMeshAgent = GetComponent<NavMeshAgent>();

        currentHealth = maxHealth;
        healthbar.SetMaxHealth(maxHealth);
    }

    private void FixedUpdate() {
        // Change target destination to player position
        if (!playerController.inSafeZone) {
            navMeshAgent.destination = player.transform.position;
        }

        // If within distance of player, deal damage
        if (Vector3.Distance(gameObject.transform.position, player.transform.position) <= 3f) {
            // Only deal damage once every second max
            if (Time.time >= attackCooldown + 1f) {
                attackCooldown = Time.time;
                playerController.TakeDamage(damage);
            }
        }
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
