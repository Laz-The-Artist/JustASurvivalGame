using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButton : MonoBehaviour {

    [Header("General Settings")]
        public bool MainPlayButton = false;
        public bool SettingsButton = false;
        public bool ExitButton = false;

    Animator ButtonAnimator;
    bool IsSelected = false;

    void Start() {
        ButtonAnimator = this.GetComponent<Animator>();
    }

    void Update() {
        if (IsSelected == true && Input.GetMouseButtonDown(0)) {
            if (MainPlayButton) {
                Play();
                ButtonAnimator.Play("btn_press");
            }else if (SettingsButton) {
                Settings();
                ButtonAnimator.Play("btn_press");
            }else if (ExitButton) {
                ExitGame();
                ButtonAnimator.Play("btn_press");
            }
        }
    }

    private void OnMouseEnter() {
        IsSelected = true;
        ButtonAnimator.Play("btn_hover_enter");
    }

    private void OnMouseExit() {
        IsSelected = false;
        ButtonAnimator.Play("btn_hover_exit");
    }

    public void Play() {
        Debug.Log("well played, gg");
    }
    public void Settings() {
        Debug.Log("Entered the settings menu");
    }
    public void ExitGame() {
        Debug.Log("Exited game");
    }

}
