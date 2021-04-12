using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class WorldGeneratorV4 : MonoBehaviour {
    
    [Header("Object References")]
    [Space]
    [Header("JAWG - Just A World Engine V4")]
    public GameObject Player;
    public Tilemap GridLand;
    public Tilemap GridWaterCollision;
    public Tilemap GridWater;
    public Tilemap GridSeafloor;
    public GameObject ChunkResourceHolder;
    public Renderer UIMapDisplay;
    public Tile ColliderTile;
    public AnimatedTile WaterTile;
    public Tile SeaFloorTile;
    
    [Header("Main Settings")]
    public string WorldName;
    public bool LoadExisting;
    public bool GenRandomSeed;
    [Range(0, 999999)] public int WorldSeed;
    public int WorldSize = 512;
    public int ChunkSize = 16;
    [Space]
    
    [Header("Generator Settings")]
    public LandmassGenMethod LandmassGenType;
    public BiomeGenMethod BiomeGenType;
    [Range(0, 100)] public float ResourceFillPercent;

    [Header("Landmass - Cellular Automata Settings")]
    public int CellSmoothCycles;
    [Range(0, 100)] public int CellFillPercent;
    [Range(0, 8)] public int CellThreshold;

    [Header("Landmass - Perlin Noise Settings")]
    public float LandmassPerlinScale;
    [Range(0, 1)] public float LandmassPerlinAreaDivision;

    [Space] [Header("Biome Generator Settings")]
    public BiomeDistortionMethod DistortionMapMethod;
    public float BiomesPerlinScale;
    [Range(0, 1)] public float BiomesPerlinAreaDivision;
    [Range(0, 8)] public int BiomeDistortionCellThreshold;
    public int BiomeSmoothCycles;
    public BiomeResourceAssignEntry[] ResourceAssignEntries;
    public BiomeEntry[] WorldBiomes;

    [HideInInspector] public int[,] LandmassGrid;
    [HideInInspector] public int[,] ChunkStateGrid;
    [HideInInspector] public List<WorldChunk> LoadedChunks;
    [HideInInspector] public float[,] HeatMapGrid;
    [HideInInspector] public RuleTile[,] WorldTileGrid;
    [HideInInspector] public BiomeResourceEntry[,] WorldResourceGrid;
    [HideInInspector] public int[,] WorldResourcesAssigned;

    [HideInInspector] public Texture2D LandmassCustomMap;
    [HideInInspector] public Texture2D LandmassMap;
    [HideInInspector] public Texture2D BiomesVoronoiMap;
    [HideInInspector] public Texture2D BiomesPerlinMap;
    [HideInInspector] public Texture2D BiomesMap;
    [HideInInspector] public Texture2D DisplayMap;
    
    [HideInInspector] public Texture2D MinimapBackground;

    [HideInInspector] public int WorldOffset;

    [HideInInspector] public string GameResourcesFolder;
    [HideInInspector] public string GameModsFolder;
    [HideInInspector] public string GameSavesFolder;
    [HideInInspector] public string WorldFolder;
    [HideInInspector] public string WorldDataFolder;
    [HideInInspector] public string WorldMapFolder;

    [HideInInspector] public SaveWorldDataInfo WorldDataInfoSaveObject;
    [HideInInspector] public SaveWorldDataGen WorldDataGenSaveObject;
    [HideInInspector] public SaveWorldDataRun WorldDataRunSaveObject;

    [HideInInspector] public bool IsWorldComplete = false;

    [HideInInspector] public int run_PlayerWorldPosX;
    [HideInInspector] public int run_PlayerWorldPosY;
    [HideInInspector] public int run_PlayerChunkX;
    [HideInInspector] public int run_PlayerChunkY;

    int ChunkUP;
    int ChunkDOWN;
    int ChunkRIGHT;
    int ChunkLEFT;
    int ChunkUPright;
    int ChunkUPleft;
    int ChunkDOWNright;
    int ChunkDOWNleft;
    
    [HideInInspector] public bool isChunkUnloading;
    [HideInInspector] public int chunkCounterX = 0;
    [HideInInspector] public int chunkCounterY = 0;
    [HideInInspector] public int LoadedListCounter = 0;


    void Awake() {
        switch (LoadExisting) {
            case true:
                SetupToLoad();
                break;
            case false:
                SetupWorld();
                SetupToGen();
                break;
        }
        
    }

    void Update()
    {
        switch (IsWorldComplete) {
            case true:
                LocatePlayer();

                //Doing this, is still faster than a for loop, even tho it seems stupid. Thank you.
                ChunkUP = ChunkStateGrid[run_PlayerChunkX, run_PlayerChunkY + 1];
                ChunkDOWN = ChunkStateGrid[run_PlayerChunkX, run_PlayerChunkY - 1];
                ChunkRIGHT = ChunkStateGrid[run_PlayerChunkX + 1, run_PlayerChunkY];
                ChunkLEFT = ChunkStateGrid[run_PlayerChunkX - 1, run_PlayerChunkY];
                ChunkUPright = ChunkStateGrid[run_PlayerChunkX + 1, run_PlayerChunkY + 1];
                ChunkUPleft = ChunkStateGrid[run_PlayerChunkX - 1, run_PlayerChunkY + 1];
                ChunkDOWNright = ChunkStateGrid[run_PlayerChunkX + 1, run_PlayerChunkY - 1];
                ChunkDOWNleft = ChunkStateGrid[run_PlayerChunkX - 1, run_PlayerChunkY - 1];
                
                if (ChunkStateGrid[run_PlayerChunkX, run_PlayerChunkY] == 0 || ChunkUP == 0 || ChunkDOWN == 0 || ChunkRIGHT == 0 || ChunkLEFT == 0 || ChunkUPleft == 0 || ChunkUPright == 0 || ChunkDOWNleft == 0 || ChunkDOWNright == 0) {
                    LoadChunk(run_PlayerChunkX, run_PlayerChunkY);
                    //Vertical and horizontal "expansion"
                    LoadChunk(run_PlayerChunkX, run_PlayerChunkY + 1);
                    LoadChunk(run_PlayerChunkX, run_PlayerChunkY - 1);
                    LoadChunk(run_PlayerChunkX - 1, run_PlayerChunkY);
                    LoadChunk(run_PlayerChunkX + 1, run_PlayerChunkY);
                    //Diagonal "expansion"
                    LoadChunk(run_PlayerChunkX + 1, run_PlayerChunkY + 1);
                    LoadChunk(run_PlayerChunkX - 1, run_PlayerChunkY - 1);
                    LoadChunk(run_PlayerChunkX - 1, run_PlayerChunkY + 1);
                    LoadChunk(run_PlayerChunkX + 1, run_PlayerChunkY - 1);
                }

                if (!isChunkUnloading && LoadedChunks.Count != 0 && LoadedListCounter >= 0) {
                    if (Vector2.Distance(new Vector2(run_PlayerChunkX, run_PlayerChunkY), new Vector2(LoadedChunks[LoadedListCounter].posX, LoadedChunks[LoadedListCounter].posY)) > 3) {
                        isChunkUnloading = true;
                        UnloadChunk(LoadedChunks[LoadedListCounter].posX, LoadedChunks[LoadedListCounter].posY, LoadedListCounter);
                        LoadedListCounter = 0;
                    }else {
                        if (LoadedListCounter >= LoadedChunks.Count) {
                            LoadedListCounter = 0;
                        }else {
                            LoadedListCounter++;
                        }
                    }
                }

                break;
        }

    }

    
    //SETUP
    public void SetupWorld() {
        
        if (WorldSize % 16 == 0) {
            WorldOffset = WorldSize / 2;
        }else {
            //im just lazy now so, there you go:
            WorldSize = 1024;
            WorldOffset = 512;
            //TODO: fill this in later with proper math
        }
        
        LandmassGrid = new int[WorldSize, WorldSize];
        ChunkStateGrid = new int[WorldSize / ChunkSize, WorldSize / ChunkSize];
        HeatMapGrid = new float[WorldSize, WorldSize];
        WorldTileGrid = new RuleTile[WorldSize, WorldSize];
        WorldResourceGrid = new BiomeResourceEntry[WorldSize, WorldSize];

        if (GenRandomSeed) {
            WorldSeed = UnityEngine.Random.Range(0, 999999);
        }

        LandmassMap = new Texture2D(WorldSize, WorldSize);
        BiomesVoronoiMap = new Texture2D(WorldSize, WorldSize);
        BiomesPerlinMap = new Texture2D(WorldSize, WorldSize);
        BiomesMap = new Texture2D(WorldSize, WorldSize);
        DisplayMap = new Texture2D(WorldSize, WorldSize);
        
        BiomesVoronoiMap.filterMode = FilterMode.Point;
        BiomesPerlinMap.filterMode = FilterMode.Point;
        LandmassMap.filterMode = FilterMode.Point;
        BiomesMap.filterMode = FilterMode.Point;
        DisplayMap.filterMode = FilterMode.Point;
        
        UIMapDisplay.GetComponent<SpriteRenderer>().sprite = Sprite.Create(DisplayMap, new Rect(0.0f, 0.0f, WorldSize, WorldSize), new Vector2(0.5f, 0.5f), 100.0f);
        
        //Making sure that the biome list has correct ids:
        for (int b = 0; b < WorldBiomes.Length; b++) {
            WorldBiomes[b].BiomeID = b;
        }

        GameResourcesFolder = Application.persistentDataPath;
        GameModsFolder = GameResourcesFolder + "/mods/";
        GameSavesFolder = GameResourcesFolder + "/saves/";
        
        if (!Directory.Exists(GameModsFolder)) {
            Directory.CreateDirectory(GameModsFolder);
            Debug.LogWarning("Mods folder wasn't found! Created new!");
        }
        if (!Directory.Exists(GameSavesFolder)) {
            Directory.CreateDirectory(GameSavesFolder);
            Debug.LogWarning("Saves folder wasn't found! Created new!");
        }

        WorldFolder = GameSavesFolder + WorldName + "/";
    }

    public void SetupToLoad() {
        bool worldCanBeloaded = true;
        
        GameResourcesFolder = Application.persistentDataPath;
        GameModsFolder = GameResourcesFolder + "/mods/";
        GameSavesFolder = GameResourcesFolder + "/saves/";
        
        if (!Directory.Exists(GameModsFolder)) {
            Directory.CreateDirectory(GameModsFolder);
            worldCanBeloaded = false;
            Debug.LogWarning("Mods folder wasn't found! Created new!");
        }
        if (!Directory.Exists(GameSavesFolder)) {
            Directory.CreateDirectory(GameSavesFolder);
            worldCanBeloaded = false;
            Debug.LogWarning("Saves folder wasn't found! Created new!");
        }

        WorldFolder = GameSavesFolder + WorldName + "/";
        
        if (!Directory.Exists(WorldFolder)) {
            Debug.LogError("Selected World Folder Does Not Exist!");
            worldCanBeloaded = false;
            Debug.Break();
            Application.Quit();
        }

        WorldDataFolder = WorldFolder + "data_" + WorldName + "/";
        WorldMapFolder = WorldFolder + "map_" + WorldName + "/";
        
        if (!Directory.Exists(WorldDataFolder)) {
            Debug.LogError("World Data Folder Does Not Exist!");
            worldCanBeloaded = false;
            Debug.Break();
            Application.Quit();
        }
        
        if (!Directory.Exists(WorldMapFolder)) {
            Debug.LogError("World Map Folder Does Not Exist!");
            worldCanBeloaded = false;
            Debug.Break();
            Application.Quit();
        }

        if (worldCanBeloaded) {
            LoadWorld();
        }else {
            Debug.Log("World Could Not Be Loaded! There are some missing files or loactions, or the world is invalid!");
        }
    }

    public void SetupToGen(){
        if (Directory.Exists(WorldFolder)) {
            WorldName = WorldName + System.DateTime.Now.ToString("_yyyy.MM.dd_HH.mm.ss");
            WorldFolder = GameSavesFolder + WorldName + "/";
            Directory.CreateDirectory(WorldFolder);
            Debug.LogWarning("A world with this name already exists! to prevent overwriting, a timestamp was added to the name!");
        }else {
            Directory.CreateDirectory(WorldFolder);
        }

        WorldDataFolder = WorldFolder + "data_" + WorldName + "/";
        WorldMapFolder = WorldFolder + "map_" + WorldName + "/";

        Directory.CreateDirectory(WorldDataFolder);
        Directory.CreateDirectory(WorldMapFolder);
        
        GenWorld();
    }
    

    //World Load/Gen
    public void GenWorld() {
        switch (LandmassGenType) {
            case LandmassGenMethod.CellularAutomata:
                GenLandmassCellular();
                break;
            case LandmassGenMethod.PerlinNoise:
                GenLandmassPerlin();
                break;
            case LandmassGenMethod.HexaGon:
                GenLandmassHexaGon();
                break;
            case LandmassGenMethod.JASGCustom:
                GenLandmassCustom();
                break;
        }

        switch (BiomeGenType) {
            case BiomeGenMethod.VoronoiNoise:
                GenBiomesVoronoi();
                break;
            case BiomeGenMethod.JASGCustom:
                GenBiomesCustom();
                break;
        }

        GenRsources();
        
        MapChunksToWorld();
        
        DetermineIntialPlayerSpanwPoint();
        
        GenHeatMap();
        
        
        //Minimap gen must be the last thing
        GenMinimap();

        InitSaveWorld();

        IsWorldComplete = true;

    }
    
    public void LoadWorld() {
        if (File.Exists(WorldFolder + "info_" + WorldName + ".json")) {
            string InfoJsonLoad = File.ReadAllText(WorldFolder + "info_" + WorldName + ".json");
            WorldDataInfoSaveObject = JsonUtility.FromJson<SaveWorldDataInfo>(InfoJsonLoad);
            LandmassGenType = WorldDataInfoSaveObject.LandmassGenMethod;
            BiomeGenType = WorldDataInfoSaveObject.BiomeGenMethod;
            DistortionMapMethod = WorldDataInfoSaveObject.BiomeDistortionMethod;
            Debug.Log("World Info Json Loaded Successfully! World Marked As Valid!");
        }else {
            Debug.LogError("Missing World Info Json File! World is Invalid!");
            Debug.Break();
        }
        
        if (File.Exists(WorldDataFolder + "gen_" + WorldName + ".json")) {
            string GenJsonLoad = File.ReadAllText(WorldDataFolder + "gen_" + WorldName + ".json");
            WorldDataGenSaveObject = JsonUtility.FromJson<SaveWorldDataGen>(GenJsonLoad);
            WorldSize = WorldDataGenSaveObject.WorldSize;
            WorldSeed = WorldDataGenSaveObject.WorldSeed;
            ChunkSize = WorldDataGenSaveObject.ChunkSize;
            WorldResourceGrid = new BiomeResourceEntry[WorldSize, WorldSize];
            for (int x = 0; x < WorldSize; x++) {
                for (int y = 0; y < WorldSize; y++) {
                    WorldResourceGrid[x, y] = WorldDataGenSaveObject.WorldResourcesSerialised[x+(y*WorldSize)];
                }
            }
            HeatMapGrid = new float[WorldSize, WorldSize];
            for (int x = 0; x < WorldSize; x++) {
                for (int y = 0; y < WorldSize; y++) {
                    HeatMapGrid[x, y] = WorldDataGenSaveObject.WorldHeatMap[x+(y*WorldSize)];
                }
            }
            Debug.Log("World Gen Json Loaded Successfully!");
        }else {
            Debug.LogError("Missing World Gen Json File! World is Invalid!");
            Debug.Break();
        }
        
        if (File.Exists(WorldDataFolder + "run_" + WorldName + ".json")) {
            string RunJsonLoad = File.ReadAllText(WorldDataFolder + "run_" + WorldName + ".json");
            WorldDataRunSaveObject = JsonUtility.FromJson<SaveWorldDataRun>(RunJsonLoad);
            Player.transform.position = new Vector3(WorldDataRunSaveObject.PlayerCoordX, WorldDataRunSaveObject.PlayerCoordY);
            
            Debug.Log("World Run Json Loaded Successfully!");
        }else {
            Debug.LogError("Missing World Run Json File! World is Invalid!");
            Debug.Break();
        }
        
        if (WorldSize % 16 == 0) {
            WorldOffset = WorldSize / 2;
        }else {
            //im just lazy now so, there you go:
            WorldSize = 1024;
            WorldOffset = 512;
            //TODO: fill this in later with proper math
        }
        
        LandmassGrid = new int[WorldSize, WorldSize];
        ChunkStateGrid = new int[WorldSize / ChunkSize, WorldSize / ChunkSize];
        WorldTileGrid = new RuleTile[WorldSize, WorldSize];
        
        DisplayMap = new Texture2D(WorldSize, WorldSize);

        //Making sure that the biome list has correct ids:
        for (int b = 0; b < WorldBiomes.Length; b++) {
            WorldBiomes[b].BiomeID = b;
        }

        if (File.Exists(WorldMapFolder + "map_landmass.png") && File.Exists(WorldMapFolder + "map_biomes.png") && File.Exists(WorldMapFolder + "map_minimap_bg.png")) {
            byte[] bytes_Landmass;
            byte[] bytes_Biomes;
            byte[] bytes_MinimapBG;
            bytes_Landmass = File.ReadAllBytes(WorldMapFolder + "map_landmass.png");
            bytes_Biomes = File.ReadAllBytes(WorldMapFolder + "map_biomes.png");
            bytes_MinimapBG = File.ReadAllBytes(WorldMapFolder + "map_minimap_bg.png");

            LandmassMap = new Texture2D(2, 2);
            BiomesMap = new Texture2D(2, 2);
            MinimapBackground = new Texture2D(2, 2);

            LandmassMap.LoadImage(bytes_Landmass);
            BiomesMap.LoadImage(bytes_Biomes);
            MinimapBackground.LoadImage(bytes_MinimapBG);
            
            LandmassMap.filterMode = FilterMode.Point;
            BiomesMap.filterMode = FilterMode.Point;
            MinimapBackground.filterMode = FilterMode.Point;

            for (int x = 0; x < WorldSize; x++) {
                for (int y = 0; y <WorldSize; y++) {
                    if (LandmassMap.GetPixel(x, y) == Color.black) {
                        LandmassGrid[x, y] = 1;
                    }else {
                        LandmassGrid[x, y] = 0;
                    }
                }
            }
            
            UIMapDisplay.GetComponent<SpriteRenderer>().sprite = Sprite.Create(DisplayMap, new Rect(0.0f, 0.0f, WorldSize, WorldSize), new Vector2(0.5f, 0.5f), 100.0f);

            Debug.Log("World Run Json Loaded Successfully!");
        }else {
            Debug.LogError("Missing World Run Json File! World is Invalid!");
            Debug.Break();
        }
        
        
        MapChunksToWorld();

        IsWorldComplete = true;
    }
    
    
    //LandmassMap Generators
    public void GenLandmassCellular() {
        //Fill the grid with points randomly by the seed
        System.Random randChoice = new System.Random(WorldSeed.GetHashCode());
        for (int x = 0; x < WorldSize; x++) {
            for (int y = 0; y < WorldSize; y++) {
                if (randChoice.Next(0, 100) < CellFillPercent) {
                    LandmassGrid[x, y] = 1;
                } else {
                    LandmassGrid[x, y] = 0;
                }

            }
        }
        
        //Running the cellular automata - smoothing
        for (int i = 0; i < CellSmoothCycles; i++) {
            for (int x = 0; x < WorldSize; x++) {
                for (int y = 0; y < WorldSize; y++) {
                    int neighboringWalls = GettingNeighbors(x, y);

                    if (neighboringWalls > CellThreshold) {
                        LandmassGrid[x, y] = 1;
                    } else if (neighboringWalls < CellThreshold) {
                        LandmassGrid[x, y] = 0;
                    }
                }
            }
        }
        
        //Putting the grid to a texture for saveing
        for (int x = 0; x < WorldSize; x++) {
            for (int y = 0; y < WorldSize; y++) {
                if (LandmassGrid[x, y] == 1) {
                    LandmassMap.SetPixel(x,y, Color.black);    
                }else {
                    LandmassMap.SetPixel(x,y, Color.white);
                }
            }
        }
        
        LandmassMap.Apply();
    }
    
    private int GettingNeighbors(int pointX, int pointY) {

        int wallNeighbors = 0;

        for (int x = pointX - 1; x <= pointX + 1; x++) {
            for (int y = pointY - 1; y <= pointY + 1; y++) {
                if (x >= 0 && x < WorldSize && y >= 0 && y < WorldSize) {
                    if (x != pointX || y != pointY) {
                        if (LandmassGrid[x, y] == 1) {
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
        
        for (int x = 0; x < WorldSize; x++) {
            for (int y = 0; y < WorldSize; y++) {
                Color color = CalculatePerlinColor(x, y);
                LandmassMap.SetPixel(x, y, color);
            }
        }
        LandmassMap.Apply();
    }

    Color CalculatePerlinColor(int GridX, int GridY) {
        float PerlinCoordX = (((float)GridX / WorldSize) * LandmassPerlinScale) + WorldSeed;
        float PerlinCoordY = (((float)GridY / WorldSize) * LandmassPerlinScale) + WorldSeed;
        
        float sample = Mathf.PerlinNoise(PerlinCoordX, PerlinCoordY);

        if (sample > LandmassPerlinAreaDivision) {
            sample = 1f;
            LandmassGrid[GridX, GridY] = 1;
        }else{
            sample = 0f;
            LandmassGrid[GridX, GridY] = 0;
        }

        return new Color(sample, sample, sample);
    }

    public void GenLandmassHexaGon() {
        GenLandmassCellular(); //Temporary solution, im just lazy at the moment to do something new...
    }

    public void GenLandmassCustom() {
        GenLandmassPerlin(); //Temporary solution, im just lazy at the moment to do something new...
    }

    
    //BiomeMap Generators
    public void GenBiomesVoronoi() {
        Vector2Int[] centroids = new Vector2Int[WorldBiomes.Length];
        System.Random randChoice = new System.Random(WorldSeed.GetHashCode());
        
        for (int c = 0; c < centroids.Length; c++) {
            centroids[c]  = new Vector2Int(randChoice.Next(0, WorldSize), randChoice.Next(0, WorldSize));
        }
        
        
        Texture2D biomes_VoronoiMap = new Texture2D(WorldSize, WorldSize);
        
        for (int x = 0; x < WorldSize; x++) {
            for (int y = 0; y < WorldSize; y++) {
                float dist = WorldSize;
                int index = 0;
                Vector2Int point = new Vector2Int(x, y);
                for (int c = 0; c < centroids.Length; c++) {
                    if (Vector2.Distance(centroids[c], point) < dist) {
                        dist = Vector2.Distance(centroids[c], point);
                        index = c;
                    }
                }
                Color32 color = CalculateVoronoiColor(point, centroids[index], index);
            
                biomes_VoronoiMap.SetPixel(x, y, color);
            }
        }
        
        biomes_VoronoiMap.Apply();

        switch (DistortionMapMethod) {
            case BiomeDistortionMethod.Perlin:
                
                Texture2D biomes_PerlinMap = new Texture2D(WorldSize, WorldSize);
                for (int x = 0; x < WorldSize; x++) {
                    for (int y = 0; y < WorldSize; y++) {
                        Color color = CalculateBiomesPerlinDistortion(x, y);
                        biomes_PerlinMap.SetPixel(x, y, color);
                    }
                }

                biomes_PerlinMap.Apply();
                for (int b = 0; b < WorldBiomes.Length; b++) {
                    for (int x = 0; x < WorldSize; x++) {
                        for (int y = 0; y < WorldSize; y++) {
                            Vector2Int pos = new Vector2Int(x, y);
                    
                            for (int xB = -1; xB < 2; xB++) {
                                for (int yB = -1; yB < 2; yB++) {
                                    if (x + xB >= 0 && y + yB >= 0 && x + xB <= WorldSize - 1 && y + yB <= WorldSize - 1) {
                                        if (biomes_PerlinMap.GetPixel(x + xB, y + yB) == Color.black && biomes_VoronoiMap.GetPixel(x, y) == WorldBiomes[b].BiomeColor32) {
                                            biomes_VoronoiMap.SetPixel(x + xB, y + yB, WorldBiomes[b].BiomeColor32);
                                        }
                                    }

                                }
                            }
                    
                        }
                    }
                }
                break;
            case BiomeDistortionMethod.JASGCustom:
                for (int c = 0; c < BiomeSmoothCycles; c++) {
                    for (int x = 0; x < WorldSize; x++) {
                        for (int y = 0; y < WorldSize; y++) {
                            Color32 col = DecidePixelBiome(x, y, biomes_VoronoiMap, biomes_VoronoiMap.GetPixel(x,y), randChoice);
                            biomes_VoronoiMap.SetPixel(x,y, col);
                        }
                    }
                }
                break;
        }

        

        BiomesMap.SetPixels(biomes_VoronoiMap.GetPixels());
        BiomesMap.Apply();
        
    }

    Color32 CalculateVoronoiColor(Vector2Int point, Vector2Int CentroidToCompare, int index) {
        float dist = Vector2.Distance(point, CentroidToCompare);

        float distScaled = scale(0f, (float) WorldSize, 0f, 1f, dist);

        Color32 color = WorldBiomes[0].BiomeColor32;
        if (distScaled <= 1f) {
                color = WorldBiomes[index].BiomeColor32;
        }
        
        return color;

    }
    
    Color CalculateBiomesPerlinDistortion(int GridX, int GridY) {
        float PerlinCoordX = (((float)GridX / WorldSize) * BiomesPerlinScale) + WorldSeed;
        float PerlinCoordY = (((float)GridY / WorldSize) * BiomesPerlinScale) + WorldSeed;
        
        float sample = Mathf.PerlinNoise(PerlinCoordX, PerlinCoordY);

        if (sample >= BiomesPerlinAreaDivision) {
            sample = 1f;
        }else{
            sample = 0f;
        }

        return new Color(sample, sample, sample);
    }

    Color32 DecidePixelBiome(int GridX, int GridY, Texture2D VoronoiNoise, Color32 CurrentBiomeColor, System.Random randNum) {
        int PixelsEqualToCenter = 0;
        int PixelsNotEqualToCenter = 0;
        int[,] cell = new int[3, 3];
        for (int x = 0; x < 2; x++) {
            for (int y = 0; y < 2; y++) {
                if (VoronoiNoise.GetPixel(GridX + (x-1), GridY + (y-1)) == CurrentBiomeColor) {
                    PixelsEqualToCenter++;
                    cell[x, y] = 1;
                }else {
                    PixelsNotEqualToCenter++;
                    cell[x, y] = 0;
                }

            }
        }

        Color32 returnColor = CurrentBiomeColor;
        if (PixelsEqualToCenter < BiomeDistortionCellThreshold) {
            Color32[] NotMatchingCol = new Color32[PixelsNotEqualToCenter];
            int countR = 0;
            for (int x = 0; x < 2; x++) {
                for (int y = 0; y < 2; y++) {
                    if(cell[x,y] == 0) {
                        NotMatchingCol[countR] = VoronoiNoise.GetPixel(GridX + (x - 1), GridY + (y - 1));
                        countR++;
                    }
                }
            }

            int decidedColorId = 0;
            for (int i = 0; i < NotMatchingCol.Length; i++) {
               if (i > 0 && i+1 < NotMatchingCol.Length) {
                   if (NotMatchingCol[i].Equals(CurrentBiomeColor)) {
                       if (NotMatchingCol[i - 1].Equals(CurrentBiomeColor) || NotMatchingCol[i + 1].Equals(CurrentBiomeColor)) {
                           decidedColorId = i;
                       }else {
                           decidedColorId = randNum.Next(0, NotMatchingCol.Length);
                       }

                   }else {
                       decidedColorId = randNum.Next(0, NotMatchingCol.Length);
                   }

               }
            }
            
            return NotMatchingCol[decidedColorId];
        }

        return returnColor;
    }

    public void GenBiomesCustom() {
        GenBiomesVoronoi();
    }

    public void GenRsources() {
        //ResourcesMap, decide positions
        System.Random randChoice = new System.Random(WorldSeed.GetHashCode());
        for (int x = 0; x < WorldSize; x++) {
            for (int y = 0; y < WorldSize; y++) {
                if (LandmassGrid[x, y] == 1) {
                    for (int b = 0; b < WorldBiomes.Length; b++) {
                        if (BiomesMap.GetPixel(x, y) == WorldBiomes[b].BiomeColor32) {
                            if (randChoice.Next(0, 100) < ResourceFillPercent) {
                                if (WorldBiomes[b].BiomeResourceEntries.Length == 1) {
                                    WorldResourceGrid[x, y] = WorldBiomes[b].BiomeResourceEntries[0];
                                }else if (WorldBiomes[b].BiomeResourceEntries.Length > 1) {
                                    int choice = randChoice.Next(0, WorldBiomes[b].BiomeResourceEntries.Length);
                                    if (choice < WorldBiomes[b].BiomeResourceEntries.Length) {
                                        WorldResourceGrid[x, y] = (BiomeResourceEntry)choice+1;
                                    }
                                }
                            } else {
                                WorldResourceGrid[x, y] = BiomeResourceEntry.None;
                            }
                        }
                    }
                }

            }
        }

        
        //Pre-generating the assignments, so we wont have to do that on a chunk load 
        for (int i = 0; i < ResourceAssignEntries.Length; i++) {
            ResourceAssignEntries[i].ID = i;
        }
        
        WorldResourcesAssigned = new int[WorldSize, WorldSize];
        
        for (int x = 0; x < WorldSize; x++) {
            for (int y = 0; y < WorldSize; y++) {
                for (int i = 0; i < ResourceAssignEntries.Length; i++) {
                    if (ResourceAssignEntries[i].EnumResource == WorldResourceGrid[x, y]) {
                        WorldResourcesAssigned[x, y] = ResourceAssignEntries[i].ID;
                    }
                }
                
            }
        }
        
    }


    //Creating the actual world made of tiles
    public void MapChunksToWorld() {
        //ChunkStateGrid mapping
        for (int chunkX = 0; chunkX < WorldSize/ChunkSize; chunkX++) {
            for (int chunkY = 0; chunkY < WorldSize/ChunkSize; chunkY++) {
                ChunkStateGrid[chunkX, chunkY] = 0;
            }
        }
        
        //WorldTileGrid mapping, based on landmass and biome maps
        for (int x = 0; x < WorldSize; x++) {
            for (int y = 0; y < WorldSize; y++) {
                if (LandmassGrid[x, y] == 1) {
                    for (int b = 0; b < WorldBiomes.Length; b++) {
                        if (BiomesMap.GetPixel(x, y) == WorldBiomes[b].BiomeColor32) {
                            WorldTileGrid[x, y] = WorldBiomes[b].BiomeSurfaceRuleTile;
                        }

                    }
                }
                
            }
        }
        
        //Creating the loaded chunk list, for later chunk unloading
        LoadedChunks = new List<WorldChunk>();
    }

    public void DetermineIntialPlayerSpanwPoint() {
        float dist = WorldSize * WorldSize * 10;
        int ProperSpawnX = 0;
        int ProperSpawnY = 0;

        for (int x = 0; x < WorldSize; x++) {
            for (int y = 0; y < WorldSize; y++) {
                if (LandmassGrid[x, y] == 1) {
                    if (Vector2.Distance(new Vector2(x, y), new Vector2(WorldOffset, WorldOffset)) < dist){
                        dist = Vector2.Distance(new Vector2(x, y), new  Vector2(WorldOffset, WorldOffset));
                        ProperSpawnX = x;
                        ProperSpawnY = y;
                    }
                }
            }
        }
        
        Player.gameObject.transform.position = new Vector3((ProperSpawnX-WorldOffset)-0.5f, (ProperSpawnY-WorldOffset)-0.5f);
    }

    public void GenHeatMap() {
        
        //Fill the grid with initial values
        for (int x = 0; x < WorldSize; x++) {
            for (int y = 0; y < WorldSize; y++) {
                for (int b = 0; b < WorldBiomes.Length; b++) {
                    if (BiomesMap.GetPixel(x, y) == WorldBiomes[b].BiomeColor32) {
                        HeatMapGrid[x, y] = WorldBiomes[b].Temperature;
                    }
                }
            }
        }
        
        //Blend the grid values for seamless transitions
        float[,] tempGrid = new float[WorldSize, WorldSize];
        for (int x = 0; x < WorldSize; x++) {
            for (int y = 0; y < WorldSize; y++) {
                float num = 0;
                int numsGiven = 0;
                for (int bx = -1; bx < 1; bx++) {
                    for (int by = -1; by < 1; by++) {
                        if (x > 0 && x < WorldSize && y > 0 && y < WorldSize) {
                            num =+ HeatMapGrid[x + bx, y + by];
                            numsGiven++;
                        }
                    }
                }
                tempGrid[x, y] = num/numsGiven;
            }
        }

        for (int x = 0; x < WorldSize; x++) {
            for (int y = 0; y < WorldSize; y++) {
                HeatMapGrid[x, y] = tempGrid[x, y];
            }
        }

    }

    public void GenMinimap() {
        
        MinimapBackground = new Texture2D(WorldSize, WorldSize);
        Color MAP_background = new Color(243f / 255f, 212f / 255f, 144f / 255f);
        Color MAP_landmass = new Color(204f / 255f, 173f / 255f, 106f / 255f);
        Color MAP_landmass_side = new Color(102f / 255f, 81f / 255f, 61f / 255f);
        Color MAP_landmass_aa = new Color(156f / 255f, 129f / 255f, 89f / 255f);

        for (int x = 0; x < WorldSize; x++) {
            for (int y = 0; y < WorldSize; y++) {
                MinimapBackground.SetPixel(x,y, MAP_background);
            }
        }
        
        for (int x = 0; x < WorldSize; x++) {
            for (int y = 0; y < WorldSize; y++) {
                if (LandmassGrid[x, y] == 1) {
                    MinimapBackground.SetPixel(x,y, MAP_landmass);
                    if (x > 0 && y > 0 && x < WorldSize-1 && y < WorldSize-1) {
                        //Sides
                        if (LandmassGrid[x, y-1] == 0) {
                            MinimapBackground.SetPixel(x,y-1, MAP_landmass_side);
                        }
                        if (y > 1) {
                            if (LandmassGrid[x, y-2] == 0) {
                                MinimapBackground.SetPixel(x,y-2, MAP_landmass_side);
                            }
                        }
                        if (LandmassGrid[x, y+1] == 0) {
                            MinimapBackground.SetPixel(x,y+1, MAP_landmass_side);
                        }
                        if (LandmassGrid[x-1, y] == 0) {
                            MinimapBackground.SetPixel(x-1,y, MAP_landmass_side);
                        }
                        if (LandmassGrid[x+1, y] == 0) {
                            MinimapBackground.SetPixel(x+1,y, MAP_landmass_side);
                        }
                        
                        //AntiAliasing
                        if (LandmassGrid[x-1,y] == 0 && LandmassGrid[x+1,y] == 1) {
                            if (LandmassGrid[x,y-1] == 0 && LandmassGrid[x,y+1] == 1) {
                                MinimapBackground.SetPixel(x,y, MAP_landmass_aa);
                            }
                            if (LandmassGrid[x,y+1] == 0 && LandmassGrid[x,y-1] == 1) {
                                MinimapBackground.SetPixel(x,y, MAP_landmass_aa);
                            }
                        }
                        if (LandmassGrid[x+1,y] == 0 && LandmassGrid[x-1,y] == 1) {
                            if (LandmassGrid[x,y-1] == 0 && LandmassGrid[x,y+1] == 1) {
                                MinimapBackground.SetPixel(x,y, MAP_landmass_aa);
                            }
                            if (LandmassGrid[x,y+1] == 0 && LandmassGrid[x,y-1] == 1) {
                                MinimapBackground.SetPixel(x,y, MAP_landmass_aa);
                            }
                        }
                        
                        
                    }
                }
            }
        }
        
        MinimapBackground.Apply();
        
        //Putting it to display
        DisplayMap.SetPixels(MinimapBackground.GetPixels());
        DisplayMap.Apply();
        
    }

    //Saveing and Loading
    public void InitSaveWorld() {
        //WorldInfoData
        string date = System.DateTime.Now.ToString(" yyyy.MM.dd  HH.mm.ss");
        WorldDataInfoSaveObject.CreationDate = date;
        WorldDataInfoSaveObject.WorldVersion = "V4";
        WorldDataInfoSaveObject.LastOpenedDate = date;
        WorldDataInfoSaveObject.LandmassGenMethod = LandmassGenType;
        WorldDataInfoSaveObject.BiomeGenMethod = BiomeGenType;
        WorldDataInfoSaveObject.BiomeDistortionMethod = DistortionMapMethod;
        WorldDataInfoSaveObject.GeneratedByRandomSeed = GenRandomSeed;
        WorldDataInfoSaveObject.NumberOfDifferentBiomes = WorldBiomes.Length;
        
        //WorldGenData
        WorldDataGenSaveObject.WorldSize = WorldSize;
        WorldDataGenSaveObject.WorldSeed = WorldSeed;
        WorldDataGenSaveObject.ChunkSize = ChunkSize;
        WorldDataGenSaveObject.WorldResourcesSerialised = new BiomeResourceEntry[WorldSize*WorldSize];
        for (int x = 0; x < WorldSize; x++) {
            for (int y = 0; y < WorldSize; y++) {
                WorldDataGenSaveObject.WorldResourcesSerialised[x+(y*WorldSize)] = WorldResourceGrid[x, y];
            }
        }
        WorldDataGenSaveObject.WorldHeatMap = new float[WorldSize*WorldSize];
        for (int x = 0; x < WorldSize; x++) {
            for (int y = 0; y < WorldSize; y++) {
                WorldDataGenSaveObject.WorldHeatMap[x+(y*WorldSize)] = HeatMapGrid[x, y];
            }
        }
        //WorldRunData
        
        WorldDataRunSaveObject.comment1 = "Runtime Data";
        WorldDataRunSaveObject.PlayerCoordX = Player.transform.position.x;
        WorldDataRunSaveObject.PlayerCoordY = Player.transform.position.y;
        WorldDataRunSaveObject.comment2 = "Game Rules";
        WorldDataRunSaveObject.GameRule_DoDaylightCycle = false;
        
        string SaveWorldDataInfo = JsonUtility.ToJson(WorldDataInfoSaveObject, true);
        File.WriteAllText(WorldFolder + "info_" + WorldName + ".json", SaveWorldDataInfo);
        Debug.Log("World Gen Data Saved!");
        
        string SaveWorldDataGen = JsonUtility.ToJson(WorldDataGenSaveObject, true);
        File.WriteAllText(WorldDataFolder + "gen_" + WorldName + ".json", SaveWorldDataGen);
        Debug.Log("World Gen Data Saved!");
        
        string SaveWorldDataRun = JsonUtility.ToJson(WorldDataRunSaveObject, true);
        File.WriteAllText(WorldDataFolder + "run_" + WorldName + ".json", SaveWorldDataRun);
        Debug.Log("World Gen Data Saved!");

        var bytes_Landmass = LandmassMap.EncodeToPNG();
        var bytes_Biomes = BiomesMap.EncodeToPNG();
        var bytes_MinimapBG = MinimapBackground.EncodeToPNG();
        
        File.WriteAllBytes(WorldMapFolder + "map_landmass.png", bytes_Landmass);
        File.WriteAllBytes(WorldMapFolder + "map_biomes.png", bytes_Biomes);
        File.WriteAllBytes(WorldMapFolder + "map_minimap_bg.png", bytes_MinimapBG);
        Debug.Log("World Maps Saved!");
    }


    //RUNTIME
    public void LocatePlayer() {
        var position = Player.transform.position;
        run_PlayerWorldPosX = (int)position.x + WorldOffset;
        run_PlayerWorldPosY = (int)position.y + WorldOffset;
        
        if (run_PlayerChunkX >= 0) {
            run_PlayerChunkX = (int)Mathf.Ceil(run_PlayerWorldPosX / ChunkSize);
        } else {
            run_PlayerChunkX = (int)Mathf.Floor(run_PlayerWorldPosX / ChunkSize) - 1;
        }

        if (run_PlayerChunkY >= 0) {
            run_PlayerChunkY = (int)Mathf.Ceil(run_PlayerWorldPosY / ChunkSize);
        } else {
            run_PlayerChunkY = (int)Mathf.Floor(run_PlayerWorldPosY / ChunkSize)-1;
        }

    }

    public void LoadChunk(int ChunkX, int ChunkY) {
        if (ChunkStateGrid[ChunkX, ChunkY] == 0) {
            ChunkStateGrid[ChunkX, ChunkY] = 1;
            GameObject ChunkObject = new GameObject();
            ChunkObject.name = "Chunk_" + ChunkX + "_" + ChunkY;
            ChunkObject.transform.position = new Vector3(ChunkX * ChunkSize, ChunkY * ChunkSize);
            ChunkObject.transform.SetParent(ChunkResourceHolder.transform);
            
            for (int cX = 0; cX < ChunkSize; cX++) {
                for (int cY = 0; cY < ChunkSize; cY++) {
                    int GridCoordX = ChunkX * ChunkSize + cX;
                    int GridCoordY = ChunkY * ChunkSize + cY;
                    int TileCoordX = GridCoordX - WorldOffset;
                    int TileCoordY = GridCoordY - WorldOffset;

                    if (WorldTileGrid[GridCoordX, GridCoordY] != null) {
                        GridLand.SetTile(new Vector3Int(TileCoordX, TileCoordY, 0), WorldTileGrid[GridCoordX, GridCoordY]);
                    }

                    GridWater.SetTile(new Vector3Int(TileCoordX, TileCoordY, 0), WaterTile);
                    GridSeafloor.SetTile(new Vector3Int(TileCoordX, TileCoordY, 0), SeaFloorTile);
                    if (LandmassGrid[GridCoordX, GridCoordY] == 0) {
                        GridWaterCollision.SetTile(new Vector3Int(TileCoordX, TileCoordY, 0), ColliderTile);
                    }

                    if (WorldResourceGrid[GridCoordX, GridCoordY] != BiomeResourceEntry.None) {
                        GameObject ResObj = Instantiate(ResourceAssignEntries[WorldResourcesAssigned[GridCoordX,GridCoordY]].GameObjResource);
                        ResObj.name = "" + ResourceAssignEntries[WorldResourcesAssigned[GridCoordX,GridCoordY]].GameObjResource.name + "_X" + GridCoordX + "_Y" + GridCoordY;
                        ResObj.transform.position = new Vector3(TileCoordX-0.5f, TileCoordY-0.5f, 99);
                        ResObj.transform.SetParent(ChunkObject.transform);
                    }

                }
            }

            ChunkStateGrid[ChunkX, ChunkY] = 2;
            LoadedChunks.Add(new WorldChunk(ChunkX, ChunkY));

        }
    }

    public void UnloadChunk(int ChunkX, int ChunkY, int LoadedChunkToRemove) {
        if (ChunkStateGrid[ChunkX, ChunkY] == 2) {
            ChunkStateGrid[ChunkX, ChunkY] = 1;

            for (int x = 0; x < ChunkSize; x++) {
                for (int y = 0; y < ChunkSize; y++) {
                    int GridCoordX = ChunkX * ChunkSize + x;
                    int GridCoordY = ChunkY * ChunkSize + y;
                    int TileCoordX = GridCoordX - WorldOffset;
                    int TileCoordY = GridCoordY - WorldOffset;
                    
                    GridLand.SetTile(new Vector3Int(TileCoordX, TileCoordY, 0), null);
                    GridWater.SetTile(new Vector3Int(TileCoordX, TileCoordY, 0), null);
                    GridSeafloor.SetTile(new Vector3Int(TileCoordX, TileCoordY, 0), null);
                    GridWaterCollision.SetTile(new Vector3Int(TileCoordX, TileCoordY, 0), null);
                    
                    Destroy(GameObject.Find("Chunk_" + ChunkX + "_" + ChunkY));
                    
                }
            }
            
            ChunkStateGrid[ChunkX, ChunkY] = 0;
            LoadedChunks.Remove(LoadedChunks[LoadedChunkToRemove]);
        }

        isChunkUnloading = false;

    }

    public void TickWorld() {
        
    }

    //UTILITY
    public float scale(float MITmin, float MITmax, float MIREmin, float MIREmax, float MIT) {

        float OldRange = (MITmax - MITmin);
        float NewRange = (MIREmax - MIREmin);
        float NewValue = (((MIT - MITmin) * NewRange) / OldRange) + MIREmin;

        return (NewValue);
    }
    
}
