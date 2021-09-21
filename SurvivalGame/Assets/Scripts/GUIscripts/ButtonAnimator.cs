using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonAnimator : MonoBehaviour {
    
    Animator buttonAnimator;
    private bool isSelected;
    
    void Start() {
        buttonAnimator = this.GetComponent<Animator>();
    }

    void Update() {
        switch (isSelected && Input.GetMouseButtonDown(0)) {
            case true:
                buttonAnimator.Play("JasgButtonPress");
                break;
        }
    }
    
    private void OnMouseEnter() {
        isSelected = true;
        buttonAnimator.Play("JasgButtonHover");
    }

    private void OnMouseExit() {
        isSelected = false;
        buttonAnimator.Play("JasgButtonHoverExit");
    }
}
