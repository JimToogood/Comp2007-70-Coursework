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

    public void ExitButton() {
        #if UNITY_EDITOR
            // If in the editor, exit play mode
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // If not in the editor, quit the application
            Application.Quit();
        #endif
    }
}
