using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGenerator : MonoBehaviour {
    [Header("Main Generator Settings")]
    [Space]
    [Header("World Generator V2")]
    public Renderer map_display;
    public Tilemap ResourcesGrid;
    public Tilemap LandmassGrid;
    public Tilemap WaterGrid;
    public Tilemap UnderwaterGrid;
    public int WorldSizeX = 100;
    public int WorldSizeY = 100;
    public int XOffset = 0;
    public int YOffset = 0;
    public bool GenerateByCellularAutomata = true;
    public bool GenerateByMapImage = false;
    [Header("Biome Generator Settings")]
    public bool GenSand = true;
    public bool GenBiomes = true;
    public int VoronoiRegionAmount;
    [Header("Cellular Automata Settings")]
    public int SmoothCycles;
    [Range(0, 100000)] public int WorldSeed;
    public bool GenRandomSeed = true;
    [Range(0, 100)] public int FillPercent;
    [Range(0, 8)] public int Treshold;
    [Header("Spawn Region Settings")]
    public int SpawnRegionSizeX;
    public int SpawnRegionSizeY;
    public int RegionOffsetX;
    public int RegionOffsetY;
    [Header("Surface Tiles")]
    public TileToColorMapper[] BiomeColorMappings;
    [Space]
    public RuleTile GrassRuleTile;
    public Tile GrassTile;
    public Tile SandTile;
    public Tile WaterTile;

    private int z = 0;
    private int _Random;
    private int[,] worldPoints;

    private int[] BiomeID;
    private bool IsBiomDataPresent;

    [HideInInspector] public bool IsWorldReady = false;
    [HideInInspector] public Texture2D mapgen;

    private void Awake() {
        IsWorldReady = true;
        Debug.LogWarning("As of 2020.10.23 the WorldGeneratorScriptV2 is out of order, and the usage of this script is not recomended! This message will show up every time unless the script is removed from the scene or the attached gameobject is tunred off.");
        //Generate World by Cellular Automata functions
        /**if (GenerateByCellularAutomata) {
            StartMapping();
            if (GenBiomes) {
                GenVoronoi();
                GenSurfaceCellular();
            } else {
                GenSurfaceCellular();
            }
        }**/

    }
    
    void Start() {
        //Start Surface Population
        /**if (GenerateByCellularAutomata) {
             StartCoroutine(PopulateSurface());
        }**/

    }


    void Update() {
        //im sure there will be things here....
    }



    //Create the map that will display the world. 1 tile in the world = 1 pixel on the image
    public void StartMapping() {
        mapgen = new Texture2D(WorldSizeX, WorldSizeY);
        mapgen.filterMode = FilterMode.Point;
        Sprite maprendersprite = Sprite.Create(mapgen, new Rect(0.0f, 0.0f, WorldSizeX, WorldSizeY), new Vector2(0.5f, 0.5f), 100.0f);
        map_display.GetComponent<SpriteRenderer>().sprite = maprendersprite;
    }

    //Vorornoi noise for biome placement
    Texture2D GenVoronoi() {
        Vector2Int[] centroids = new Vector2Int[VoronoiRegionAmount];
        Color[] regions = new Color[VoronoiRegionAmount];
        for (int i = 0; i < VoronoiRegionAmount; i++) {
            centroids[i] = new Vector2Int(Random.Range(0, WorldSizeX), Random.Range(0, WorldSizeY));
            int Biomechosen = Random.Range(0, BiomeColorMappings.Length); 
            regions[i] = BiomeColorMappings[Biomechosen].color;
        }
        Color[] pixelColors = new Color[WorldSizeX * WorldSizeY];
        for (int x = 0; x < WorldSizeX; x++) {
            for (int y = 0; y < WorldSizeY; y++) {
                int index = x * WorldSizeX + y;
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
        mapgen.SetPixels(pixelColors);
        mapgen.Apply();
        return mapgen;
    }

    //Cellular Automata world generator
    public void GenSurfaceCellular() {
        worldPoints = new int[WorldSizeX, WorldSizeY];
        //Seed generation
        if (GenRandomSeed == true) {
            int RandomSeed = Random.Range(0, 100000);
            WorldSeed = RandomSeed;
            System.Random randChoice = new System.Random(RandomSeed.GetHashCode());
            for (int x = 0; x < WorldSizeX; x++) {
                for (int y = 0; y < WorldSizeY; y++) {
                    if (randChoice.Next(0, 100) < FillPercent) {
                        worldPoints[x, y] = 1;
                    } else {
                        worldPoints[x, y] = 0;
                    }

                }
            }
        } else {
            System.Random randChoice = new System.Random(WorldSeed.GetHashCode());

            for (int x = 0; x < WorldSizeX; x++) {
                for (int y = 0; y < WorldSizeY; y++) {
                    if (randChoice.Next(0, 100) < FillPercent) {
                        worldPoints[x, y] = 1;
                    } else {
                        worldPoints[x, y] = 0;
                    }
                }
            }
        }

        //Cellular Automata function - Smoothing cycles
        for (int i = 0; i < SmoothCycles; i++) {
            for (int x = 0; x < WorldSizeX; x++) {
                for (int y = 0; y < WorldSizeY; y++) {

                    int neighboringWalls = GettingNeighbors(x, y);

                    if (neighboringWalls > Treshold) {
                        worldPoints[x, y] = 1;
                    } else if (neighboringWalls < Treshold) {
                        worldPoints[x, y] = 0;
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
                        if (worldPoints[x, y] == 1) {
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

    //Surface building
    public IEnumerator PopulateSurface(){

        //fill the map with water
        for (int x = 0; x < WorldSizeX+20; x++) {
            for (int y = 0; y < WorldSizeY+20; y++) {
                WaterGrid.SetTile(new Vector3Int(x - XOffset-10, y - YOffset - 10, z), WaterTile);
            }
        }

        //place landmasses
        for (int x = 0; x < WorldSizeX; x++) {
            for (int y = 0; y < WorldSizeY; y++) {
                if (worldPoints[x, y] == 1) {

                    if (GenBiomes == false) {
                        LandmassGrid.SetTile(new Vector3Int(x - XOffset, y - YOffset, z), BiomeColorMappings[1].Tile);
                        mapgen.SetPixel(x, y, BiomeColorMappings[1].color);
                    }

                    Color MapPixel = mapgen.GetPixel(x, y);
                    // bcN stands for Biome Color Number
                    for (int bcN = 0; bcN < BiomeColorMappings.Length; bcN++) {
                        if (MapPixel.Equals(BiomeColorMappings[bcN].color)) {
                            LandmassGrid.SetTile(new Vector3Int(x - XOffset, y - YOffset, z), BiomeColorMappings[bcN].Tile);
                            mapgen.SetPixel(x, y, BiomeColorMappings[bcN].color);
                        }
                    }

                } else {
                    if (GenSand) {
                        for (int xB = -1; xB < 2; xB++) {
                            for (int yB = -1; yB < 2; yB++) {
                                if (x + xB >= 0 && y + yB >= 0 && x + xB <= WorldSizeX - 1 && y + yB <= WorldSizeY - 1) {
                                    if (worldPoints[x + xB, y + yB] == 1) {
                                        Color sandYellow = new Color(0.9215686f, 0.8235294f, 0.5647059f, 1f);
                                        mapgen.SetPixel(x + xB, y + yB, sandYellow);
                                        LandmassGrid.SetTile(new Vector3Int(x + xB - XOffset, y + yB - YOffset, z), SandTile);
                                    }
                                }

                            }
                        }
                    }
                    //LandmassGrid.SetTile(new Vector3Int(x - XOffset, y - YOffset, z), WaterTile);
                    Color waterBlue = new Color(0.15f, 0.37f, 0.72f, 1f);
                    mapgen.SetPixel(x, y, waterBlue);
                }


            }
            mapgen.Apply();
            yield return new WaitForEndOfFrame();
        }


        //fill spawn region
        for (int spX = 0; spX < SpawnRegionSizeX; spX++) {
            for (int spY = 0; spY < SpawnRegionSizeY; spY++) {
                yield return new WaitForEndOfFrame();
                mapgen.SetPixel(spX+XOffset-RegionOffsetX, spY+YOffset - RegionOffsetY, Color.red);
                LandmassGrid.SetTile(new Vector3Int(spX - RegionOffsetX, spY - RegionOffsetY, z), GrassTile);

            }
        }

        mapgen.Apply();

        //Marking the world as complete
        IsWorldReady = true;
    }

    


    

}

