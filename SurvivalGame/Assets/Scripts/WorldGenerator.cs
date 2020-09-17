using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGenerator : MonoBehaviour
{
    [Header("Generator Settings")]
    public Tilemap WorldGrid;
    public int WorldSizeX = 100;
    public int WorldSizeY = 100;
    public int XOffset = 0;
    public int YOffset = 0;
    public int SmoothCycles;
    [Range(0, 100000)] public int WorldSeed;
    public bool GenRandomSeed = true;
    [Range(0,100)] public int FillPercent;
    [Range(0, 8)] public int Treshold;
    [Header("Spawn Region Settings")]
    public int SpawnRegionSizeX;
    public int SpawnRegionSizeY;
    public int RegionOffsetX;
    public int RegionOffsetY;
    [Header("Surface Tiles")]
    public Tile[] Surface;
    public RuleTile Surface_rule;
    public Tile Water;
    [Header("Resource Tiles")]
    public Tile[] Resources;

    private int z = 0;
    private int _Random;
    private int[,] worldPoints;

    [HideInInspector] public bool IsWorldReady = false;

    private void Awake() {
        //Generate World by Cellular Automata functions
        GenSurfaceCellular();
    }

    void Start()
    {
        //Start Surface Population
        StartCoroutine(PlaceSurfaceCellular());

        
    }


    void Update()
    {
        //im sure there will be things here....
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
    public IEnumerator PlaceSurfaceCellular (){
        //fill the map with water
        for (int x = 0; x < WorldSizeX+20; x++) {
            for (int y = 0; y < WorldSizeY+20; y++) {
                    WorldGrid.SetTile(new Vector3Int(x - XOffset-10, y - YOffset - 10, z), Water);
            }
        }
        //place landmasses
        for (int x = 0; x < WorldSizeX; x++) {
            for (int y = 0; y < WorldSizeY; y++) {
                if (worldPoints[x, y] == 1) {
                    WorldGrid.SetTile(new Vector3Int(x - XOffset, y - YOffset, z), Surface_rule);
                }
            }
            yield return new WaitForEndOfFrame();
        }
        //fill spawn region
        for (int spX = 0; spX < SpawnRegionSizeX; spX++) {
            for (int spY = 0; spY < SpawnRegionSizeY; spY++) {
                yield return new WaitForEndOfFrame();
                WorldGrid.SetTile(new Vector3Int(spX - RegionOffsetX, spY - RegionOffsetY, z), Surface_rule);
            }
        }

        //Marking the world as complete
        IsWorldReady = true;
    }



}
