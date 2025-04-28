using UnityEngine;
using UnityEngine.UI;

public class PauseMenuHandler : MonoBehaviour {
    private PlayerController player;

    private void Awake() {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    public void ResumeButton() {
        player.TogglePause(true);
    }

    public void SettingsButton() {
        // Add settings menu here later
        Debug.Log("Settings");
    }

    public void ExitButton() {
        // Add save logic here later
        Application.Quit();
        
        // Application.Quit does not work in the Editor
        UnityEditor.EditorApplication.isPlaying = false;
    }
}
