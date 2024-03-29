﻿using System;
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
        [SerializeField] private AnalyticsServiceRef _analyticsService;
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
        
        private string[] _editableLoadedLevelMessages;
        private Action[] _editableLoadedLevelActions;

        protected override void Awake()
        {
            base.Awake();
            _levelButtonScrollRect.LevelButtonClicked += OnLevelButtonClicked;
            
            _editableLoadedLevelMessages = new[] {"Edit Level", "Delete Level", "Cancel"};
            _editableLoadedLevelActions = new Action[] {EditLevel, ClearLevel, Cancel};
            
            _loadedLevelMessages = new[] {"Delete Level", "Cancel"};
            _loadedLevelActions = new Action[] {ClearLevel, Cancel};
            
            _emptyLevelClickMessages = new[] {"Download Level", "Create Level", "Cancel"};
            _emptyLevelClickActions = new Action[] {DownloadExistingLevel, CreateNewLevel, Cancel};
        }

        protected override async Task StartAsync()
        {
            await Task.WhenAll(_partakState.Cache(this),
                    _sceneTransit.Cache(this),
                    _analyticsService.Cache(this));

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
            if (!_partakState.Ref.FullVersion && _catalogDatumIndex >= 3)
            {
                return Task.FromResult(true);
            }
            
            if (_catalogDatumIndex >= _partakState.Ref.LevelCatalogDatum.LevelIDs.Count) return Task.FromResult(true);
            
            List<LocalLevelDatum> levelDatumList = new List<LocalLevelDatum>();
            for (int i = 0; i < _levelButtonScrollRect.ColumnCount; ++i)
            {
                if (_catalogDatumIndex >= _partakState.Ref.LevelCatalogDatum.LevelIDs.Count) break;

                string levelPath = LevelUtility.LevelPath(_partakState.Ref.LevelCatalogDatum.LevelIDs[_catalogDatumIndex]);
                string json = File.ReadAllText(levelPath);
                LocalLevelDatum levelDatum = JsonUtility.FromJson<LocalLevelDatum>(json);
                levelDatumList.Add(levelDatum);
                _catalogDatumIndex++;
            }
            datumLists.Add(levelDatumList);

            return Task.FromResult(_catalogDatumIndex >= _partakState.Ref.LevelCatalogDatum.LevelIDs.Count);
        }

        private void FinalButton(LevelButton levelButton)
        {
            levelButton.Image.color = Color.clear;
            levelButton.Image.texture = null;
            levelButton.Button.interactable = true;
            
            if (!_partakState.Ref.FullVersion && _catalogDatumIndex >= 3)
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
                if (!_partakState.Ref.FullVersion && _catalogDatumIndex >= 3)
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
                if (levelButton.LevelDatum.Downloaded)
                {
                    CurrentlyRenderedBy.DisplaySelectionModal("", _loadedLevelMessages, _loadedLevelActions, 0);
                }
                else
                {
                    CurrentlyRenderedBy.DisplaySelectionModal("", _editableLoadedLevelMessages, _editableLoadedLevelActions, 0);
                }
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
            _partakState.Ref.RemoveLevelId(_selectedLevelButton.LevelDatum.LevelID);
            _catalogDatumIndex = 0;
            _levelButtonScrollRect.Clear();
            _levelButtonScrollRect.Initialize(DownloadNextSet, PopulateLevelButton, FinalButton);
            _analyticsService.Ref.LevelDeleted();
            
            //Reset level index so play menu doesn't load on empty level.
            _partakState.Ref.LevelIndex = 0;
        }
        
        private void EditLevel()
        {
            _partakState.Ref.EditingLevelIndex = _selectedLevelButton.TotalIndex(_levelButtonScrollRect.ColumnCount);
            _sceneTransit.Ref.Load(_mainMenuScene, _newLevelScene);

            //Reset level index so play menu doesn't load on empty level.
            _partakState.Ref.LevelIndex = 0;
        }
        
        private void DownloadExistingLevel()
        {
            _partakState.Ref.EditingLevelIndex = _selectedLevelButton.TotalIndex(_levelButtonScrollRect.ColumnCount);
            CurrentlyRenderedBy.InstantiateAndDisplayStackUI(_levelDownloadUI);
            _analyticsService.Ref.DownloadLevelOpened();
        }

        private void CreateNewLevel()
        {
            _partakState.Ref.EditingLevelIndex = _selectedLevelButton.TotalIndex(_levelButtonScrollRect.ColumnCount);
            _sceneTransit.Ref.Load(_mainMenuScene, _newLevelScene);
            _analyticsService.Ref.CreateLevelOpened();
            
            //Reset level index so play menu doesn't load on empty level.
            _partakState.Ref.LevelIndex = 0;
        }
    }
}