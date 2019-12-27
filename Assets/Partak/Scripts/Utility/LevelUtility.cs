using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeoTetra.Partak
{
    public static class LevelUtility
    {
        public static string LevelPath(int index)
        {
            return System.IO.Path.Combine(Application.persistentDataPath, $"level{index}.level");
        }
        
        public static string LevelImagePath(int index)
        {
            return System.IO.Path.Combine(Application.persistentDataPath, $"level{index}.png");
        }
    }
}