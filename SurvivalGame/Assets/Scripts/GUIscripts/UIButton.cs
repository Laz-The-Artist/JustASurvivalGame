using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIButton : MonoBehaviour {

    [Header("Load Scene Functions")]
        public bool isAnimatable = true;

    Animator ButtonAnimator;
    bool IsSelected = false;

    void Start() {
        ButtonAnimator = this.GetComponent<Animator>();
    }

    void Update() {
        if (IsSelected && Input.GetMouseButtonDown(0) && isAnimatable) {

            ButtonAnimator.Play("btn_press");
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

    public void ExitGame() {
        Application.Quit();
        Debug.Log("Exited game");
    }

}
