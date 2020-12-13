using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GlobalVariableHandler : MonoBehaviour
{
    public TMP_InputField WorldSeed;
    public Toggle useRandomSeedInstead;

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
        int.TryParse(WorldSeed.text, out seed);
        genRandomSeed = useRandomSeedInstead.isOn;
    }


}
