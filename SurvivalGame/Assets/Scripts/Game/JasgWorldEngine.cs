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
    public Renderer uiMapDisplay;

    [Header("Main World Config")]
    public bool bypassLoadConfig = false;
    public WorldConfig worldConfig;

    [HideInInspector] public int[,] landmassGrid;
    [Header("Landmass Gen - cellular")]
    [Range(0, 100)] public int cellularFillPercentage;
    public int cellularSmoothingCyclesAmount;
    public int cellThreshold;
    public Texture2D landmassTexture;
    [Header("Landmass Gen - cellular")]
    public float perlinLandmassScale;
    [Range(0, 1)] public float perlinLandmassAreaDivision;
    
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
        
        isLoadingWorld = false;
    }

    public void LoadWorldConfig() {
        string jsonString = File.ReadAllText(GameLocationUtil.gameSavesLoc + "worldGenConfig.json");
        WorldConfig config;
        config = JsonUtility.FromJson<WorldConfig>(jsonString);
        worldConfig = config;
    }

    public void SetupWorld() {

        if (worldConfig.genRandomSeed) {
            worldConfig.worldSeed = UnityEngine.Random.Range(0, 999999);
        }
        
        landmassGrid = new int[worldConfig.worldSize, worldConfig.worldSize];
        landmassTexture = new Texture2D(worldConfig.worldSize, worldConfig.worldSize);
        
        landmassTexture.filterMode = FilterMode.Point;

        uiMapDisplay.GetComponent<SpriteRenderer>().sprite = Sprite.Create(landmassTexture, new Rect(0.0f, 0.0f, worldConfig.worldSize, worldConfig.worldSize), new Vector2(0.5f, 0.5f), 100.0f);

    }
    
    
    public void StartWorld() {
        switch (worldConfig.loadExisting) {
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
        Directory.CreateDirectory(GameLocationUtil.gameSavesLoc + "/" + worldConfig.worldName);

        switch (worldConfig.landmassGeneratorType) {
            case LandmassGenerator.CELLULAR:
                genCellularLandmass();
                break;
            case LandmassGenerator.PERLIN:
                GenLandmassPerlin();
                break;
            case LandmassGenerator.JASG_CUSTOM:
                genCellularLandmass();
                break;
        }
    }

    public void genCellularLandmass() {
        System.Random randChoice = new System.Random(worldConfig.worldSeed.GetHashCode());
        for (int x = 0; x < worldConfig.worldSize; x++) {
            for (int y = 0; y < worldConfig.worldSize; y++) {
                if (randChoice.Next(0, 100) < cellularFillPercentage) {
                    landmassGrid[x, y] = 1;
                } else {
                    landmassGrid[x, y] = 0;
                }

            }
        }
        
        //Running the cellular automata - smoothing
        for (int i = 0; i < cellularSmoothingCyclesAmount; i++) {
            for (int x = 0; x < worldConfig.worldSize; x++) {
                for (int y = 0; y < worldConfig.worldSize; y++) {
                    int neighboringWalls = GettingNeighbors(x, y);

                    if (neighboringWalls > cellThreshold) {
                        landmassGrid[x, y] = 1;
                    } else if (neighboringWalls < cellThreshold) {
                        landmassGrid[x, y] = 0;
                    }
                }
            }
        }
        
        //Putting the grid to a texture for saveing
        for (int x = 0; x < worldConfig.worldSize; x++) {
            for (int y = 0; y < worldConfig.worldSize; y++) {
                if (landmassGrid[x, y] == 1) {
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
                if (x >= 0 && x < worldConfig.worldSize && y >= 0 && y < worldConfig.worldSize) {
                    if (x != pointX || y != pointY) {
                        if (landmassGrid[x, y] == 1) {
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
    
    public void GenLandmassPerlin() {
        
        for (int x = 0; x < worldConfig.worldSize; x++) {
            for (int y = 0; y < worldConfig.worldSize; y++) {
                Color color = CalculatePerlinColor(x, y);
                landmassTexture.SetPixel(x, y, color);
            }
        }
        
        landmassTexture.Apply();
    }

    Color CalculatePerlinColor(int GridX, int GridY) {
        float PerlinCoordX = (((float)GridX / worldConfig.worldSize) * perlinLandmassScale) + worldConfig.worldSeed;
        float PerlinCoordY = (((float)GridY / worldConfig.worldSize) * perlinLandmassScale) + worldConfig.worldSeed;
        
        float sample = Mathf.PerlinNoise(PerlinCoordX, PerlinCoordY);

        if (sample > perlinLandmassAreaDivision) {
            sample = 1f;
            landmassGrid[GridX, GridY] = 1;
        }else{
            sample = 0f;
            landmassGrid[GridX, GridY] = 0;
        }

        return new Color(sample, sample, sample);
    }


    void Update() {
        
    }
    
}

public enum LandmassGenerator {
    CELLULAR,
    PERLIN,
    JASG_CUSTOM
}
