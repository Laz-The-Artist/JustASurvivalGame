using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MultiCoreLibCSE;
using TMPro;
using UnityEditorInternal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

    [Header("Main Menu")]
    public MenuScreen menuPage = MenuScreen.MAIN;
    public Canvas menuMain;
    public Canvas menuPlay;
    public Canvas menuSettings;
    
    [Header("Play Submenu")]
    public SubMenuScreenOfPlay subMenuPlayPage = SubMenuScreenOfPlay.LOAD;
    public Canvas subMenuPlayLoad;
    public Canvas subMenuPlayGen;

    [Header("Play Submenu - Gen")] 
    public TMP_InputField worldNameInputField;
    public Toggle shouldGenRandomSeedToggle;
    public TMP_InputField worldSeedInputField;
    public TMP_Dropdown worldSizeDropDown;
    public TMP_Dropdown chunkSizeDropDown;
    public TMP_Dropdown landmassGeneratorDropDown;

    [Header("Loading Screen")]
    public Canvas loadingScreen;
    public Image loadingBar;
    
    void Start() {
        
        GameLocationUtil.checkGameLocationsIntegrity(GameLocationUtil.gameSettingsLoc, GameLocationUtil.gameModsLoc, GameLocationUtil.gameSavesLoc);
        
        menuPage = MenuScreen.MAIN;
    }

    void Update() {
        
        switchMenuPages();

    }

    void switchMenuPages() {
        switch (menuPage) {
            case MenuScreen.MAIN:
                loadMainMenu();
                break;
            case MenuScreen.PLAY:
                loadPlayMenu();
                break;
            case MenuScreen.SETTINGS:
                loadSettingsMenu();
                break;
            case MenuScreen.LOADING:
                loadLoadingScreen();
                break;

        }

        switch (subMenuPlayPage) {
            case SubMenuScreenOfPlay.LOAD:
                loadPlaySubLoadMenu();
                break;
            case SubMenuScreenOfPlay.GEN:
                loadPlaySubGenMenu();
                break;
        }
    }

    void loadMainMenu() {
        menuMain.transform.gameObject.SetActive(true);
        menuPlay.transform.gameObject.SetActive(false);
        menuSettings.transform.gameObject.SetActive(false);
        loadingScreen.transform.gameObject.SetActive(false);
    }
    void loadPlayMenu() {
        menuMain.transform.gameObject.SetActive(false);
        menuPlay.transform.gameObject.SetActive(true);
        menuSettings.transform.gameObject.SetActive(false);
        loadingScreen.transform.gameObject.SetActive(false);
    }
    void loadSettingsMenu() {
        menuMain.transform.gameObject.SetActive(false);
        menuPlay.transform.gameObject.SetActive(false);
        menuSettings.transform.gameObject.SetActive(true);
        loadingScreen.transform.gameObject.SetActive(false);
    }
    void loadLoadingScreen() {
        menuMain.transform.gameObject.SetActive(false);
        menuPlay.transform.gameObject.SetActive(false);
        menuSettings.transform.gameObject.SetActive(false);
        loadingScreen.transform.gameObject.SetActive(true);
    }


    void loadPlaySubLoadMenu() {
        subMenuPlayGen.transform.gameObject.SetActive(false);
        subMenuPlayLoad.transform.gameObject.SetActive(true);
    }
    void loadPlaySubGenMenu() {
        subMenuPlayGen.transform.gameObject.SetActive(true);
        subMenuPlayLoad.transform.gameObject.SetActive(false);
    }
    
    
    public void setMenuState(String inputState) {
        MenuScreen.TryParse(inputState, out menuPage);
    }
    
    public void setSubMenuStateOfPlay(String inputState) {
        SubMenuScreenOfPlay.TryParse(inputState, out subMenuPlayPage);
    }


    public void generateWorldAction() {
        //Preparation - Generate world gen info/config
        WorldConfig config = new WorldConfig();
        config.loadExisting = false;
        config.worldName = worldNameInputField.text;
        config.genRandomSeed = shouldGenRandomSeedToggle.isOn;
        if (!shouldGenRandomSeedToggle.isOn) {
            config.worldSeed = int.Parse(worldSeedInputField.text);
        }else {
            config.worldSeed = 0;
        }
        config.worldSize = int.Parse(worldSizeDropDown.options[worldSizeDropDown.value].text);
        config.chunkSize = int.Parse(chunkSizeDropDown.options[chunkSizeDropDown.value].text);
        config.landmassGeneratorType = (LandmassGenerator)landmassGeneratorDropDown.value;
        
        FileHelperUtil.saveFile(config, GameLocationUtil.gameSavesLoc + "worldGenConfig.json");
        
        //Loading the world scene and the world itslef
        StartCoroutine(LoadSceneAsync("GameWorld"));
    }
    
    public IEnumerator LoadSceneAsync(string sceneToLoad) {
        
        AsyncOperation SceneLoadOp = SceneManager.LoadSceneAsync(sceneToLoad);

        while (!SceneLoadOp.isDone) {
            loadingBar.fillAmount = SceneLoadOp.progress;
            yield return null;
        }
        
    }

    public void setReverseOfISActive(GameObject toDeactivate) {
        toDeactivate.SetActive(!toDeactivate.activeSelf);
    }
}

public enum MenuScreen {
    MAIN,
    PLAY,
    SETTINGS,
    LOADING
}

public enum SubMenuScreenOfPlay {
    LOAD,
    GEN
}

[System.Serializable]
public class WorldConfig {
    public bool loadExisting;
    public String worldName;
    public bool genRandomSeed;
    public int worldSeed;
    public int worldSize;
    public int chunkSize;
    public LandmassGenerator landmassGeneratorType;
}

