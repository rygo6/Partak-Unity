using System;
using System.Collections.Generic;
using GeoTetra.GTCommon.Variables;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

namespace Partak
{
    public class AnalyticsRelay : MonoBehaviour
    {
        [SerializeField] private GameState _gameState;
        
        private int _levelPlayCount;

        private void OnApplicationPause(bool pauseStatus)
        {
            LevelQuit();
            LevelPlayCount();
        }

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
                {"PlayerCount", _gameState.ActivePlayerCount()}
            });
            Analytics.CustomEvent("HumanPlayerCount", new Dictionary<string, object>
            {
                {"PlayerCount", _gameState.ActiveHumanPlayerCount()}
            });
        }

        public void MenuLevelLoad()
        {
            string levelName = "Level" + (_gameState.LevelIndex + 1);
            Analytics.CustomEvent("MenuLeveLoad", new Dictionary<string, object>
            {
                {"LevelName", levelName}
            });
            ++_levelPlayCount;
            GamePlayerCount();
        }

        public void NextLevel()
        {
            ++_levelPlayCount;
            GamePlayerCount();
        }

        public void SkipLevel()
        {
            Analytics.CustomEvent("SkipLevel", new Dictionary<string, object>
            {
                {"LevelName", SceneManager.GetActiveScene().name}
            });
            GamePlayerCount();
        }

        public void ReplayLevel()
        {
            Analytics.CustomEvent("ReplayLevel", new Dictionary<string, object>
            {
                {"LevelName", SceneManager.GetActiveScene().name}
            });
            ++_levelPlayCount;
            GamePlayerCount();
        }

        private void LevelPlayCount()
        {
            Analytics.CustomEvent("LevelPlayCount", new Dictionary<string, object>
            {
                {"count", _levelPlayCount}
            });
        }

        private void LevelQuit()
        {
            Analytics.CustomEvent("LevelQuit", new Dictionary<string, object>
            {
                {"LevelName", SceneManager.GetActiveScene().name}
            });
        }
    }
}