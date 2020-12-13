using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIButton : MonoBehaviour {

    [Header("Load Scene Functions")]
        public string SceneName = "";
        public float LoadProgress;
        public bool UseProgressBar = false;
        public GameObject LoadScreen;
        public Image LoadingProgressBar;

    Animator ButtonAnimator;
    bool IsSelected = false;

    void Start() {
        ButtonAnimator = this.GetComponent<Animator>();
    }

    void Update() {
        if (IsSelected == true && Input.GetMouseButtonDown(0)) {
            /*if (LoadSceneBTN) {
                StartCoroutine(LoadSceneAsync(SceneName));
            } else if (OpenMenuBTN) {
                Settings();
            }else if (ExitGameBTN) {
                ExitGame();
            }*/

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

    public void LoadScene() {
        StartCoroutine(LoadSceneAsync(SceneName));
    }

    public IEnumerator LoadSceneAsync(string __sceneName) {
        LoadScreen.SetActive(true);
        AsyncOperation SceneLoadOp = SceneManager.LoadSceneAsync(__sceneName);

        while (!SceneLoadOp.isDone) {
            LoadProgress = SceneLoadOp.progress;
            if (UseProgressBar) {
                LoadingProgressBar.fillAmount = LoadProgress;
            }
            Debug.Log("Loading Scene " + __sceneName + "; progress: " + LoadProgress);
            yield return null;
        }
        
    }

    public void Settings() {
        Debug.Log("Entered the settings menu");
    }
    public void ExitGame() {
        Application.Quit();
        Debug.Log("Exited game");
    }

}
