using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class WorldBiomes {

    //Biome Settings and gen info for WorldGeneratorV3

    [Header("General Settings")]
    public string BiomeName = "Name.BiomeNameGeneric"; //Is a placeholder name, CHANGE IT in the inspector....
    [Range(1, 10)] public int Rarity = 1;
    public Color BiomeColor; //on the map
    public Color32 BiomeColor32;
    [Header("Generator Info")]
    public Tile[] SurfaceTiles;
    public GameObject[] SurfaceObjects; //trees, ores, plants and such

}
