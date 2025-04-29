using UnityEngine;
using UnityEngine.Pool;

public class EnemySpawner : MonoBehaviour {
    [SerializeField, Range(0f, 10f)] private float spawnInterval = 2f;
    [SerializeField, Range(0f, 15f)] private float maxEnemies = 3f;

    [SerializeField] private Transform[] spawnpoints;
    [SerializeField] private EnemyController enemyPrefab;
    [SerializeField] private AudioClip deathSound;

    private ParticleSpawner particleSpawner;
    private AudioSource audioSource;
    private IObjectPool<EnemyController> enemyPool;
    private float spawnCooldown = 0f;
    private int activePoolSize = 0;

    private void Awake() {
        particleSpawner = GetComponent<ParticleSpawner>();
        audioSource = GetComponentInChildren<AudioSource>();
        enemyPool = new ObjectPool<EnemyController>(CreateEnemy, OnSpawn, OnRelease);
    }

    private void OnSpawn(EnemyController enemy) {
        // Respawn enemy in pool
        activePoolSize ++;
        enemy.gameObject.SetActive(true);

        enemy.transform.position = spawnpoints[Random.Range(0, spawnpoints.Length)].position;
        enemy.droppedGold = Random.Range(5, 11);
        enemy.ResetHealth();
    }

    private void OnRelease(EnemyController enemy) {
        // Despawn enemy in pool
        activePoolSize --;
        particleSpawner.SpawnParticle(enemy.transform.position);
        enemy.playerController.ChangeGold(enemy.droppedGold);

        audioSource.transform.position = enemy.transform.position;
        audioSource.PlayOneShot(audioSource.clip, 0.75f);

        enemy.gameObject.SetActive(false);
        // spawnCooldown also functions as respawnCooldown
        spawnCooldown = Time.time;
    }

    private void FixedUpdate() {
        if (Time.time >= spawnCooldown + spawnInterval && activePoolSize < maxEnemies) {
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
}
