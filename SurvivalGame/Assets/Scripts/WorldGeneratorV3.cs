using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGeneratorV3 : MonoBehaviour {

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
    public Texture2D[] PerlinMaps;

    int PlayerWorldPosX;
    int PlayerWorldPosY;
    [Space]
    int PlayerChunkX;
    int PlayerChunkY;


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

    }

    void Start() {

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
        PerlinMaps = new Texture2D[BiomesList.Length];

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
        Sprite maprendersprite = Sprite.Create(map_Biomes, new Rect(0.0f, 0.0f, WorldSizeX, WorldSizeY), new Vector2(0.5f, 0.5f), 100.0f);
        map_display.GetComponent<SpriteRenderer>().sprite = maprendersprite;

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
        GenVoronoiV2();
        for (int BiomeLenght = 0; BiomeLenght < BiomesList.Length; BiomeLenght++) {

            int NextPerlin = BiomeLenght * 5;
            Texture2D CurrentPerlin = new Texture2D(WorldSizeX, WorldSizeY);
            CurrentPerlin.filterMode = FilterMode.Point;

            //making the perlin noise
            for (int x = 0; x < WorldSizeX; x++) {
                for (int y = 0; y < WorldSizeY; y++) {
                    Color color = CalcPerlin(x, y, NextPerlin);
                    CurrentPerlin.SetPixel(x, y, color);
                }
            }
            CurrentPerlin.Apply();

            //Storing the perlin noise
            PerlinMaps[BiomeLenght] = CurrentPerlin;
            PerlinMaps[BiomeLenght].SetPixels(CurrentPerlin.GetPixels());

            if (BiomeLenght == 0) {
                //map_Biomes.SetPixels(CurrentPerlin.GetPixels());
                
            }




        }

    }

    Color CalcPerlin(int PerlX, int PerlY, int NextPerlinValue) {
        float xCoord = (((float)PerlX / WorldSizeX) * SettingPerlinScale) + (WorldSeed * NextPerlinValue);
        float yCoord = (((float)PerlY / WorldSizeY) * SettingPerlinScale) + (WorldSeed * NextPerlinValue);

        float sample = Mathf.PerlinNoise(xCoord, yCoord);
        if (sample >= SettingPerlinMinDivisionValue) {
            sample = 1;
        } else if (sample < SettingPerlinMinDivisionValue) {
            sample = 0;
        }
        return new Color(sample, sample, sample);
    }

    //Voronoi Noise

    public Texture2D GenVoronoiV2() {
        Vector2Int[] centroids = new Vector2Int[BiomesList.Length];
        Color32[] regions = new Color32[BiomesList.Length];
        for (int BiomLength = 0; BiomLength < BiomesList.Length; BiomLength++) {
            centroids[BiomLength] = new Vector2Int(Random.Range(0, WorldSizeX), Random.Range(0, WorldSizeY));
            regions[BiomLength] = BiomesList[BiomLength].BiomeColor32;
        }
        Color32[] PixelColors = new Color32[WorldSizeX * WorldSizeY];
        for (int x = 0; x < WorldSizeX; x++) {
            for (int y = 0; y < WorldSizeY; y++) {
                int index = x * WorldSizeY + y;
                PixelColors[index] = regions[GenCentroidIndex(new Vector2Int(x, y), centroids)];
            }
        }

        //Texture2D VoronoiMap = new Texture2D(WorldSizeX, WorldSizeY);
        //VoronoiMap.filterMode = FilterMode.Point;
        map_Biomes.SetPixels32(PixelColors);
        map_Biomes.Apply();
        return map_Biomes;
    }

    int GenCentroidIndex(Vector2Int PixelPos, Vector2Int[] centroids) {
        float smallestDst = WorldSeed;
        int index = 0;
        for (int i = 0; i < centroids.Length; i++) {
            if (Vector2.Distance(PixelPos, centroids[i]) < smallestDst) {
                smallestDst = Vector2.Distance(PixelPos, centroids[1]);
                index = i;
            }
        }
        return index;
    }

    Texture2D GenVoronoi() {
        Vector2Int[] centroids = new Vector2Int[BiomesList.Length];
        Color[] regions = new Color[BiomesList.Length];
        for (int i = 0; i < BiomesList.Length; i++) {
            centroids[i] = new Vector2Int(Random.Range(0, WorldSizeX), Random.Range(0, WorldSizeY));
            regions[i] = BiomesList[i].BiomeColor;
        }
        Color[] pixelColors = new Color[WorldSizeX * WorldSizeY];
        for (int x = 0; x < WorldSizeX; x++) {
            for (int y = 0; y < WorldSizeY; y++) {
                int index = x * WorldSizeY + y;
                pixelColors[index] = regions[GetClosestCentroidIndex(new Vector2Int(x, y), centroids)];
            }
        }
        return GetImageFromColorArray(pixelColors);
    }

    //Voronoi Noise Indexing
    int GetClosestCentroidIndex(Vector2Int pixelPos, Vector2Int[] centroids) {
        float smallestDst = float.MaxValue;
        int index = 0;
        for (int i = 0; i < centroids.Length; i++) {
            if (Vector2.Distance(pixelPos, centroids[i]) < smallestDst) {
                smallestDst = Vector2.Distance(pixelPos, centroids[1]);
                index = i;
            }
        }
        return index;
    }

    //Voronoi noise complement
    Texture2D GetImageFromColorArray(Color[] pixelColors) {
        //Texture2D VoronoiMap = new Texture2D(WorldSizeX, WorldSizeY);
        //VoronoiMap.filterMode = FilterMode.Point;
        map_Biomes.SetPixels(pixelColors);
        map_Biomes.Apply();
        return map_Biomes;
    }

    public void MapChunksToWorld() {
        for (int chunkX = 0; chunkX < WorldSizeX/SettingChunkSize; chunkX++) {
            for (int chunkY = 0; chunkY < WorldSizeY/SettingChunkSize; chunkY++) {

                WorldChunks[chunkX, chunkY] = 0;

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
            //yield return new WaitForEndOfFrame();
        }
        Debug.Log("Mapped Chunks");
        //map_Landmass.Apply();
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

        //Debug.Log("Chunk " + chunkX + "," + chunkY + " generated succesfully!");
        WorldChunks[((SettingWorldSize / SettingChunkSize) / 2) + PlayerChunkX, ((SettingWorldSize / SettingChunkSize) / 2) + PlayerChunkY] = 1;
    }
}














//HG8946