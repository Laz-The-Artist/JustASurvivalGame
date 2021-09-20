using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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

    }
}
