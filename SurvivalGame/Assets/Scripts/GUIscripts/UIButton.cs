using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIButton : MonoBehaviour {

    GlobalVariableHandler GVH;
    [Header("Load Scene Functions")]
        public bool isAnimatable = true;
        public string LoadSceneName = "";
        public string WorldName;
        public float LoadProgress;
        public bool UseProgressBar = false;
        public GameObject LoadScreen;
        public Image LoadingProgressBar;

    Animator ButtonAnimator;
    bool IsSelected = false;

    void Start() {
        ButtonAnimator = this.GetComponent<Animator>();

        GameObject tmp = GameObject.FindWithTag("GlobalReference");
        if (tmp != null) {
            GVH = tmp.GetComponent<GlobalVariableHandler>();
        }

    }

    void Update() {
        if (IsSelected == true && Input.GetMouseButtonDown(0) && isAnimatable) {

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

    public void LoadExistingWorld() {
        GVH.loadExisting = true;
        GVH.ReadStartSettingsForGen = true;
        GVH.LoadworldName = WorldName;
        StartCoroutine(LoadSceneAsync(LoadSceneName));
    }

    public void CreateNewGameWorld() {
        GVH.loadExisting = false;
        GVH.ReadStartSettingsForGen = true;
        StartCoroutine(LoadSceneAsync(LoadSceneName));

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
