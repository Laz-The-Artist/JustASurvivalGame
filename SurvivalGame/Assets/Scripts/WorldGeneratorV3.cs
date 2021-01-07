using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using TMPro;

public class WorldGeneratorV3 : MonoBehaviour {
    
    
        GlobalVariableHandler GVH;
    [Header("Worldgen References")]
    [Space]
    [Header("JAWG - Just A World Engine")]
        public GameObject Player;
        public Tilemap GridLandmass;
        public Tilemap GridLandmass_collider;
        public Tilemap GridLandmass_;
        public Tilemap GridLandmass__;
        public GameObject ResourcesLayer;
        public Renderer map_display;
        public Tile ColliderTile;

    [Header("Main Settings")]
        public string SettingWorldName;
        public bool LoadExistingWorld;
        public bool ReadStartMenuSettingToGenerate = true;
        public bool GenRandomSeed = true;
        [Range(0, 999999)] public int WorldSeed;
        public int SettingWorldSize = 512; //always use a number that is a power of 2; otherwise things WILL go wrong
        public int SettingWorldOffset = 0;
        public int SettingChunkSize = 16; //always use a number that is a power of 2; otherwise things WILL go wrong
        [Range(1, 4)] public int SettingChunkLoadingRadius = 2;
        [Range(4, 8)] public int SettingChunkUnloadDistance = 4;

    [Header("Cellular Automata Settings")]
        public bool GenCellularMap = true;
        public int CellularSmoothCycles;
        [Range(0, 100)] public int CellularFillPercent;
        [Range(0, 8)] public int CellularTreshold;

    [Header("Biome Generator Settings")]
        public bool SettingGenBiomes = true;
        public bool SettingGenResources = true;
        [Range(0, 100)] public int ResourceFillPercent;
        public AnimatedTile WaterTile;
        public Tile Seafloor;
        public float SettingVoronoiSmallestDst;
        public float SettingPerlinScale;
        [Range(0, 1)] public float SettingPerlinMinDivisionValue = 0.55f;
        public WorldBiomes[] BiomesList;
        public WorldBiomes[] BiomesListModdable;

    [Header("Day-Night Cycle Settings")]
        public bool SettingCycleDayNight = true; //this grants the power of ZA WARUDO
        public GameObject WorldGlobalLight;
        UnityEngine.Experimental.Rendering.Universal.Light2D WorldGlobalLight2D;
        public float CurrentWorldTimeMinutes;
        [HideInInspector] public float CurrentWorldTimeMinutesCounter;
        [HideInInspector] public float ScaledCurrentWorldTimeMinutesCounter;
        public float CurrentWorldTimeHours;
        public float CurrentWorldTimeDays;
        public TextMeshProUGUI worldTimeDisp;
        public string CurrentDaytime;

    [Header("World Runtime")]
        public string CurrentBiomeName;
        public int CurrentBiomeTemp;


    [HideInInspector] public Texture2D gen_VoronoiMap;
    [HideInInspector] public Texture2D gen_PerlinMap;

    [HideInInspector] public Texture2D map_Landmass;
    [HideInInspector] public Texture2D map_Biomes;
    [HideInInspector] public Texture2D map_Resources;
    [HideInInspector] public Texture2D map_Minimap;

    [HideInInspector] public string SettingWorldPath;
    [HideInInspector] public string WorldMapPath;
    [HideInInspector] public string WorldDataPath;


    //SaveClasses
    [HideInInspector] public JAWGSaveWorldGenData SaveWorldGenData;
    [HideInInspector] public JAWGSaveWorldRuntimeData SaveWorldRuntimeData;
    [HideInInspector] public SaveBiomes SaveBiomeData;

    [HideInInspector] public string ResourceGameLocation;
    [HideInInspector] public string[] ModsLoc;
    [HideInInspector] public string[] ModsNames;
    [HideInInspector] public string GameModsLocation;


    int WorldSizeX;
    int WorldSizeY;
    int WorldOffsetX;
    int WorldOffsetY;
    int NumberOfWorldChunks;
    private int[,] WorldChunks;
    private int[,] CellularWorldPoints;
    [HideInInspector] public int[,] RandomResourcePoints;
    Vector2Int[] centroids;
    Color Transparent = new Color(0f, 0f, 0f, 0f);

    int PlayerWorldPosX;
    int PlayerWorldPosY;
    int PlayerChunkX;
    int PlayerChunkY;

    [HideInInspector] public bool IsWorldComplete = false;


    //Where it all begins
    private void Awake() {
        ResourceGameLocation = Directory.GetCurrentDirectory() + "/GameResources/";
        if (!Directory.Exists(Application.persistentDataPath + "/saves/")) {
            Directory.CreateDirectory(Application.persistentDataPath + "/");
        }
        if (!Directory.Exists(Application.persistentDataPath + "/mods/")) {
            Directory.CreateDirectory(Application.persistentDataPath + "/");
        }
        if (!Directory.Exists(ResourceGameLocation)) {
            Directory.CreateDirectory(ResourceGameLocation);
        }


        SaveBiomeData.s_BiomesList = BiomesList;

        string jsonSaveBiomes = JsonUtility.ToJson(SaveBiomeData, true);
        File.WriteAllText(ResourceGameLocation + "biomes_0.0.0.json", jsonSaveBiomes);
        Debug.Log("World_Biomes Json Saved Succesfully");

        LoadModdables();
    }

    void Start() {
        //Initialise the sun 
        WorldGlobalLight2D = WorldGlobalLight.GetComponent<UnityEngine.Experimental.Rendering.Universal.Light2D>();

        GameObject tmp = GameObject.FindWithTag("GlobalReference");
        if (tmp != null) {
            GVH = tmp.GetComponent<GlobalVariableHandler>();
            LoadExistingWorld = GVH.loadExisting;
            ReadStartMenuSettingToGenerate = GVH.ReadStartSettingsForGen;
        }
        
        if (LoadExistingWorld) {
            if (ReadStartMenuSettingToGenerate) { SettingWorldName = GVH.LoadworldName; }
            SettingWorldPath = Application.persistentDataPath + "/saves/" + SettingWorldName + "/";
            WorldMapPath = SettingWorldPath + "/map_" + SettingWorldName + "/";
            WorldDataPath = SettingWorldPath + "/data_" + SettingWorldName + "/";
            LoadWorld();
        } else {
            if (ReadStartMenuSettingToGenerate) { SettingWorldName = GVH.GenworldName; }
            if (!Directory.Exists(Application.persistentDataPath + "/saves/" + SettingWorldName + "/")) {
                SettingWorldPath = Application.persistentDataPath + "/saves/" + SettingWorldName + "/";
            } else {
                string time = System.DateTime.Now.ToString("_yyyy.MM.dd_HH.mm.ss");
                SettingWorldPath = Application.persistentDataPath + "/saves/" + SettingWorldName + time + "/";
                SettingWorldName = SettingWorldName + time;
                Debug.LogWarning("This world location already exists. To prevent overwriting, the current time was added to the world's name: " + SettingWorldName);
            }

            WorldMapPath = SettingWorldPath + "/map_" + SettingWorldName + "/";
            WorldDataPath = SettingWorldPath + "/data_" + SettingWorldName + "/";

            Directory.CreateDirectory(SettingWorldPath);
            Directory.CreateDirectory(WorldMapPath);
            Directory.CreateDirectory(WorldDataPath);

            GenerateNewWorld();
        }

        if (CurrentWorldTimeHours >= 12) {
            CurrentWorldTimeMinutesCounter = ((CurrentWorldTimeHours-12) * 60) + CurrentWorldTimeMinutes;
        } else {
            CurrentWorldTimeMinutesCounter = (CurrentWorldTimeHours * 60) + CurrentWorldTimeMinutes;
        }
        

    }

    void FixedUpdate() {

        LocatePlayer(PlayerWorldPosX, PlayerWorldPosY);

        //Chunkloading around the player
        if (WorldChunks[((SettingWorldSize / SettingChunkSize) / 2) + PlayerChunkX, ((SettingWorldSize / SettingChunkSize) / 2) + PlayerChunkY] == 0) {
            LoadChunk(PlayerChunkX, PlayerChunkY);
            for (int RenderDistanceX = 0; RenderDistanceX < SettingChunkLoadingRadius; RenderDistanceX++) {
                for (int RenderDistanceY = 0; RenderDistanceY < SettingChunkLoadingRadius; RenderDistanceY++) {
                    //Vertical and horizontal "expansion"
                    LoadChunk(PlayerChunkX, PlayerChunkY + RenderDistanceY);
                    LoadChunk(PlayerChunkX, PlayerChunkY - RenderDistanceY);
                    LoadChunk(PlayerChunkX - RenderDistanceX, PlayerChunkY);
                    LoadChunk(PlayerChunkX + RenderDistanceX, PlayerChunkY);
                    //Diagonal "expansion"
                    LoadChunk(PlayerChunkX + RenderDistanceX, PlayerChunkY + RenderDistanceY);
                    LoadChunk(PlayerChunkX - RenderDistanceX, PlayerChunkY - RenderDistanceY);
                    LoadChunk(PlayerChunkX - RenderDistanceX, PlayerChunkY + RenderDistanceY);
                    LoadChunk(PlayerChunkX + RenderDistanceX, PlayerChunkY - RenderDistanceY);
                }
            }
        }

        //Unloading chunks
        //i have no idea how to unload chunks
        /*foreach (int i in WorldChunks) {
            if (i == 1 && Vector2Int.Distance(new Vector2Int(PlayerChunkX,PlayerChunkY), i need the coords of i here and it should work) >= SettingChunkUnloadDistance) {
                UnloadChunk(THE COORD.X of i, THE COORD.Y of i);
            }
        }*/

        //Day-Night Cycle
        if (SettingCycleDayNight) {
            CycleDayNight();
        }
    }



    public void GenerateNewWorld() {

        InitializeWorld(false);

        if (GenCellularMap) {
            GenLandmassCellular();
            MapChunksToWorld();
        }
        if (SettingGenBiomes) {
            GenerateBiomeMap();
        }
        if (SettingGenResources) {
            GenResources();
        }

        IntSaveWorldData();

        IsWorldComplete = true;
    }

    public void LoadWorld() {

        IntLoadWorldData();

        InitializeWorld(true);

        MapChunksToWorld();

        IsWorldComplete = true;
    }


    public void InitializeWorld(bool isLoadingExisting) {

        if (!isLoadingExisting) {
            if (ReadStartMenuSettingToGenerate && GVH != null) {
                WorldSeed = GVH.seed;
                GenRandomSeed = GVH.genRandomSeed;
            }

            //set initial values that are required
            WorldSizeX = SettingWorldSize;
            WorldSizeY = SettingWorldSize;

            WorldOffsetX = SettingWorldOffset;
            WorldOffsetY = SettingWorldOffset;
            NumberOfWorldChunks = SettingWorldSize * SettingWorldSize / SettingChunkSize;
            Debug.Log(NumberOfWorldChunks + " Chunks will be generated");
            WorldChunks = new int[SettingWorldSize / SettingChunkSize, SettingWorldSize / SettingChunkSize];

            RandomResourcePoints = new int[WorldSizeX, WorldSizeY];
            CellularWorldPoints = new int[WorldSizeX, WorldSizeY];

            centroids = new Vector2Int[BiomesList.Length];

            //Generate the random seed, if its set to generate one

            if (GenRandomSeed) {
                int RandomSeed = UnityEngine.Random.Range(0, 100000);
                WorldSeed = RandomSeed;
            }

            //start mapping; making the textures for maps; set the filter mode to point
            gen_VoronoiMap = new Texture2D(WorldSizeX, WorldSizeY);
            gen_PerlinMap = new Texture2D(WorldSizeX, WorldSizeY);

            map_Landmass = new Texture2D(WorldSizeX, WorldSizeY);
            map_Biomes = new Texture2D(WorldSizeX, WorldSizeY);
            map_Resources = new Texture2D(WorldSizeX, WorldSizeY);

            gen_VoronoiMap.filterMode = FilterMode.Point;
            gen_PerlinMap.filterMode = FilterMode.Point;

            map_Landmass.filterMode = FilterMode.Point;
            map_Biomes.filterMode = FilterMode.Point;
            map_Resources.filterMode = FilterMode.Point;
            //map_Minimap.filterMode = FilterMode.Point;

            //Input map_ in the Sprite Renderer; Displaying in-world.
            Sprite maprendersprite = Sprite.Create(map_Biomes, new Rect(0.0f, 0.0f, WorldSizeX, WorldSizeY), new Vector2(0.5f, 0.5f), 100.0f);
            map_display.GetComponent<SpriteRenderer>().sprite = maprendersprite;

            Debug.Log("World Initialised succesfully!");

        } else if(isLoadingExisting) {

            GenRandomSeed = false;

            //set initial values that are required
            WorldSizeX = SettingWorldSize;
            WorldSizeY = SettingWorldSize;
            WorldOffsetX = SettingWorldOffset;
            WorldOffsetY = SettingWorldOffset;
            NumberOfWorldChunks = SettingWorldSize * SettingWorldSize / SettingChunkSize;
            Debug.Log(NumberOfWorldChunks + " Chunks will be generated");
            WorldChunks = new int[SettingWorldSize / SettingChunkSize, SettingWorldSize / SettingChunkSize];

            //Input map_ in the Sprite Renderer; Displaying in-world.
            Sprite maprendersprite = Sprite.Create(map_Biomes, new Rect(0.0f, 0.0f, WorldSizeX, WorldSizeY), new Vector2(0.5f, 0.5f), 100.0f);
            map_display.GetComponent<SpriteRenderer>().sprite = maprendersprite;

            Debug.Log("World Initialised succesfully!");
        }
        

    }

    //CellularAutomata based landmass generator
    public void GenLandmassCellular() {
        //Seed generation
        System.Random randChoice = new System.Random(WorldSeed.GetHashCode());
        for (int x = 0; x < WorldSizeX; x++) {
            for (int y = 0; y < WorldSizeY; y++) {
                if (randChoice.Next(0, 100) < CellularFillPercent) {
                    CellularWorldPoints[x, y] = 1;
                } else {
                    CellularWorldPoints[x, y] = 0;
                }

            }
        }

        //Cellular Automata function - Smoothing cycles
        for (int i = 0; i < CellularSmoothCycles; i++) {
            for (int x = 0; x < WorldSizeX; x++) {
                for (int y = 0; y < WorldSizeY; y++) {

                    int neighboringWalls = GettingNeighbors(x, y);

                    if (neighboringWalls > CellularTreshold) {
                        CellularWorldPoints[x, y] = 1;
                    } else if (neighboringWalls < CellularTreshold) {
                        CellularWorldPoints[x, y] = 0;
                    }
                }
            }
        }

        Debug.Log("Landmass map generated Succesfully");
    }

    //Cellular Automata function - Smoothing cycles - Neighbor Cell check
    private int GettingNeighbors(int pointX, int pointY) {

        int wallNeighbors = 0;

        for (int x = pointX - 1; x <= pointX + 1; x++) {
            for (int y = pointY - 1; y <= pointY + 1; y++) {
                if (x >= 0 && x < WorldSizeX && y >= 0 && y < WorldSizeY) {
                    if (x != pointX || y != pointY) {
                        if (CellularWorldPoints[x, y] == 1) {
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

    //Generate Biome Map by Voronoi Noise And Perlin Noise
    public void GenerateBiomeMap() {

        //making the voronoi noise
        gen_VoronoiMap.SetPixels32(GenVoronoiV2());
        gen_VoronoiMap.Apply();

        //making the perlin noise
        for (int x = 0; x < WorldSizeX; x++) {
                for (int y = 0; y < WorldSizeY; y++) {
                    Color color = CalcPerlin(x, y);
                    gen_PerlinMap.SetPixel(x, y, color);
                }
            }
        gen_PerlinMap.Apply();

        //Combining the VoronoiMap and PerlinMap
        GenTestBiomeNoise();

    }

    Color CalcPerlin(int PerlX, int PerlY) {
        float xCoord = (((float)PerlX / WorldSizeX) * SettingPerlinScale) + WorldSeed;
        float yCoord = (((float)PerlY / WorldSizeY) * SettingPerlinScale) + WorldSeed;

        float sample = Mathf.PerlinNoise(xCoord, yCoord);
        if (sample >= SettingPerlinMinDivisionValue) {
            sample = 1;
        } else if (sample < SettingPerlinMinDivisionValue) {
            sample = 0;
        }
        return new Color(sample, sample, sample);
    }

    //Voronoi Noise
    public Color32[] GenVoronoiV2() {
        System.Random randChoice = new System.Random(WorldSeed.GetHashCode());
        Color32[] regions = new Color32[BiomesList.Length];
        for (int BiomLength = 0; BiomLength < BiomesList.Length; BiomLength++) {
            centroids[BiomLength] = new Vector2Int(randChoice.Next(0, WorldSizeX), randChoice.Next(0, WorldSizeY));
            regions[BiomLength] = BiomesList[BiomLength].BiomeColor32;
        }
        Color32[] PixelColors = new Color32[WorldSizeX * WorldSizeY];
        for (int x = 0; x < WorldSizeX; x++) {
            for (int y = 0; y < WorldSizeY; y++) {
                int index = x * WorldSizeY + y;
                PixelColors[index] = regions[GenCentroidIndex(new Vector2Int(x, y), centroids)];
            }
        }

        return PixelColors;
    }

    int GenCentroidIndex(Vector2Int PixelPos, Vector2Int[] centroids) {
        float smallestDst = SettingVoronoiSmallestDst;
        int index = 0;
        for (int i = 0; i < centroids.Length; i++) {
            if (Vector2.Distance(PixelPos, centroids[i]) < smallestDst) {
                smallestDst = Vector2.Distance(PixelPos, centroids[1]);
                index = i;
            }
        }
        return index;
    }

    //TestBiomeNoise
    public void GenTestBiomeNoise() {
        for (int BiomeNum = 0; BiomeNum < BiomesList.Length; BiomeNum++) {

            for (int x = 0; x < WorldSizeX; x++) {
                for (int y = 0; y < WorldSizeY; y++) {

                    for (int xB = -1; xB < 2; xB++) {
                        for (int yB = -1; yB < 2; yB++) {

                            if (x + xB >= 0 && y + yB >= 0 && x + xB <= WorldSizeX - 1 && y + yB <= WorldSizeY - 1) {
                                if (gen_PerlinMap.GetPixel(x + xB, y + yB) == Color.black && gen_VoronoiMap.GetPixel(x, y) == BiomesList[BiomeNum].BiomeColor32) {
                                    gen_VoronoiMap.SetPixel(x + xB, y + yB, BiomesList[BiomeNum].BiomeColor32);
                                }
                            }

                        }
                    }

                }
            }

        }
        gen_VoronoiMap.Apply();
        map_Biomes.SetPixels(gen_VoronoiMap.GetPixels());
        map_Biomes.Apply();

        for (int x = 0; x < WorldSizeX; x++) {
            for (int y = 0; y < WorldSizeY; y++) {
                if (CellularWorldPoints[x, y] == 0) {
                    map_Landmass.SetPixel(x, y, Color.white);
                } else if (CellularWorldPoints[x, y] == 1){
                    map_Landmass.SetPixel(x, y, Color.black);
                }
            }
        }

        map_Landmass.Apply();

    }

    public void MapChunksToWorld() {
        for (int chunkX = 0; chunkX < WorldSizeX/SettingChunkSize; chunkX++) {
            for (int chunkY = 0; chunkY < WorldSizeY/SettingChunkSize; chunkY++) {

                WorldChunks[chunkX, chunkY] = 0;

                //This down here, it stays. what if i'd need it in the future?
                /**for (int iX = 0; iX < SettingChunkSize; iX++) {
                    for (int iY = 0; iY < SettingChunkSize; iY++) {

                        //This here generates the whole world
                        if (CellularWorldPoints[chunkX * SettingChunkSize + iX, chunkY * SettingChunkSize + iY] == 1) {
                            
                            //this is nice, but i want the world to load around the player; this generates like how it used to but with an extra step and less lag;
                            GridLandmass.SetTile(new Vector3Int( (chunkX * SettingChunkSize - WorldOffsetX)+ iX, (chunkY * SettingChunkSize - WorldOffsetY)+ iY, 0), GrassForTest);
                            //map_Landmass.SetPixel((chunkX * SettingChunkSize) + iX, (chunkY * SettingChunkSize) + iY, Color.black);
                        }
                    }
                }**/

            }
        }
        Debug.Log("Mapped Chunks");
    }

    public void GenResources() {
        System.Random randChoice = new System.Random(WorldSeed.GetHashCode());
        for (int x = 0; x < WorldSizeX; x++) {
            for (int y = 0; y < WorldSizeY; y++) {
                if (randChoice.Next(0, 1000) < ResourceFillPercent) {
                    map_Resources.SetPixel(x,y,Color.red);
                } else {
                    map_Resources.SetPixel(x, y, Color.white);
                }

            }
        }
    }

    public void IntSaveWorldData() {

        SaveWorldGenData.s_WorldSize = SettingWorldSize;
        SaveWorldGenData.s_WorldOffset = SettingWorldOffset;
        SaveWorldGenData.s_ChunkSize = SettingChunkSize;
        SaveWorldGenData.s_WorldSeed = WorldSeed;

        SaveWorldRuntimeData.s_CurrentMinutes = CurrentWorldTimeMinutes;
        SaveWorldRuntimeData.s_CurrentHours = CurrentWorldTimeHours;
        SaveWorldRuntimeData.s_CurrentDays = CurrentWorldTimeDays;
        SaveWorldRuntimeData.s_GameRuleDoDayCycle = SettingCycleDayNight;

        string jsonSaveGenData = JsonUtility.ToJson(SaveWorldGenData, true);
        File.WriteAllText(WorldDataPath + "gen_" + SettingWorldName + ".json", jsonSaveGenData);
        Debug.Log("World_GenData Json Saved Succesfully");

        string jsonSaveRunData = JsonUtility.ToJson(SaveWorldRuntimeData, true);
        File.WriteAllText(WorldDataPath + "run_" + SettingWorldName + ".json", jsonSaveRunData);
        Debug.Log("World_RuntimeData Json Saved Succesfully");


        var bytes_Landmass = map_Landmass.EncodeToPNG();
        var bytes_Biomes = map_Biomes.EncodeToPNG();
        var bytes_Resources = map_Resources.EncodeToPNG();

        File.WriteAllBytes(WorldMapPath + "map_Landmass.png", bytes_Landmass);
        File.WriteAllBytes(WorldMapPath + "map_Biomes.png", bytes_Biomes);
        File.WriteAllBytes(WorldMapPath + "map_Resources.png", bytes_Resources);
        Debug.Log("World_Map png Saved Succesfully");
    }

    public void IntLoadWorldData() {
        if (File.Exists(WorldDataPath + "gen_" + SettingWorldName + ".json")) {
            string jsonLoad = File.ReadAllText(WorldDataPath + "gen_" + SettingWorldName + ".json");
            SaveWorldGenData = JsonUtility.FromJson<JAWGSaveWorldGenData>(jsonLoad);
            SettingWorldSize = SaveWorldGenData.s_WorldSize;
            SettingWorldOffset = SaveWorldGenData.s_WorldOffset;
            SettingChunkSize = SaveWorldGenData.s_ChunkSize;
            WorldSeed = SaveWorldGenData.s_WorldSeed;
            Debug.Log("World_GenData Json Loaded Succesfully");
        } else {
            Debug.LogError("Missing WorldGenData File at: " + WorldDataPath + "gen_" + SettingWorldName + ".json" );
            Debug.Break();
        }

        if (File.Exists(WorldDataPath + "run_" + SettingWorldName + ".json")) {
            string jsonLoad = File.ReadAllText(WorldDataPath + "run_" + SettingWorldName + ".json");
            SaveWorldRuntimeData = JsonUtility.FromJson<JAWGSaveWorldRuntimeData>(jsonLoad);
            CurrentWorldTimeMinutes = SaveWorldRuntimeData.s_CurrentMinutes;
            CurrentWorldTimeHours = SaveWorldRuntimeData.s_CurrentHours;
            CurrentWorldTimeDays  = SaveWorldRuntimeData.s_CurrentDays;
            SettingCycleDayNight = SaveWorldRuntimeData.s_GameRuleDoDayCycle;
            Player.transform.position = new Vector3(SaveWorldRuntimeData.s_PlayerCoordX, SaveWorldRuntimeData.s_PlayerCoordY, 0);
            Debug.Log("World_RunData Json Loaded Succesfully");
        } else {
            Debug.LogError("Missing WorldRuntimeData File at: " + WorldDataPath + "run_" + SettingWorldName + ".json");
            Debug.Break();
        }

        if (File.Exists(WorldMapPath + "map_Landmass.png") && File.Exists(WorldMapPath + "map_Biomes.png") && File.Exists(WorldMapPath + "map_Resources.png")) {
            byte[] bytes_Landmass;
            byte[] bytes_Biomes;
            byte[] bytes_Resources;

            bytes_Landmass = File.ReadAllBytes(WorldMapPath + "map_Landmass.png");
            bytes_Biomes = File.ReadAllBytes(WorldMapPath + "map_Biomes.png");
            bytes_Resources = File.ReadAllBytes(WorldMapPath + "map_Resources.png");

            map_Landmass = new Texture2D(2, 2);
            map_Biomes = new Texture2D(2, 2);
            map_Resources = new Texture2D(2, 2);

            map_Landmass.LoadImage(bytes_Landmass);
            map_Biomes.LoadImage(bytes_Biomes);
            map_Resources.LoadImage(bytes_Resources);

            map_Landmass.filterMode = FilterMode.Point;
            map_Biomes.filterMode = FilterMode.Point;
            map_Resources.filterMode = FilterMode.Point;

            Debug.Log("World_Map png Loaded Succesfully");
        } else {
            Debug.LogError("Missing WorldGenMap Files at: " + WorldMapPath + "map_Landmass.png" + "\n" + WorldMapPath + "map_Biomes.png" + "\n" + WorldMapPath + "map_Resources.png");
            Debug.Break();
        }


    }

    public void LoadModdables() {
        GameModsLocation = Application.persistentDataPath + "/mods/";
        Array.Clear(ModsLoc, 0, ModsLoc.Length);
        Array.Clear(ModsNames, 0, ModsNames.Length);
        ModsLoc = Directory.GetDirectories(GameModsLocation);
        ModsNames = Directory.GetDirectories(GameModsLocation);

        for (int i = 0; i < ModsLoc.Length; i++) {
            ModsNames[i] = ModsLoc[i].Replace(GameModsLocation, "");
        }
    }

    //Post-init and and Runtime functions
    public void LocatePlayer(int CurrentChunkX, int CurrentChunkY) {

        PlayerWorldPosX = ((int)Player.transform.position.x);
        PlayerWorldPosY = (int)Player.transform.position.y;

        //loacting player
        if (CurrentChunkX >= 0) {
            PlayerChunkX = (int)Mathf.Ceil(CurrentChunkX / SettingChunkSize);
        } else {
            PlayerChunkX = (int)Mathf.Floor(CurrentChunkX / SettingChunkSize) - 1;
        }

        if (CurrentChunkY >= 0) {
            PlayerChunkY = (int)Mathf.Ceil(CurrentChunkY / SettingChunkSize);
        } else {
            PlayerChunkY = (int)Mathf.Floor(CurrentChunkY / SettingChunkSize)-1;
        }

        //Locate player's current biome
        for (int b = 0; b < BiomesList.Length; b++) {
            if (map_Landmass.GetPixel(PlayerWorldPosX + WorldOffsetX, PlayerWorldPosY + WorldOffsetY) == BiomesList[b].BiomeColor32) {
                CurrentBiomeName = BiomesList[b].BiomeName;
                CurrentBiomeTemp = BiomesList[b].Temperature;
            } else if (map_Landmass.GetPixel(PlayerWorldPosX + WorldOffsetX, PlayerWorldPosY + WorldOffsetY) == Transparent) {
                CurrentBiomeName = "Ocean";
                CurrentBiomeTemp = 10;
            }
        }

        //This below basically does the same thing the one above, but it dosent update idk why
        //Im gonna leave it here because i can learn from it.
        /* foreach (WorldBiomes Col in BiomesList) {
            if (map_Biomes.GetPixel(PlayerWorldPosX + WorldOffsetX, PlayerChunkY + WorldOffsetY) == Col.BiomeColor32) {
                CurrentBiome = Col.BiomeColor32;
            }
        }
         */
    }

    public void LoadChunk(int chunkX, int chunkY) {
        for (int iX = 0; iX < SettingChunkSize; iX++) {
            for (int iY = 0; iY < SettingChunkSize; iY++) {

                int CoordX = (chunkX * SettingChunkSize) + iX;
                int CoordY = (chunkY * SettingChunkSize) + iY;

                if (map_Landmass.GetPixel(CoordX + WorldOffsetX, CoordY + WorldOffsetY) == Color.black) {
                    for (int b = 0; b < BiomesList.Length; b++) {
                        if (map_Biomes.GetPixel(CoordX + WorldOffsetX, CoordY + WorldOffsetY) == BiomesList[b].BiomeColor32) {
                            //Load Surface
                            GridLandmass.SetTile(new Vector3Int(CoordX, CoordY, 0), BiomesList[b].SurfaceRuleTiles[0]);
                            GridLandmass_.SetTile(new Vector3Int(CoordX, CoordY, 0), WaterTile);
                            GridLandmass__.SetTile(new Vector3Int(CoordX, CoordY, 0), Seafloor);
                            //Load resources
                            if (map_Resources.GetPixel(CoordX + WorldOffsetX, CoordY + WorldOffsetY) == Color.red && BiomesList[b].SurfaceObjects.Length != 0) {
                                GameObject tmpObj = Instantiate(GetResource(b));
                                tmpObj.name = "" + BiomesList[b].BiomeName + "_" + BiomesList[b].SurfaceObjects[0].ResourceObj.name + "_X" + CoordX + "_Y" + CoordY;
                                tmpObj.transform.position = new Vector3(CoordX - 0.5f, CoordY - 0.5f, 99);
                                tmpObj.transform.SetParent(ResourcesLayer.transform);
                                map_Resources.SetPixel(CoordX + WorldOffsetX, CoordY + WorldOffsetY, Color.green);
                                
                            }
                        }
                    }

                } else {
                    GridLandmass_collider.SetTile(new Vector3Int(CoordX, CoordY, 0), ColliderTile);
                    GridLandmass_.SetTile(new Vector3Int(CoordX, CoordY, 0), WaterTile);
                    GridLandmass__.SetTile(new Vector3Int(CoordX, CoordY, 0), Seafloor);
                }
            }
        }
        //Mark the chunk as loaded
        WorldChunks[((SettingWorldSize / SettingChunkSize) / 2) + PlayerChunkX, ((SettingWorldSize / SettingChunkSize) / 2) + PlayerChunkY] = 1;
        
    }

    GameObject GetResource(int CurrentBiome) {
        int obj = UnityEngine.Random.Range(0, BiomesList[CurrentBiome].SurfaceObjects.Length);
        return BiomesList[CurrentBiome].SurfaceObjects[obj].ResourceObj;
        
    }

    public void UnloadChunk(int chunkX, int chunkY) {
        //Unloading somewhen
        //Basically setting every tile to null at [chunkX,chunkY]; with the same method used in LoadChunk()
    }

    public void CycleDayNight() {
        CurrentWorldTimeMinutes += Time.deltaTime;
        
        //Real life Seconds are ingame minutes;
        if (CurrentWorldTimeMinutes >= 60) {
            CurrentWorldTimeMinutes = 0;
            CurrentWorldTimeHours += 1;
        }
        //Minutes are ingame hours;
        if (CurrentWorldTimeHours >= 24) {
            CurrentWorldTimeHours = 0;
            CurrentWorldTimeDays += 1;
        }

        if (CurrentWorldTimeHours <= 12) {
            CurrentWorldTimeMinutesCounter += Time.deltaTime;
        } else {
            CurrentWorldTimeMinutesCounter -= Time.deltaTime;
        }

        worldTimeDisp.text = "Time: " + Mathf.Round(CurrentWorldTimeMinutes) + "m " + CurrentWorldTimeHours + "h " + CurrentWorldTimeDays + "d";

        ScaledCurrentWorldTimeMinutesCounter = scale(0F, 781F, 0F, 1F, CurrentWorldTimeMinutesCounter);
        WorldGlobalLight2D.intensity = ScaledCurrentWorldTimeMinutesCounter;

        if (CurrentWorldTimeHours > 0 && CurrentWorldTimeHours <= 11) {
            CurrentDaytime = ("Morning");
        } else if (CurrentWorldTimeHours > 11 && CurrentWorldTimeHours <= 13) {
            CurrentDaytime = ("Noon");
        } else if (CurrentWorldTimeHours > 13 && CurrentWorldTimeHours <= 18) {
            CurrentDaytime = ("Afternoon");
        } else if (CurrentWorldTimeHours > 18 && CurrentWorldTimeHours <= 24) {
            CurrentDaytime = ("Evening");
        }

    }

    public void RuntimeSaveWorld() {
        SaveWorldRuntimeData.s_CurrentMinutes = CurrentWorldTimeMinutes;
        SaveWorldRuntimeData.s_CurrentHours = CurrentWorldTimeHours;
        SaveWorldRuntimeData.s_CurrentDays = CurrentWorldTimeDays;
        SaveWorldRuntimeData.s_GameRuleDoDayCycle = SettingCycleDayNight;
        SaveWorldRuntimeData.s_PlayerCoordX = PlayerWorldPosX;
        SaveWorldRuntimeData.s_PlayerCoordY = PlayerWorldPosY;

        string jsonSaveRunData = JsonUtility.ToJson(SaveWorldRuntimeData, true);
        File.WriteAllText(WorldDataPath + "run_" + SettingWorldName + ".json", jsonSaveRunData);
        Debug.Log("World_RuntimeData Json Saved Succesfully");


        /*var bytes_Landmass = map_Landmass.EncodeToPNG();
        var bytes_Biomes = map_Biomes.EncodeToPNG();
        var bytes_Resources = map_Resources.EncodeToPNG();

        File.WriteAllBytes(WorldMapPath + "map_Landmass.png", bytes_Landmass);
        File.WriteAllBytes(WorldMapPath + "map_Biomes.png", bytes_Biomes);
        File.WriteAllBytes(WorldMapPath + "map_Resources.png", bytes_Resources);
        Debug.Log("World_Map png Saved Succesfully");*/
    }

    //Utility functions
    public float scale(float OldMin, float OldMax, float NewMin, float NewMax, float OldValue) {

        float OldRange = (OldMax - OldMin);
        float NewRange = (NewMax - NewMin);
        float NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;

        return (NewValue);
    }
}














//HG8946