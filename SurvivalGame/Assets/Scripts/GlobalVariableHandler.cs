using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GlobalVariableHandler : MonoBehaviour
{
    public TMP_InputField WorldSeedInput;
    public TMP_InputField WorldNameInput;
    public Toggle useRandomSeedInsteadInput;

    public string worldName;
    public bool loadExisting;
    public bool ReadStartSettingsForGen = true;
    public int seed;
    public bool genRandomSeed;

    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        
    }

    void Update()
    {
        int.TryParse(WorldSeedInput.text, out seed);
        WorldNameInput.text = worldName;
        genRandomSeed = useRandomSeedInsteadInput.isOn;
    }


}
