using System;
using System.Collections.Generic;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

namespace GeoTetra.Partak
{
    [CreateAssetMenu(menuName = "Partak/AnalyticsRelay")]
    public class AnalyticsRelay : ScriptableObject
    {
        [SerializeField] private ServiceReference _gameStateReference;
        
        private int _levelPlayCount;

        public void GameTime(float gameTime)
        {
            Analytics.CustomEvent("GameTime_" + SceneManager.GetActiveScene().name, new Dictionary<string, object>
            {
                {"Time", gameTime}
            });
        }

        public void GamePlayerCount()
        {
            Analytics.CustomEvent("GamePlayerCount", new Dictionary<string, object>
            {
                {"PlayerCount", _gameStateReference.Service<GameState>().ActivePlayerCount()}
            });
            Analytics.CustomEvent("HumanPlayerCount", new Dictionary<string, object>
            {
                {"PlayerCount", _gameStateReference.Service<GameState>().ActiveHumanPlayerCount()}
            });
        }
    }
}