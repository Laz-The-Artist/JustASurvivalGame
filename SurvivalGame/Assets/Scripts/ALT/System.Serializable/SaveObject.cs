using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class JAWGSaveWorldGenData {

    [Header("JAWG - Generate")]
    public int s_WorldSize;
    public int s_WorldOffset;
    public int s_ChunkSize;
    public int s_WorldSeed;
}

[System.Serializable]
public class JAWGSaveWorldRuntimeData {

    public string c_1;
    public bool s_GameRuleDoDayCycle;
    public string c_2;
    public float s_CurrentMinutes;
    public float s_CurrentHours;
    public float s_CurrentDays;
    public float s_PlayerCoordX;
    public float s_PlayerCoordY;
}

[System.Serializable]
public class SaveBiomes {
    public WorldBiomes[] s_BiomesList;
}

[System.Serializable]
public class GameVersion {
    public string name;
    public string[] version;
    public string iconURL;
    public string previewURL;
    public string descriptionTitle;
    public string description;
    public string downloadURL;
    public string color;
}

[System.Serializable]
public class ModsList {
    public string ModName;
    public string ModDescription;
    public string ModAuthor;
    public string ModVersion;
    public string ModGameVersion;
    public bool loadBiomes;
}

[System.Serializable]
public class ResourceObject {
    public int coordX;
    public int coordY;
    public int BiomeID;
    public int SurfaceObjectID;
}
