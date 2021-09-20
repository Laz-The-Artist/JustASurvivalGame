using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MultiCoreLibCSE;

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
                    menuMain.transform.gameObject.SetActive(true);
                    menuPlay.transform.gameObject.SetActive(false);
                    menuSettings.transform.gameObject.SetActive(false);
                break;
            case MenuScreen.PLAY:
                    menuMain.transform.gameObject.SetActive(false);
                    menuPlay.transform.gameObject.SetActive(true);
                    menuSettings.transform.gameObject.SetActive(false);
                break;
            case MenuScreen.SETTINGS:
                    menuMain.transform.gameObject.SetActive(false);
                    menuPlay.transform.gameObject.SetActive(false);
                    menuSettings.transform.gameObject.SetActive(true);
                break;

        }

        switch (subMenuPlayPage) {
            case SubMenuScreenOfPlay.LOAD:
                    subMenuPlayGen.transform.gameObject.SetActive(false);
                    subMenuPlayLoad.transform.gameObject.SetActive(true);
                break;
            case SubMenuScreenOfPlay.GEN:
                    subMenuPlayGen.transform.gameObject.SetActive(true);
                    subMenuPlayLoad.transform.gameObject.SetActive(false);
                break;
        }
    }

    public void setMenuState(String inputState) {
        MenuScreen.TryParse(inputState, out menuPage);
    }
    
    public void setSubMenuStateOfPlay(String inputState) {
        SubMenuScreenOfPlay.TryParse(inputState, out subMenuPlayPage);
    }
    

}

[System.Serializable]
public enum MenuScreen {
    MAIN,
    PLAY,
    SETTINGS
}

[System.Serializable]
public enum SubMenuScreenOfPlay {
    LOAD,
    GEN
}

