using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIHandler : MonoBehaviour
{
    [Header("Script References")]
        public GameObject World;
        WorldGeneratorV3 WorldGenScript;

    [Header("UI References")]
        public Camera MainCam;
        public Image UI_icon_dayphase_day;
        public Image UI_icon_dayphase_night;
        public Image UI_icon_dayphase_fog;
        public TextMeshProUGUI Disp_Time;
        public TextMeshProUGUI Disp_Phase;

    [Header("UI Settings")]
        [Range(2, 16)]public float FieldOfView = 7;

    void Start()
    {
        WorldGenScript = World.GetComponent<WorldGeneratorV3>();
    }

    void Update()
    {
        UpdateUI();

    }

    public void UpdateUI() {
        //MAIN
        MainCam.orthographicSize = FieldOfView;

        //Celestial Watch
        UI_icon_dayphase_day.fillAmount = WorldGenScript.WorldTime / WorldGenScript.SettingDayNightCycleLength;
        Disp_Time.text = "" + WorldGenScript.WorldTime;
        Disp_Phase.text = "" + WorldGenScript.CurrentDaytime;

    }
}
