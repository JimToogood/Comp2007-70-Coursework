using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class ParticleSpawner : MonoBehaviour {
    [SerializeField] private GameObject particlePrefab;
    [SerializeField] private float particleLifetime;
    [SerializeField] private AudioClip audioClip = null;
    [SerializeField] private float audioVolume;

    private IObjectPool<GameObject> particlePool;
    private AudioSource audioSource = null;

    private void Awake() {
        particlePool = new ObjectPool<GameObject>(CreateParticle, OnSpawn, OnRelease);
        if (audioClip != null) {
            // If particle spawner also spawns sound, get audio source
            audioSource = GetComponentInChildren<AudioSource>();
        }
    }

    public void SpawnParticle(Vector3 spawnPosition) {
        // Generate particle
        GameObject particle = particlePool.Get();
        particle.transform.position = spawnPosition;

        if (audioClip != null) {
            // Play audio at particle location
            audioSource.transform.position = spawnPosition;
            audioSource.PlayOneShot(audioSource.clip, audioVolume);
        }
    }

    private void OnSpawn(GameObject particle) {
        // Respawn particle in pool
        particle.gameObject.SetActive(true);
        StartCoroutine(ReleaseAfterDelay(particle));
    }

    private void OnRelease(GameObject particle) {
        // Despawn particle in pool
        particle.gameObject.SetActive(false);
    }

    private GameObject CreateParticle() {
        // Generate new particle
        GameObject particle = Instantiate(particlePrefab);

        return particle;
    }

    private IEnumerator ReleaseAfterDelay(GameObject particle) {
        // Hide particle after x amount of time
        yield return new WaitForSeconds(particleLifetime);
        particlePool.Release(particle);
    }
}
