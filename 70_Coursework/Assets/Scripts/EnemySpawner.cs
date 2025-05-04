using UnityEngine;
using UnityEngine.Pool;

public class EnemySpawner : MonoBehaviour {
    [SerializeField, Range(0f, 10f)] private float spawnInterval;
    [SerializeField, Range(0f, 15f)] private float maxEnemies;

    [SerializeField] private Transform[] spawnpoints;
    [SerializeField] private EnemyController enemyPrefab;

    private GameObject player;
    private PlayerController playerController;
    private ParticleSpawner particleSpawner;
    private IObjectPool<EnemyController> enemyPool;
    private float spawnCooldown = 0f;
    private int activePoolSize = 0;
    private Vector3 closestSpawnpoint;

    private void Awake() {
        player = GameObject.FindGameObjectWithTag("Player");
        playerController = player.GetComponent<PlayerController>();
        particleSpawner = GetComponent<ParticleSpawner>();
        enemyPool = new ObjectPool<EnemyController>(CreateEnemy, OnSpawn, OnRelease);
    }

    private void OnSpawn(EnemyController enemy) {
        // Respawn enemy in pool
        activePoolSize ++;
        enemy.gameObject.SetActive(true);

        closestSpawnpoint = FindClosestSpawnpoint();
        enemy.navMeshAgent.Warp(closestSpawnpoint);

        enemy.droppedGold = Random.Range(5, 11);
        enemy.ResetHealth();
    }

    private void OnRelease(EnemyController enemy) {
        // Despawn enemy in pool
        activePoolSize --;
        particleSpawner.SpawnParticle(enemy.transform.position);
        playerController.ChangeGold(enemy.droppedGold);

        enemy.gameObject.SetActive(false);
        // spawnCooldown also functions as respawnCooldown
        spawnCooldown = Time.time;
    }

    private void FixedUpdate() {
        if (Time.time >= spawnCooldown + spawnInterval && activePoolSize < maxEnemies && !playerController.inSafeZone) {
            // Generate enemy
            spawnCooldown = Time.time;
            enemyPool.Get();
        }
    }

    private EnemyController CreateEnemy() {
        // Generate new enemy
        EnemyController enemy = Instantiate(enemyPrefab);
        enemy.SetPool(enemyPool);

        return enemy;
    }

    private Vector3 FindClosestSpawnpoint() {
        Vector3 closest = spawnpoints[0].position;
        float closestSqrDistance = (spawnpoints[0].position - player.transform.position).sqrMagnitude;

        for (int i = 1; i < spawnpoints.Length; i++) {
            // sqrMagnitude instead of Vector3.Distance for better runtime performance (no sqrt)
            float sqrDistance = (spawnpoints[i].position - player.transform.position).sqrMagnitude;

            if (sqrDistance < closestSqrDistance) {
                closestSqrDistance = sqrDistance;
                closest = spawnpoints[i].position;
            }
        }

        return closest;
    }
}
