﻿using System;
using System.Collections.Generic;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

namespace GeoTetra.Partak
{
    [Serializable]
    public class AnalyticsServiceRef : ServiceObjectReferenceT<AnalyticsService>
    {
        public AnalyticsServiceRef(string guid) : base(guid)
        { }
    }
    
    [CreateAssetMenu(menuName = "GeoTetra/Services/AnalyticsService", fileName = "AnalyticsService.asset")]
    public class AnalyticsService : ServiceObject
    {
        [SerializeField]
        private PartakStateRef _partakState;
        
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
                {"PlayerCount", _partakState.Ref.ActivePlayerCount()}
            });
            Analytics.CustomEvent("HumanPlayerCount", new Dictionary<string, object>
            {
                {"PlayerCount", _partakState.Ref.ActiveHumanPlayerCount()}
            });
        }
    }
}