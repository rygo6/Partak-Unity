using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeoTetra.Partak
{
    public static class LevelUtility
    {
        public static string LevelPath(string index)
        {
            return System.IO.Path.Combine(Application.persistentDataPath, $"{index}.level");
        }
        
        public static string LevelImagePath(string index)
        {
            return System.IO.Path.Combine(Application.persistentDataPath, $"{index}.png");
        }
        
        public static LocalLevelDatum GetLevelDatum(string levelId)
        {
            string path = LevelUtility.LevelPath(levelId);
            string json = System.IO.File.ReadAllText(path);
            return JsonUtility.FromJson<LocalLevelDatum>(json);
        }
    }
    
    [Serializable]
    public class LevelCatalogDatum
    {
        [SerializeField]
        public List<string> _levelIDs = new List<string>();

        public List<string> LevelIDs => _levelIDs;

        public static LevelCatalogDatum LoadLevelCatalogDatum()
        {
            if (System.IO.File.Exists(LevelCatalogDatumPath()))
            {
                string json = System.IO.File.ReadAllText(LevelCatalogDatumPath());
                LevelCatalogDatum datum = JsonUtility.FromJson<LevelCatalogDatum>(json);
                return datum;
            }
            else
            {
                return new LevelCatalogDatum();
            }
        }
        
        public void SaveLevelCatalogDatum()
        {
            string json = JsonUtility.ToJson(this);
            System.IO.File.WriteAllText(LevelCatalogDatumPath(), json);
        }

        private static string LevelCatalogDatumPath()
        {
            return System.IO.Path.Combine(Application.persistentDataPath, "levelcatalogdatum");
        }
    }

}