using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = System.Object;

namespace MultiCoreLibCSE {
    
    public static class GameLocationUtil {
        
        public static String gameSRCLoc = Application.persistentDataPath;
        public static String gameSettingsLoc = gameSRCLoc + "/settings/";
        public static String gameModsLoc = gameSRCLoc + "/mods/";
        public static String gameSavesLoc = gameSRCLoc + "/saves/";
        
        public static void checkGameLocationsIntegrity(String gameSettingsLoc_, String gameModsLoc_, String gameSavesLoc_) {
            if (!Directory.Exists(gameSettingsLoc_)) {
                Directory.CreateDirectory(gameSettingsLoc_);
            }
            if (!Directory.Exists(gameModsLoc_)) {
                Directory.CreateDirectory(gameModsLoc_);
            }
            if (!Directory.Exists(gameSavesLoc_)) {
                Directory.CreateDirectory(gameSavesLoc_);
            }
        }
        
    }
    
    public static class FileHelperUtil {

        public static void saveFile(Object saveObject, String location) {
            string jsonString = JsonUtility.ToJson(saveObject, true);
            File.WriteAllText(location, jsonString);
        }
        
    }

    public static class MathHelperUtil {
        public static float scale(float FromMin, float FromMax, float ToMin, float ToMax, float value) {

            float OldRange = (FromMax - FromMin);
            float NewRange = (ToMax - ToMin);
            float NewValue = (((value - FromMin) * NewRange) / OldRange) + ToMin;

            return NewValue;
        }
    
        public static int closestNumber(int input, int dividableBy) {
            int q = input / dividableBy;
      
            int n1 = dividableBy * q;
      
            int n2 = (input * dividableBy) > 0 ? (dividableBy * (q + 1)) : (dividableBy * (q - 1));

            if (Mathf.Abs(input - n1) < Mathf.Abs(input - n2)) {
                return n1;
            }
        
            return n2;     
        }
        
    }
}
