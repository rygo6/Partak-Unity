using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GeoTetra.GTBackend;
using GeoTetra.GTUI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace GeoTetra.Partak
{
    public class MainUI : StackUI
    {
        [SerializeField] private PartakStateRef _partakState;
        [SerializeField] private PartakAWSRef _partakAWS;
        [SerializeField] private Button _playButton;
        [SerializeField] private AssetReference _playUI;
        [SerializeField] private Button _levelButton;
        [SerializeField] private AssetReference _levelUI;
        [SerializeField] private Button _optionButton;
        [SerializeField] private AssetReference _optionsUI;
        [SerializeField] private List<string> _initialLevelIds;       
        
        protected override async Task StartAsync()
        {
            await _partakState.Cache(this);
            
            _playButton.onClick.AddListener(() =>
            {
                if (_partakState.Ref.LevelCatalogDatum.LevelIDs.Count == 0)
                {
                    if (_partakState.Ref.InitialLevelsDownloaded)
                    {
                        CurrentlyRenderedBy.DisplayMessageModal("You have no levels loaded! Go into the level editor to download or create levels.");
                    }
                }
                else
                {
                    InstantiateAndDisplayStackUI(_playUI);
                }
            });
            InstantiateAndDisplayStackUIOnClick(_levelButton, _levelUI);
            InstantiateAndDisplayStackUIOnClick(_optionButton, _optionsUI);

            await base.StartAsync();
        }

        public override void OnTransitionInFinish()
        {
            DownloadInitialLevels();
            base.OnTransitionInFinish();
        }

        private async void DownloadInitialLevels()
        {
            if (_partakState.Ref.LevelCatalogDatum.LevelIDs.Count == 0 && !_partakState.Ref.InitialLevelsDownloaded)
            {
                await CurrentlyRenderedBy.DisplayLoadModal("Downloading Initial Levels...");
                await _partakAWS.Cache(this);
                foreach (string initialLevelId in _initialLevelIds)
                {
                    await _partakAWS.Ref.DownloadLevel(initialLevelId);
                    _partakState.Ref.AddLevelId(initialLevelId);
                }
                _partakState.Ref.SetInitialLevelsDownloaded();
                CurrentlyRenderedBy.CloseModal();
            }
        }
    }
}