using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnController : MonoBehaviour {
    public void Respawn() {
        Debug.Log("Respawning...");

        SceneManager.LoadScene("Game");
    }
}
