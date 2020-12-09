using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GeoTetra.GTUI;
using UnityEngine.AddressableAssets;

namespace GeoTetra.Partak
{
    public class LevelCatalogUI : StackUI
    {
        [SerializeField] private AnalyticsRelayReference _analyticsRelay;
        [SerializeField] private PartakStateRef _partakState;
        [SerializeField] private SceneTransitRef _sceneTransit;
        [SerializeField] private AssetReference _newLevelScene;
        [SerializeField] private AssetReference _mainMenuScene;
        [SerializeField] private AssetReference _levelDownloadUI;
        [SerializeField] private AssetReference _purchaseUI;
        [SerializeField] private LevelButtonScrollRect _levelButtonScrollRect; 
        
        private int _catalogDatumIndex;
        private LevelButton _selectedLevelButton;
        
        private string[] _emptyLevelClickMessages;
        private Action[] _emptyLevelClickActions;

        private string[] _loadedLevelMessages;
        private Action[] _loadedLevelActions;

        protected override void Awake()
        {
            base.Awake();
            _levelButtonScrollRect.LevelButtonClicked += OnLevelButtonClicked;
            
            _loadedLevelMessages = new[] {"Delete Level", "Cancel"};
            _loadedLevelActions = new Action[] {ClearLevel, Cancel};
            
            _emptyLevelClickMessages = new[] {"Download Level", "Create Level", "Cancel"};
            _emptyLevelClickActions = new Action[] {DownloadExistingLevel, CreateNewLevel, Cancel};
        }

        protected override async Task StartAsync()
        {
            await _partakState.Cache(this);
            await _sceneTransit.Cache(this);

            await base.StartAsync();
        }

        public override void OnTransitionInStart(UIRenderer uiRenderer)
        {
            base.OnTransitionInStart(uiRenderer);
        }

        public override void OnTransitionInFinish()
        {
            base.OnTransitionInFinish();
            _catalogDatumIndex = 0;
            _levelButtonScrollRect.Initialize(DownloadNextSet, PopulateLevelButton, FinalButton);
        }

        public override void OnTransitionOutFinish()
        {
            base.OnTransitionOutFinish();
            _levelButtonScrollRect.Clear();
        }

        private Task PopulateLevelButton(LevelButton levelButton, CancellationToken cancellationToken)
        {
            levelButton.LoadTextureFromDisk(LevelUtility.LevelImagePath(levelButton.LevelDatum.LevelID));
            levelButton.ShowRating(false);
            levelButton.Text.text = "";
            levelButton.Image.color = Color.white;
            levelButton.Button.interactable = true;
            return Task.CompletedTask;
        }

        private Task<bool> DownloadNextSet(List<List<LocalLevelDatum>> datumLists, CancellationToken cancellationToken)
        {
            if (!_partakState.Service.FullVersion && _catalogDatumIndex >= 3)
            {
                return Task.FromResult(true);
            }
            
            if (_catalogDatumIndex >= _partakState.Service.LevelCatalogDatum.LevelIDs.Count) return Task.FromResult(true);
            
            List<LocalLevelDatum> levelDatumList = new List<LocalLevelDatum>();
            for (int i = 0; i < _levelButtonScrollRect.ColumnCount; ++i)
            {
                if (_catalogDatumIndex >= _partakState.Service.LevelCatalogDatum.LevelIDs.Count) break;

                string levelPath = LevelUtility.LevelPath(_partakState.Service.LevelCatalogDatum.LevelIDs[_catalogDatumIndex]);
                string json = File.ReadAllText(levelPath);
                LocalLevelDatum levelDatum = JsonUtility.FromJson<LocalLevelDatum>(json);
                levelDatumList.Add(levelDatum);
                _catalogDatumIndex++;
            }
            datumLists.Add(levelDatumList);

            return Task.FromResult(_catalogDatumIndex >= _partakState.Service.LevelCatalogDatum.LevelIDs.Count);
        }

        private void FinalButton(LevelButton levelButton)
        {
            levelButton.Image.color = new Color(1,1,1,.5f);
            levelButton.Image.texture = null;
            levelButton.Button.interactable = true;
            
            if (!_partakState.Service.FullVersion && _catalogDatumIndex >= 3)
            {
                levelButton.Text.text = "Unlock\nLevel Slot";
            }
            else
            {
                levelButton.Text.text = "Add\nLevel";
            }
        }

        private void OnLevelButtonClicked(LevelButton levelButton)
        {
            _selectedLevelButton = levelButton;
            
            if (levelButton.LevelDatum == null)
            {
                if (!_partakState.Service.FullVersion && _catalogDatumIndex >= 3)
                {
                    PurchaseFullVersion();
//                    CurrentlyRenderedBy.DisplayMessageModal("Purchase full version to unlock unlimited level slots, unlock the ability to edit levels and disable all ads.", PurchaseFullVersion);
                }
                else
                {
                    CurrentlyRenderedBy.DisplaySelectionModal("", _emptyLevelClickMessages, _emptyLevelClickActions, 0);
                }
            }
            else
            {
                CurrentlyRenderedBy.DisplaySelectionModal("", _loadedLevelMessages, _loadedLevelActions, 0);
            }
        }

        private async void PurchaseFullVersion()
        {
            await CurrentlyRenderedBy.InstantiateAndDisplayModalUI(_purchaseUI);
        }

        private void Cancel()
        {
            
        }

        private void ClearLevel()
        {
            string levelPath = LevelUtility.LevelPath(_selectedLevelButton.LevelDatum.LevelID);
            string imagePath = LevelUtility.LevelImagePath(_selectedLevelButton.LevelDatum.LevelID);
            System.IO.File.Delete(levelPath);
            System.IO.File.Delete(imagePath);
            _partakState.Service.RemoveLevelId(_selectedLevelButton.LevelDatum.LevelID);
            _catalogDatumIndex = 0;
            _levelButtonScrollRect.Clear();
            _levelButtonScrollRect.Initialize(DownloadNextSet, PopulateLevelButton, FinalButton);
            _analyticsRelay.Service.LevelDeleted();
            
            //Reset level index so play menu doesn't load on empty level.
            _partakState.Service.LevelIndex = 0;
        }
        
        private void EditLevel()
        {
            _partakState.Service.EditingLevelIndex = _selectedLevelButton.TotalIndex(_levelButtonScrollRect.ColumnCount);
            _sceneTransit.Service.Load(_mainMenuScene, _newLevelScene);
            
            //Reset level index so play menu doesn't load on empty level.
            _partakState.Service.LevelIndex = 0;
        }
        
        private void DownloadExistingLevel()
        {
            _partakState.Service.EditingLevelIndex = _selectedLevelButton.TotalIndex(_levelButtonScrollRect.ColumnCount);
            CurrentlyRenderedBy.InstantiateAndDisplayStackUI(_levelDownloadUI);
            _analyticsRelay.Service.DownloadLevelOpened();
        }

        private void CreateNewLevel()
        {
            _partakState.Service.EditingLevelIndex = _selectedLevelButton.TotalIndex(_levelButtonScrollRect.ColumnCount);
            _sceneTransit.Service.Load(_mainMenuScene, _newLevelScene);
            _analyticsRelay.Service.CreateLevelOpened();
            
            //Reset level index so play menu doesn't load on empty level.
            _partakState.Service.LevelIndex = 0;
        }
    }
}