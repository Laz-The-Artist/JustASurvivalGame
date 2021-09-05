using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenuHandler : MonoBehaviour {
    public GameObject LoadingScreen;
    public Image LoadingBarFill;
    public float LoadingProgress;
    public GameObject ScrollViewContentBox;
    public GameObject WorldEntry;
    [Space]
    public TMP_InputField InputWorldName;
    public Toggle InputRandSeedBool;
    public TMP_InputField InputSeed;
    public TMP_InputField InputWorldSize;
    public TMP_Dropdown InputLandmassGenType;
    public TMP_Dropdown InputBiomeGenType;
    public TMP_InputField InputResourceFillPercent;
    public TMP_InputField InputCellSmoothCycles;
    public TMP_InputField InputLandmassPerlinScale;
    public TMP_InputField InputLandmassPerlinAreaDivision;
    public TMP_Dropdown InputBiomeDistortionMethod;
    public TMP_InputField InputBiomePerlinScale;
    public TMP_InputField InputBiomePerlinAreaDivision;
    public TMP_InputField InputBiomeSmoothCycles;
    [Space]
    public string GameResourcesFolder;
    public string GameModsFolder;
    public string GameSavesFolder;
    [HideInInspector] public SaveGenConfig GenConfigData; 
    //Variables for saving them GenConfig File
    [HideInInspector] public string WorldName;
    [HideInInspector] public bool LoadExisting;
    [HideInInspector] public bool GenRandomSeed;
    [HideInInspector] public int WorldSeed;
    [HideInInspector] public int WorldSize;
    [HideInInspector] public int ChunkSize;
    [HideInInspector] public LandmassGenMethod LandmassGenType;
    [HideInInspector] public BiomeGenMethod BiomeGenType;
    [HideInInspector] public int ResourceFillPercent;
    [HideInInspector] public int CellSmoothCycles;
    [HideInInspector] public int CellFillPercent;
    [HideInInspector] public int CellThreshold;
    [HideInInspector] public int LandmassPerlinScale;
    [HideInInspector] public float LandmassPerlinAreaDivision;
    [HideInInspector] public BiomeDistortionMethod BiomeDistortionMethod;
    [HideInInspector] public int BiomePerlinScale;
    [HideInInspector] public float BiomePerlinAreaDivision;
    [HideInInspector] public int BiomeDistortionCellThreshold;
    [HideInInspector] public int BiomeSmoothCycles;

    [HideInInspector] public List<MainMenuWorldItem> WorldItems;
    [HideInInspector] public List<GameObject> WorldItemObjects;
    [HideInInspector] public SaveWorldDataInfo WorldDataInfoSaveObject;

    void Awake() {
        GameResourcesFolder = Application.persistentDataPath;
        GameModsFolder = GameResourcesFolder + "/mods/";
        GameSavesFolder = GameResourcesFolder + "/saves/";
        if (!Directory.Exists(GameSavesFolder)) {
            Directory.CreateDirectory(GameSavesFolder);
        }
        if (!Directory.Exists(GameModsFolder)) {
            Directory.CreateDirectory(GameModsFolder);
        }

        WorldItems = new List<MainMenuWorldItem>();
        WorldItemObjects = new List<GameObject>();
    }

    void Start() {
        GetWorldItems();
        SetupDropdowns();
    }

    void Update() {
        WorldName = InputWorldName.text;
        GenRandomSeed = InputRandSeedBool.isOn;
        int.TryParse(InputSeed.text, out WorldSeed);
        int.TryParse(InputWorldSize.text, out WorldSize);
        if (WorldSize % 16 != 0) {
           WorldSize = closestNumber(WorldSize, 16);
        }
        ChunkSize = 16;
        LandmassGenType = (LandmassGenMethod) InputLandmassGenType.value;
        BiomeGenType = (BiomeGenMethod) InputBiomeGenType.value;
        int.TryParse(InputResourceFillPercent.text, out ResourceFillPercent);
        int.TryParse(InputCellSmoothCycles.text, out CellSmoothCycles);
        CellFillPercent = 55;
        CellThreshold = 4;
        int.TryParse(InputLandmassPerlinScale.text, out LandmassPerlinScale);
        float.TryParse(InputLandmassPerlinAreaDivision.text, out LandmassPerlinAreaDivision);
        BiomeDistortionMethod = (global::BiomeDistortionMethod) InputBiomeDistortionMethod.value;
        int.TryParse(InputBiomePerlinScale.text, out BiomePerlinScale);
        float.TryParse(InputBiomePerlinAreaDivision.text, out BiomePerlinAreaDivision);
        BiomeDistortionCellThreshold = 4;
        int.TryParse(InputBiomeSmoothCycles.text, out BiomeSmoothCycles);
    }

    public void SetupDropdowns() {
        InputBiomeDistortionMethod.options.Add(new TMP_Dropdown.OptionData(BiomeDistortionMethod.None.ToString()));
        InputBiomeDistortionMethod.options.Add(new TMP_Dropdown.OptionData(BiomeDistortionMethod.Perlin.ToString()));
        InputBiomeDistortionMethod.options.Add(new TMP_Dropdown.OptionData(BiomeDistortionMethod.JASGCustom.ToString()));
        InputBiomeGenType.options.Add(new TMP_Dropdown.OptionData(BiomeGenMethod.VoronoiNoise.ToString()));
        InputBiomeGenType.options.Add(new TMP_Dropdown.OptionData(BiomeGenMethod.JASGCustom.ToString()));
        InputLandmassGenType.options.Add(new TMP_Dropdown.OptionData(LandmassGenMethod.CellularAutomata.ToString()));
        InputLandmassGenType.options.Add(new TMP_Dropdown.OptionData(LandmassGenMethod.PerlinNoise.ToString()));
        InputLandmassGenType.options.Add(new TMP_Dropdown.OptionData(LandmassGenMethod.HexaGon.ToString()));
        InputLandmassGenType.options.Add(new TMP_Dropdown.OptionData(LandmassGenMethod.JASGCustom.ToString()));
    }

    public void MakeGenConfigFile() {
        GenConfigData.WorldName = WorldName;
        GenConfigData.LoadExisting = LoadExisting;
        GenConfigData.GenRandomSeed = GenRandomSeed;
        GenConfigData.WorldSeed = WorldSeed;
        GenConfigData.WorldSize = WorldSize;
        GenConfigData.ChunkSize = ChunkSize;
        GenConfigData.LandmassGenType = LandmassGenType;
        GenConfigData.BiomeGenType = BiomeGenType;
        GenConfigData.ResourceFillPercent = ResourceFillPercent;
        GenConfigData.CellSmoothCycles = CellSmoothCycles;
        GenConfigData.CellFillPercent = CellFillPercent;
        GenConfigData.CellThreshold = CellThreshold;
        GenConfigData.LandmassPerlinScale = LandmassPerlinScale;
        GenConfigData.LandmassPerlinAreaDivision = LandmassPerlinAreaDivision;
        GenConfigData.BiomeDistortionMethod = BiomeDistortionMethod;
        GenConfigData.BiomePerlinScale = BiomePerlinScale;
        GenConfigData.BiomePerlinAreaDivision = BiomePerlinAreaDivision;
        
        GenConfigData.BiomeDistortionCellThreshold = BiomeDistortionCellThreshold;
        GenConfigData.BiomeSmoothCycles = BiomeSmoothCycles;
        
        string SaveGenConfigDataJson = JsonUtility.ToJson(GenConfigData, true);
        File.WriteAllText(GameSavesFolder + "gen_config.json",SaveGenConfigDataJson);
    }

    public void GetWorldItems() {
        string[] WorldLocations = Directory.GetDirectories(GameSavesFolder);
        string[] WorldNames = Directory.GetDirectories(GameSavesFolder);
        for (int i = 0; i < WorldLocations.Length; i++) {
            WorldNames[i] = WorldLocations[i].Replace(GameSavesFolder, "");
        }

        for (int i = 0; i < WorldNames.Length; i++) {
            if (File.Exists(WorldLocations + "/info" + WorldNames[i] + ".json")) {
                string WorldInfoJson = File.ReadAllText(WorldLocations + "/info" + WorldNames[i] + ".json");
                WorldDataInfoSaveObject = JsonUtility.FromJson<SaveWorldDataInfo>(WorldInfoJson);
                WorldItems.Add(new MainMenuWorldItem(WorldNames[i], WorldDataInfoSaveObject.WorldVersion, WorldDataInfoSaveObject.CreationDate, WorldDataInfoSaveObject.LastOpenedDate));
            }
        }

        foreach (var item in WorldItems) {
            GameObject WorldItem = Instantiate(WorldEntry, ScrollViewContentBox.transform);
            WorldItemObjects.Add(WorldItem);
            WorldItem.name = item.WorldName;
            TMP_Text WorldItemDispName = WorldItem.transform.Find("WorldName").GetComponent<TMP_Text>();
            TMP_Text WorldItemDispWorldVersion = WorldItem.transform.Find("WorldVersion").GetComponent<TMP_Text>();
            TMP_Text WorldItemDispCreationDate = WorldItem.transform.Find("DateOfCreation").GetComponent<TMP_Text>();
            TMP_Text WorldItemDispLastOpenedDate = WorldItem.transform.Find("LastOpenedDate").GetComponent<TMP_Text>();
            WorldItemDispName.text = item.WorldName;
            WorldItemDispWorldVersion.text = item.WorldVersion;
            WorldItemDispCreationDate.text = item.CreationDate;
            WorldItemDispLastOpenedDate.text = item.LastOpenedDate;
        }
        
    }

    public void UpdateWorldItems() {
        WorldItems.Clear();
        foreach (var Object in WorldItemObjects) {
            if (Object != null) {
                Destroy(Object);
            }
        }
        GetWorldItems();
    }

    public void GenWorld(string LoadSceneName) {
        MakeGenConfigFile();
        StartCoroutine(LoadSceneAsync(LoadSceneName));
    }

    public void LoadWorld(string LoadSceneName) {
        StartCoroutine(LoadSceneAsync(LoadSceneName));
    }

    public IEnumerator LoadSceneAsync(string SceneToLoad) {
        LoadingScreen.SetActive(true);
        AsyncOperation SceneLoadOp = SceneManager.LoadSceneAsync(SceneToLoad);

        while (!SceneLoadOp.isDone) {
            LoadingProgress = SceneLoadOp.progress;
            LoadingBarFill.fillAmount = LoadingProgress;
            Debug.Log("Loading Scene " + SceneToLoad + "; progress: " + LoadingProgress);
            yield return null;
        }
        
    }

    int closestNumber(int input, int dividableBy) {
        int q = input / dividableBy;
      
        int n1 = dividableBy * q;
      
        int n2 = (input * dividableBy) > 0 ? (dividableBy * (q + 1)) : (dividableBy * (q - 1));

        if (Mathf.Abs(input - n1) < Mathf.Abs(input - n2)) {
            return n1;
        }
        
        return n2;    
    }
}

public class MainMenuWorldItem {
    public string WorldName;
    public string WorldVersion;
    public string CreationDate;
    public string LastOpenedDate;

    public MainMenuWorldItem(string newWorldName, string newWorldVersion, string newCreationDate, string newLastOpenedDate) {
        WorldName = newWorldName;
        WorldVersion = newWorldVersion;
        CreationDate = newCreationDate;
        LastOpenedDate = newLastOpenedDate;
    }

}
