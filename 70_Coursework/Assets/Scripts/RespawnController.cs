using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnController : MonoBehaviour {
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioSource audioSource;

    private void OnEnable() {
        audioSource.PlayOneShot(deathSound, 1f);
    }

    public void Respawn() {
        Debug.Log("Respawning...");
        SceneManager.LoadScene("Game");
    }
}
