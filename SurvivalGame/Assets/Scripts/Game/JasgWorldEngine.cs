using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using UnityEngine;
using MultiCoreLibCSE;
using TMPro;
using UnityEngine.SceneManagement;

public class JasgWorldEngine : MonoBehaviour {
    
    private CancellationTokenSource tokenSource;
    
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

    [HideInInspector] public bool[,] landmassGrid;
    [Header("Landmass Gen")]
    [Range(0, 100)] public int cellularFillPercentage;
    public int cellularSmoothingCyclesAmount;
    public Texture2D landmassTexture;

    [Header("Loading and Runtime")]
    public bool isLoadingWorld = true;
    public String worldStatus;
    public int worldLoadAmount;


    void Start() {

        tokenSource = new CancellationTokenSource();
        
        GameLocationUtil.checkGameLocationsIntegrity(GameLocationUtil.gameSettingsLoc, GameLocationUtil.gameModsLoc, GameLocationUtil.gameSavesLoc);

        if (!bypassLoadConfig) {
            LoadWorldConfig();
        }

        SetupWorld();
        
        StartWorld();
    }

    public void LoadWorldConfig() {
        string jsonString = File.ReadAllText(GameLocationUtil.gameSavesLoc + "worldGenConfig.json");
        WorldConfig config;
        config = JsonUtility.FromJson<WorldConfig>(jsonString);
        loadExisting = config.loadExisting;
        worldName = config.worldName;
        genRandomSeed = config.genRandomSeed;
        worldSeed = config.worldSeed;
        worldSize = config.worldSize;
        chunkSize = config.chunkSize;
    }

    public void SetupWorld() {

        if (genRandomSeed) {
            worldSeed = UnityEngine.Random.Range(0, 999999);
        }
        
        landmassGrid = new Boolean[worldSize, worldSize];
        landmassTexture = new Texture2D(worldSize, worldSize);

    }
    
    
    public void StartWorld() {
        switch (loadExisting) {
            case true:
                LoadWorld();
                break;
            case false:
                GenerateWorld();
                break;
        }
    }

    public void LoadWorld() {
        
    }
    
    
    public void GenerateWorld() {
        Directory.CreateDirectory(GameLocationUtil.gameSavesLoc + "/" + worldName);
        
        genCellularLandmass();
    }

    public void genCellularLandmass() {
        worldStatus = "Generating initial landmass noise";
        System.Random randChoice = new System.Random(worldSeed.GetHashCode());
        for (int x = 0; x < worldSize; x++) {
            for (int y = 0; y < worldSize; y++) {
                switch (randChoice.Next(0, 100) < cellularFillPercentage) {
                    case true:
                        landmassGrid[x, y] = true;
                        break;
                    case false:
                        landmassGrid[x, y] = false;
                        break;
                }
            }
        }
        
        worldStatus = "Applying cellular automata smoothing to landmass noise";
        for (int i = 0; i < cellularSmoothingCyclesAmount; i++) {
            for (int x = 0; x < worldSize; x++) {
                for (int y = 0; y < worldSize; y++) {
                    int neighboringWalls = GettingNeighbors(x, y);

                    if (neighboringWalls > 5) {
                        landmassGrid[x, y] = true;
                    } else if (neighboringWalls < 5) {
                        landmassGrid[x, y] = false;
                    }
                }
            }
        }
        
        //Putting the grid to a texture for saving
        worldStatus = "Converting landmass noise to image format";
        for (int x = 0; x < worldSize; x++) {
            for (int y = 0; y < worldSize; y++) {
                if (landmassGrid[x, y]) {
                    landmassTexture.SetPixel(x,y, Color.black);    
                }else {
                    landmassTexture.SetPixel(x,y, Color.white);
                }
            }
        }
        
        landmassTexture.Apply();
    }
    private int GettingNeighbors(int pointX, int pointY) {

        int wallNeighbors = 0;

        for (int x = pointX - 1; x <= pointX + 1; x++) {
            for (int y = pointY - 1; y <= pointY + 1; y++) {
                if (x >= 0 && x < worldSize && y >= 0 && y < worldSize) {
                    if (x != pointX || y != pointY) {
                        if (landmassGrid[x, y]) {
                            wallNeighbors++;
                        }
                    }
                } else {
                    wallNeighbors++;
                }
            }
        }

        return wallNeighbors;
    }
    

    void Update() {
        
    }
    
}
