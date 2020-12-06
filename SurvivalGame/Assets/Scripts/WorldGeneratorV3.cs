using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using TMPro;

public class WorldGeneratorV3 : MonoBehaviour {

    //JAWG - Just A World Engine

    [Header("Main Generator Settings")]
    [Space]
    [Header("World Generator V3")]
        public GameObject Player;
        public int SettingWorldSize = 512; //always use a number that is a power of 2; otherwise things WILL go wrong
        public int SettingWorldOffset = 0;
        public int SettingChunkSize = 16; //always use a number that is a power of 2; otherwise things WILL go wrong
        [Range(2, 4)] public int SettingChunkLoadingRadius = 1;
        public Tilemap GridLandmass;
        public Renderer map_display;

    [Header("Cellular Automata Settings")]
        public bool GenCellularMap = true;
        public int CellularSmoothCycles;
        public bool GenRandomSeed = true;
        [Range(0, 100000)] public int WorldSeed;
        [Range(0, 100)] public int CellularFillPercent;
        [Range(0, 8)] public int CellularTreshold;

    [Header("Biome Generator Settings")]
        public bool SettingGenSandEdges = true;
        public Tile SandTile;
        public bool SettingGenBiomes = true;
        public float SettingVoronoiSmallestDst;
        public float SettingPerlinScale;
        [Range(0, 1)] public float SettingPerlinMinDivisionValue = 0.55f;
        public WorldBiomes[] BiomesList;

    [Header("Day-Night Cycle References")]
        public bool SettingCycleDayNight = true;
        public GameObject WorldGlobalLight;
        UnityEngine.Experimental.Rendering.Universal.Light2D WorldGlobalLight2D;

    [Header("Day-Night Cycle Settings")]
        public float SettingDayNightCycleLength = 12000;
        public float WorldTime;
        public int phase;
        public string CurrentDaytime;
        

    [HideInInspector] public Texture2D map_Landmass;
    [HideInInspector] public Texture2D map_Biomes;
    [HideInInspector] public Texture2D map_Minimap;

    int WorldSizeX;
    int WorldSizeY;
    int WorldOffsetX;
    int WorldOffsetY;
    int NumberOfWorldChunks;
    private int[,] WorldChunks;
    private int[,] CellularWorldPoints;
    //public Texture2D[] PerlinMaps;
    Vector2Int[] centroids;

    int PlayerWorldPosX;
    int PlayerWorldPosY;
    int PlayerChunkX;
    int PlayerChunkY;

    Texture2D VoronoiMap;
    Texture2D PerlinMapNormal;

    [HideInInspector] public bool IsWorldComplete = false;


    //Where it all begins
    private void Awake() {
        InitialiseWorld();

        if (GenCellularMap) {
            GenLandmassCellular();
            MapChunksToWorld();
        }
        if (SettingGenBiomes) {
            GenerateBiomeMap();
        }

        IsWorldComplete = true;
    }

    void Start() {
        WorldGlobalLight2D = WorldGlobalLight.GetComponent<UnityEngine.Experimental.Rendering.Universal.Light2D>();
        
    }

    void FixedUpdate() {

        LocatePlayer(PlayerWorldPosX, PlayerWorldPosY);

        //Chunkloading around the player
        if (WorldChunks[((SettingWorldSize / SettingChunkSize) / 2) + PlayerChunkX, ((SettingWorldSize / SettingChunkSize) / 2) + PlayerChunkY] == 0) {
            //Debug.Log("Loading chunks around " + PlayerChunkX + "," + PlayerChunkY + "; with a distance of " + SettingChunkLoadingRadius);
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

        //Day-Night Cycle
        DayNightCycleWorld();
        WorldGlobalLight2D.intensity = Mathf.Clamp(WorldTime / SettingDayNightCycleLength, 0.13f, 1f);

    }


    public void InitialiseWorld() {
        //set initial values that are required
        WorldSizeX = SettingWorldSize;
        WorldSizeY = SettingWorldSize;

        WorldOffsetX = SettingWorldOffset;
        WorldOffsetY = SettingWorldOffset;
        NumberOfWorldChunks = SettingWorldSize * SettingWorldSize / SettingChunkSize;
        Debug.Log(NumberOfWorldChunks + " Chunks will be generated");
        WorldChunks = new int[SettingWorldSize / SettingChunkSize, SettingWorldSize / SettingChunkSize];

        VoronoiMap = new Texture2D(WorldSizeX, WorldSizeY);
        VoronoiMap.filterMode = FilterMode.Point;
        PerlinMapNormal = new Texture2D(WorldSizeX, WorldSizeY);
        PerlinMapNormal.filterMode = FilterMode.Point;

        //PerlinMaps = new Texture2D[BiomesList.Length];

        centroids = new Vector2Int[BiomesList.Length];

        //Generate the random seed, if its set to generate one
        if (GenRandomSeed) {
            int RandomSeed = Random.Range(0, 100000);
            WorldSeed = RandomSeed;
        }

        //start mapping; making the textures for maps; set the filter mode to point
        map_Landmass = new Texture2D(WorldSizeX, WorldSizeY);
        map_Biomes = new Texture2D(WorldSizeX, WorldSizeY);
        map_Minimap = new Texture2D(WorldSizeX, WorldSizeY);
        map_Landmass.filterMode = FilterMode.Point;
        map_Biomes.filterMode = FilterMode.Point;
        map_Minimap.filterMode = FilterMode.Point;
        //Input map_ in the Sprite Renderer; Displaying in-world.
        Sprite maprendersprite = Sprite.Create(VoronoiMap, new Rect(0.0f, 0.0f, WorldSizeX, WorldSizeY), new Vector2(0.5f, 0.5f), 100.0f);
        map_display.GetComponent<SpriteRenderer>().sprite = maprendersprite;

        //World Settings
        WorldTime = SettingDayNightCycleLength;

        Debug.Log("World Initialised succesfully!");

    }

    //CellularAutomata based landmass generator
    public void GenLandmassCellular() {
        CellularWorldPoints = new int[WorldSizeX, WorldSizeY];
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

        //Defining the temporary VoronoiMap and PerlinMap
        

        //making the voronoi noise
        VoronoiMap.SetPixels32(GenVoronoiV2());
        VoronoiMap.Apply();

        //making the perlin noise
        for (int x = 0; x < WorldSizeX; x++) {
                for (int y = 0; y < WorldSizeY; y++) {
                    Color color = CalcPerlin(x, y);
                    PerlinMapNormal.SetPixel(x, y, color);
                }
            }
        PerlinMapNormal.Apply();

        //Combining the VoronoiMap and PerlinMap
        GenTestBiomeNoise();

        /**for (int x = 0; x < WorldSizeX; x++) {
            for (int y = 0; y < WorldSizeY; y++) {
                Color modifiedPix = PerlinMapNormal.GetPixel(x, y) * VoronoiMap.GetPixel(x, y);
                map_Minimap.SetPixel(x,y,modifiedPix);
            }
        }
        map_Minimap.Apply();**/
        

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
                                if (PerlinMapNormal.GetPixel(x + xB, y + yB) == Color.black && VoronoiMap.GetPixel(x, y) == BiomesList[BiomeNum].BiomeColor32) {
                                    VoronoiMap.SetPixel(x + xB, y + yB, BiomesList[BiomeNum].BiomeColor32);
                                }
                            }

                        }
                    }

                }
            }

            //Count the areas
            /**for (int x = 0; x < WorldSizeX; x++) {
                for (int y = 0; y < WorldSizeY; y++) {
                    if (VoronoiMap.GetPixel(x, y) == BiomesList[BiomeNum].BiomeColor32) {
                        BiomesList[BiomeNum].BiomeArea++;
                    }
                }
            }**/

        }

        VoronoiMap.Apply();
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
                            
                            //that is nice, but i want the world to load around the player; this generates like how it used to but with an extra step and less lag;
                            GridLandmass.SetTile(new Vector3Int( (chunkX * SettingChunkSize - WorldOffsetX)+ iX, (chunkY * SettingChunkSize - WorldOffsetY)+ iY, 0), GrassForTest);
                            //map_Landmass.SetPixel((chunkX * SettingChunkSize) + iX, (chunkY * SettingChunkSize) + iY, Color.black);
                        }
                    }
                }**/

            }
        }
        Debug.Log("Mapped Chunks");
    }

    public void LocatePlayer(int CurrentChunkX, int CurrentChunkY) {

        PlayerWorldPosX = (int)Player.transform.position.x;
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

    }

    public void LoadChunk(int chunkX, int chunkY) {
        for (int iX = 0; iX < SettingChunkSize; iX++) {
            for (int iY = 0; iY < SettingChunkSize; iY++) {
                if (CellularWorldPoints[chunkX * SettingChunkSize + WorldOffsetX + iX, chunkY * SettingChunkSize + WorldOffsetY + iY] == 1) {
                    GridLandmass.SetTile(new Vector3Int((chunkX * SettingChunkSize) + iX, (chunkY * SettingChunkSize) + iY, 0), BiomesList[0].SurfaceTiles[0]);
                }
            }
        }
        WorldChunks[((SettingWorldSize / SettingChunkSize) / 2) + PlayerChunkX, ((SettingWorldSize / SettingChunkSize) / 2) + PlayerChunkY] = 1;
    }

    public void DayNightCycleWorld() {
        if (phase == 0) {
            WorldTime--;
            if (WorldTime <= 0) { phase = 1; }
            //yield return new WaitForEndOfFrame();
        } else if (phase == 1) {
            WorldTime++;
            if (WorldTime >= SettingDayNightCycleLength) { phase = 0; }
            //yield return new WaitForEndOfFrame();
        }

        if (phase == 0 && WorldTime >= SettingDayNightCycleLength / 2) {
            CurrentDaytime = ("Forenoon");
        }else if (phase == 0 && WorldTime <= SettingDayNightCycleLength / 2) {
            CurrentDaytime = ("Afternoon");
        }else if (phase == 1 && WorldTime >= SettingDayNightCycleLength / 2) {
            CurrentDaytime = ("Dawn");
        } else if (phase == 1 && WorldTime <= SettingDayNightCycleLength / 2) {
            CurrentDaytime = ("Night");
        }

    }
}














//HG8946