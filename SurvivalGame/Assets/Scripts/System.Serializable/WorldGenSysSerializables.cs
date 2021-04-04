using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BiomeEntry {
    
    [Header("General Settings")]
    public string BiomeName = "Name";
    public int BiomeID;
    [Range(-99, 99)] public int Temperature = 1;
    public Color32 BiomeColor32;
    
    [Header("Generator Info")]
    public RuleTile BiomeSurfaceRuleTile;
    public BiomeResourceEntry[] BiomeResourceEntries;

}

[System.Serializable]
public enum BiomeResourceEntry {
    None,
    OakTree,
    Rock,
    Stick
}

[System.Serializable]
public enum LandmassGenMethod {
    CellularAutomata,
    PerlinNoise,
    HexaGon,
    JASGCustom
}

[System.Serializable]
public enum BiomeGenMethod {
    VoronoiNoise,
    JASGCustom
}

[System.Serializable]
public enum BiomeDistortionMethod {
    None,
    Perlin,
    JASGCustom
}

[System.Serializable]
public class BiomeResourceAssignEntry {
    public BiomeResourceEntry EnumResource;
    public GameObject GameObjResource;
}

public class WorldChunk {
    public int posX;
    public int posY;

    public WorldChunk(int newPosX, int newPosY) {
        posX = newPosX;
        posY = newPosY;
    }
    
}


//SaveObjects
[System.Serializable]
public class SaveWorldDataInfo {
    public string WorldVersion;
    public string CreationDate;
    public string LastOpenedDate;
    public LandmassGenMethod LandmassGenMethod;
    public BiomeGenMethod BiomeGenMethod;
    public BiomeDistortionMethod BiomeDistortionMethod;
    public bool GeneratedByRandomSeed;
    public int NumberOfDifferentBiomes;
}

[System.Serializable]
public class SaveWorldDataGen {
    public int WorldSeed;
    public int WorldSize;
    public int ChunkSize;
    public BiomeResourceEntry[] WorldResourcesSerialised;
    public float[] WorldHeatMap;
}

[System.Serializable]
public class SaveWorldDataRun {
    public string comment1;
    public float PlayerCoordX;
    public float PlayerCoordY;
    public float CurrentDaytimeMinutes;
    public float CurrentDaytimeHours;
    public float CurrentDaytimeDays;
    public string comment2;
    public bool GameRule_DoDaylightCycle;
}


