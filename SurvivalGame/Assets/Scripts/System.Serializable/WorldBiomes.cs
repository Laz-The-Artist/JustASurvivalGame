using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class WorldBiomes {

    //Biome Settings and gen info for WorldGeneratorV3

    [Header("General Settings")]
        public string BiomeName = "Name.BiomeNameGeneric"; //Is a placeholder name, CHANGE IT in the inspector....
        public int BiomeID;
        [Range(-99, 99)] public int Temperature = 1;
        public Color32 BiomeColor32;
    [Header("Generator Info")]
        public Tile[] SurfaceTiles;
        public RuleTile[] SurfaceRuleTiles;
        public GameObject[] SurfaceObjects; //trees, ores, plants and such

}
