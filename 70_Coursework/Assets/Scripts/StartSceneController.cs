using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneController : MonoBehaviour {
    public void StartGame() {
        Debug.Log("Starting Game...");
        SceneManager.LoadScene("Game");
    }
}
