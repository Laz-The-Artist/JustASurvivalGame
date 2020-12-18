using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveObject{

    [Header("JAWG - Generate")]
        public int s_WorldSize;
        public int s_WorldOffset;
        public int s_ChunkSize;
        public int s_WorldSeed;
        public float s_DayNightCycleLength;
    [Header("JAWG - Runtime")]
        public bool s_DoDayNightCycle;
        public float s_WorldTime;
        public int s_phase;
        public string s_CurrentDayTime;
        public float s_PlayerCoordX;
        public float s_PlayerCoordY;

}
