using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UITextController : MonoBehaviour {
    private TextMeshProUGUI text;
    private Animator textAnimator;

    private void Awake() {
        text = GetComponent<TextMeshProUGUI>();
        textAnimator = GetComponent<Animator>();
    }

    public void DisplayText(string textInput) {
        text.text = textInput;
        textAnimator.Play("TextFade", -1, 0f);
    }
}
