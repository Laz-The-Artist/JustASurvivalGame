using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class WorldBiomes {

    //Biome Settings and gen info for WorldGeneratorV3

    [Header("General Settings")]
        public string BiomeName = "Name.BiomeNameGeneric";
        public int BiomeID;
        [Range(-99, 99)] public int Temperature = 1;
        public Color32 BiomeColor32;
    [Header("Generator Info")]
        public RuleTile[] SurfaceRuleTiles;
        public SurfaceObjects[] SurfaceObjects;//trees, ores, plants and such
        

}

[System.Serializable]
public class SurfaceObjects {
    public GameObject ResourceObj;
    [Range(1, 100)] public int ResourceChance;
}

[System.Serializable]
public class WorldBiomesModdable {

    //Biome Settings and gen info for WorldGeneratorV3

    [Header("General Settings")]
    public string BiomeName = "Name.BiomeNameGeneric";
    public int BiomeID;
    [Range(-99, 99)] public int Temperature = 1;
    public Color32 BiomeColor32;
    [Header("Generator Info")]
    public RuleTile[] SurfaceRuleTiles;
    public SurfaceObjects[] SurfaceObjects;//trees, ores, plants and such

}

[System.Serializable]
public class TileConstructor {
    public TileConstructArrayElements[] TileContruct;
}
[System.Serializable]
public class TileConstructArrayElements {

    public string ModTileName;
    public string ModTileTextureLocation;
}