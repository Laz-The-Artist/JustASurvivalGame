using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGeneratorV3 : MonoBehaviour {

    [Header("Main Generator Settings")]
    [Space]
    [Header("World Generator V3")]
        public int SettingWorldSize = 512; //always use a number that is a power of 2; otherwise things WILL go wrong
        public int SettingWorldOffset = 0;
        public int SettingChunkSize = 16; //always use a number that is a power of 2; otherwise things WILL go wrong
        public GameObject Player;
        public Tilemap GridLandmass;
        public Tile GrassForTest;
        public Renderer map_display;

    [Header("Cellular Automata Settings")]
        public bool GenCellularMap = true;
        public int CellularSmoothCycles;
        public bool GenRandomSeed = true;
        [Range(0, 100000)] public int WorldSeed;
        [Range(0, 100)] public int CellularFillPercent;
        [Range(0, 8)] public int CellularTreshold;

    [Header("Biome Generator Settings")]
        public bool SettingGenSand = true;
        public bool SettingGenBiomes = true;

    [HideInInspector] public Texture2D map_Landmass;
    [HideInInspector] public Texture2D map_Biomes;
    [HideInInspector] public Texture2D map_Combined;

    int WorldSizeX;
    int WorldSizeY;
    int WorldOffsetX;
    int WorldOffsetY;
    int NumberOfWorldChunks;
    private int[,] WorldChunks;

    public int PlayerWorldPosX;
    public int PlayerWorldPosY;
    [Space]
    public int PlayerChunkX;
    public int PlayerChunkY;
    public float Joska;

    private int[,] CellularWorldPoints;

    //Where it all begins
    private void Awake() {
        InitialiseWorld();
        if (GenCellularMap) {
            GenLandmassCellular();
            MapChunksToWorld();
        }

    }

    void Start() {
        //MapChunksToWorld();

    }

    void FixedUpdate() {
        PlayerWorldPosX = (int)Player.transform.position.x;
        PlayerWorldPosY = (int)Player.transform.position.y;

        LocatePlayer(PlayerWorldPosX, PlayerWorldPosY);
        if (WorldChunks[((SettingWorldSize / SettingChunkSize)/2)+PlayerChunkX, ((SettingWorldSize / SettingChunkSize) / 2) + PlayerChunkY] == 0){
            Debug.Log("Loading chunk at " + PlayerChunkX + "," + PlayerChunkY);
            LoadChunk(PlayerChunkX, PlayerChunkY);
        }
        //if (GridLandmass.GetTile(new Vector3Int(PlayerWorldPosX,PlayerWorldPosY,0))==null) {}
    }



    public void InitialiseWorld() {
        //set initial values that are required
        WorldSizeX = SettingWorldSize;
        WorldSizeY = SettingWorldSize;

        WorldOffsetX = SettingWorldOffset;
        WorldOffsetY = SettingWorldOffset;
        NumberOfWorldChunks = SettingWorldSize*SettingWorldSize / SettingChunkSize;
        Debug.Log(NumberOfWorldChunks + " Chunks will be generated");
        WorldChunks = new int[SettingWorldSize / SettingChunkSize, SettingWorldSize / SettingChunkSize];

        //start mapping; making the textures for maps
        map_Landmass = new Texture2D(WorldSizeX, WorldSizeY);
        map_Landmass.filterMode = FilterMode.Point;
        Sprite maprendersprite = Sprite.Create(map_Landmass, new Rect(0.0f, 0.0f, WorldSizeX, WorldSizeY), new Vector2(0.5f, 0.5f), 100.0f);
        map_display.GetComponent<SpriteRenderer>().sprite = maprendersprite;

    }

    //CellularAutomata based landmass generator
    public void GenLandmassCellular() {
        CellularWorldPoints = new int[WorldSizeX, WorldSizeY];
        //Seed generation
        if (GenRandomSeed == true) {
            int RandomSeed = Random.Range(0, 100000);
            WorldSeed = RandomSeed;
            System.Random randChoice = new System.Random(RandomSeed.GetHashCode());
            for (int x = 0; x < WorldSizeX; x++) {
                for (int y = 0; y < WorldSizeY; y++) {
                    if (randChoice.Next(0, 100) < CellularFillPercent) {
                        CellularWorldPoints[x, y] = 1;
                    } else {
                        CellularWorldPoints[x, y] = 0;
                    }

                }
            }
        } else {
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
                    GridLandmass.SetTile(new Vector3Int( (chunkX * SettingChunkSize)+ iX, (chunkY * SettingChunkSize)+ iY, 0), GrassForTest);
                }
            }
        }

        Debug.Log("Chunk " + PlayerChunkX + "," + PlayerChunkY + " generated succesfully!");
        WorldChunks[((SettingWorldSize / SettingChunkSize) / 2) + PlayerChunkX, ((SettingWorldSize / SettingChunkSize) / 2) + PlayerChunkY] = 1;
    }
}














//HG8946