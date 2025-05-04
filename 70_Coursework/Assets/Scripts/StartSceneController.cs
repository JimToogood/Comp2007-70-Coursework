using UnityEngine;

public class StartSceneController : MonoBehaviour {
    [SerializeField] private GameManagerScript sceneLoader;

    public void StartGame() {
        Debug.Log("Starting Game...");
        sceneLoader.LoadScene("Game");
    }
}
