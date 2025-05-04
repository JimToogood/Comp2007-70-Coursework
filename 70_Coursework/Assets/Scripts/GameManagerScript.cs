using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerScript : MonoBehaviour {
    [SerializeField] Animator transitionAnimator;
    [SerializeField] CanvasGroup canvasGroup;

    public static GameManagerScript instance;
    public string loadSource = "";

    private void Awake() {
        if (instance == null) {
            DontDestroyOnLoad(this.gameObject);
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string scene, string givenLoadSource = "") {
        canvasGroup.blocksRaycasts = true;
        loadSource = givenLoadSource;
        StartCoroutine(AnimateLoad(scene));
    }

    IEnumerator AnimateLoad(string scene) {
        transitionAnimator.SetTrigger("End");
        yield return new WaitForSeconds(1);

        SceneManager.LoadSceneAsync(scene);
        transitionAnimator.SetTrigger("Start");
        canvasGroup.blocksRaycasts = false;
    }
}
