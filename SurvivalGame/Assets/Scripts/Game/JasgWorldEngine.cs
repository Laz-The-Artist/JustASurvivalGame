using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MultiCoreLibCSE;

public class JasgWorldEngine : MonoBehaviour {
    
    [Header("References")]
    public IngameMenu IngameMenu;

    [Header("Main World Config")]
    public bool bypassLoadConfig = false;
    public bool loadExisting;
    public String worldName;
    public bool genRandomSeed;
    public int worldSeed;
    public int worldSize;
    public int chunkSize;
    
    void Start() {
        
        GameLocationUtil.checkGameLocationsIntegrity(GameLocationUtil.gameSettingsLoc, GameLocationUtil.gameModsLoc, GameLocationUtil.gameSavesLoc);

        if (!bypassLoadConfig) {
            LoadWorldConfig();
        }
        
    }

    public void LoadWorldConfig() {
        WorldConfig config = (WorldConfig)FileHelperUtil.loadFile(GameLocationUtil.gameSavesLoc + "worldGenConfig.json");
        loadExisting = config.loadExisting;
        worldName = config.worldName;
        genRandomSeed = config.genRandomSeed;
        worldSeed = config.worldSeed;
        worldSize = config.worldSize;
        chunkSize = config.chunkSize;
    }

    void Update() {
        
    }
}
