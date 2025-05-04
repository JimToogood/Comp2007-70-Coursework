using UnityEngine;

public class RespawnController : MonoBehaviour {
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioSource audioSource;
    private GameManagerScript sceneLoader;

    private void Awake() {
        sceneLoader = FindObjectOfType<GameManagerScript>();
    }

    private void OnEnable() {
        audioSource.PlayOneShot(deathSound, 1f);
    }

    public void Respawn() {
        Debug.Log("Respawning...");
        sceneLoader.LoadScene("Game", "DeathScene");
    }
}
