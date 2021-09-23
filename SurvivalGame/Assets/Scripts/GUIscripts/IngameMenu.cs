using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IngameMenu : MonoBehaviour {


    [Header("Game HUD")] public Canvas HUD;
    public bool hudState;

    [Header("Pause Menu")] public Canvas pauseMenu;
    public bool pauseMenuState;

    [Header("Loading Screen")] public Canvas loadingScreen;
    public bool loadingScreenState;
    public Image loadingBar;
    public TMP_Text loadingStatusMessage;

    [Header("Other References")] 
    public JasgWorldEngine worldEngine;

    void Start() {

    }

    void Update() {

        if (Input.GetKeyDown(KeyCode.Escape) && !loadingScreen.transform.gameObject.activeSelf) {
            pauseMenuState = !pauseMenu.transform.gameObject.activeSelf;
            pauseMenu.transform.gameObject.SetActive(pauseMenuState);
        }

        if (Input.GetKeyDown(KeyCode.F1) && !loadingScreen.transform.gameObject.activeSelf) {
            hudState = !HUD.transform.gameObject.activeSelf;
            HUD.transform.gameObject.SetActive(hudState);
        }

        if (worldEngine.isLoadingWorld) {
            loadingScreen.transform.gameObject.SetActive(true);
        }else if (loadingScreenState) {
            loadingScreen.transform.gameObject.SetActive(true);
        }else {
            loadingScreen.transform.gameObject.SetActive(false);
        }

        if (worldEngine.isLoadingWorld) {
            updateLoadingScreen();
        }

    }

    public void updateLoadingScreen() {
        loadingBar.fillAmount = worldEngine.worldLoadAmount;
        loadingStatusMessage.text = worldEngine.worldStatus;
    }

    public void ExitToMainMenu() {
        loadingScreenState = true;
        StartCoroutine(LoadSceneAsync("MainMenu"));
    }

    public IEnumerator LoadSceneAsync(string sceneToLoad) {
        
        AsyncOperation SceneLoadOp = SceneManager.LoadSceneAsync(sceneToLoad);

        while (!SceneLoadOp.isDone) {
            loadingBar.fillAmount = SceneLoadOp.progress;
            yield return null;
        }
        
    }

}
