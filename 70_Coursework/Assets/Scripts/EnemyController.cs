using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

public class EnemyController : MonoBehaviour {
    [SerializeField, Range(0, 250)] public int maxHealth = 100;
    [SerializeField] public EnemyHealthbar healthbar;
    [SerializeField] public int currentHealth;
    [SerializeField] public AudioClip deathSound;

    private int damage = 5;
    private NavMeshAgent navMeshAgent;
    private float attackCooldown = -10f;
    private GameObject player;
    private PlayerController playerController;
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
        navMeshAgent.destination = player.transform.position;

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
        enemyPool = pool;
    }

    public void TakeDamage(int damage) {
        currentHealth -= damage;
        healthbar.SetHealth(currentHealth);

        if (currentHealth <= 0) {
            // Cant use an audio source attached to the enemy as it would not play after the enemy is released from the pool
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
            enemyPool.Release(this);
        }
    }

    public void ResetHealth() {
        currentHealth = maxHealth;
        healthbar.SetHealth(currentHealth);
    }
}
