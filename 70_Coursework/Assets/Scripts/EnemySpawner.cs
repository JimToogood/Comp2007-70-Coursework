using UnityEngine;
using UnityEngine.Pool;

public class EnemySpawner : MonoBehaviour {
    [SerializeField, Range(0f, 10f)] public float spawnInterval = 2f;
    [SerializeField, Range(0f, 15f)] public float maxEnemies = 3f;

    [SerializeField] public Transform[] spawnpoints;
    [SerializeField] public EnemyController enemyPrefab;
    [SerializeField] public AudioClip deathSound;

    private float spawnCooldown = -10f;
    private IObjectPool<EnemyController> enemyPool;
    private int activePoolSize = 0;

    private void Awake() {
        enemyPool = new ObjectPool<EnemyController>(CreateEnemy, OnSpawn, OnRelease);
    }

    private void OnSpawn(EnemyController enemy) {
        activePoolSize ++;
        enemy.gameObject.SetActive(true);
        enemy.transform.position = spawnpoints[Random.Range(0, spawnpoints.Length)].position;
        enemy.ResetHealth();
    }

    private void OnRelease(EnemyController enemy) {
        activePoolSize --;
        enemy.gameObject.SetActive(false);
        // spawnCooldown also functions as respawnCooldown
        spawnCooldown = Time.time;
    }

    private void FixedUpdate() {
        if (Time.time >= spawnCooldown + spawnInterval && activePoolSize < maxEnemies) {
            spawnCooldown = Time.time;
            enemyPool.Get();
        }
    }

    private EnemyController CreateEnemy() {
        EnemyController enemy = Instantiate(enemyPrefab);
        enemy.deathSound = deathSound;
        enemy.SetPool(enemyPool);

        return enemy;
    }
}
