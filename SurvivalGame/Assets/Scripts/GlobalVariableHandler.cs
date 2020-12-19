using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GlobalVariableHandler : MonoBehaviour
{
    public TMP_InputField WorldSeedInput;
    public TMP_InputField WorldNameInput;
    public Toggle useRandomSeedInsteadInput;
    public GameObject ScrollViewContentBox;
    public GameObject WorldEntry;
    public GameObject LoadingScreen;
    public Image LoadingBarFill;

    [HideInInspector] public string GenworldName;
    [HideInInspector] public string LoadworldName;
    [HideInInspector] public bool loadExisting;
    [HideInInspector] public bool ReadStartSettingsForGen = false;
    [HideInInspector] public int seed;
    [HideInInspector] public bool genRandomSeed;

    [HideInInspector] public string[] WorldsLoc;
    [HideInInspector] public string[] WorldsNames;
    [HideInInspector] public string WorldSavesLocation;
    [HideInInspector] public GameObject[] WorldEntrys;
    [HideInInspector] public GameObject tmpObj;
    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        CheckWorldFolder();
    }

    void Update()
    {
        int.TryParse(WorldSeedInput.text, out seed);
        GenworldName = WorldNameInput.text;
        genRandomSeed = useRandomSeedInsteadInput.isOn;
    }

    public void CheckWorldFolder() {
        WorldSavesLocation = Application.persistentDataPath + "/";
        Array.Clear(WorldsLoc, 0, WorldsLoc.Length);
        Array.Clear(WorldsNames, 0, WorldsNames.Length);
        Array.Clear(WorldEntrys, 0, WorldEntrys.Length);
        WorldsLoc = Directory.GetDirectories(WorldSavesLocation);
        WorldsNames = Directory.GetDirectories(WorldSavesLocation);
        WorldEntrys = new GameObject[WorldsLoc.Length];

        for (int i = 0; i < WorldsLoc.Length; i++) {
            WorldsNames[i] = WorldsLoc[i].Replace(WorldSavesLocation, "");
            tmpObj = Instantiate(WorldEntry, ScrollViewContentBox.transform);
            tmpObj.name = "world_" + WorldsNames[i];
            TMP_Text tmpWorldName = tmpObj.transform.Find("WorldName").GetComponent<TMP_Text>();
            TMP_Text tmpWorldDate = tmpObj.transform.Find("DateOfCreation").GetComponent<TMP_Text>();
            UIButton tmpButton = tmpObj.transform.Find("WorldIcon").GetComponent<UIButton>();
            tmpButton.WorldName = WorldsNames[i];
            tmpButton.UseProgressBar = true;
            tmpButton.LoadScreen = LoadingScreen;
            tmpButton.LoadingProgressBar = LoadingBarFill;
            tmpWorldName.text = WorldsNames[i];
            tmpWorldDate.text = "" + File.GetCreationTime(WorldsLoc[i]);
            WorldEntrys[i] = tmpObj;
        }
    }

}
