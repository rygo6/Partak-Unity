using System;
using System.Collections.Generic;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

namespace GeoTetra.Partak
{
    [Serializable]
    public class AnalyticsRelayReference : ServiceReferenceT<AnalyticsRelay>
    {
        public AnalyticsRelayReference(string guid) : base(guid)
        { }
    }
    
    public class AnalyticsRelay : ServiceBehaviour
    {
        [SerializeField]
        private PartakStateRef _partakState;

        private void Awake()
        {
            OnLoadComplete();
        }

        public void LevelDownloaded()
        {
            Analytics.CustomEvent("LevelDownloaded");
        }
        
        public void DownloadLevelOpened()
        {
            Analytics.CustomEvent("DownloadLevelOpen");
        }
        
        public void CreateLevelUploaded()
        {
            Analytics.CustomEvent("LevelUploaded");
        }
        
        public void LevelDeleted()
        {
            Analytics.CustomEvent("LevelDeleted");
        }
        
        public void CreateLevelOpened()
        {
            Analytics.CustomEvent("CreateLevelOpened");
        }
        
        public void CreateLevelSaved()
        {
            Analytics.CustomEvent("CreateLevelSaved");
        }
        
        public void CreateLevelCancelled()
        {
            Analytics.CustomEvent("CreateLevelCancelled");
        }

        public void GameTime(float gameTime)
        {
            Analytics.CustomEvent("GameTime", new Dictionary<string, object>
            {
                {"Time", gameTime}
            });
        }

        public async void GamePlayerCount()
        {
            await _partakState.Cache(this);
            
            Analytics.CustomEvent("GamePlayerCount", new Dictionary<string, object>
            {
                {"PlayerCount", _partakState.Service.ActivePlayerCount()}
            });
            Analytics.CustomEvent("HumanPlayerCount", new Dictionary<string, object>
            {
                {"PlayerCount", _partakState.Service.ActiveHumanPlayerCount()}
            });
        }
    }
}