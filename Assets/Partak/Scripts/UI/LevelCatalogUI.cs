using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using GeoTetra.GTBackend;
using GeoTetra.GTCommon.Attributes;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using GeoTetra.GTUI;
using UnityEngine.AddressableAssets;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace GeoTetra.Partak
{
    public class LevelCatalogUI : StackUI
    {
        [SerializeField] private AssetReference _loadModalUI;
        [SerializeField] private ServiceReference _gameStateService;
        [SerializeField] private LevelButtonScrollRect _levelButtonScrollRect; 
        [SerializeField] private AssetReference _newLevelScene;
        [SerializeField] private AssetReference _mainMenuScene;
        [SerializeField] private ServiceReference _sceneLoadSystem;
        [SerializeField] private AssetReference _levelDownloadUI;
        
        private int _catalogDatumIndex;
        private GameState _gameState;
        private LevelButton _selectedLevelButton;
        
        private string[] EmptyLevelClickMessages;
        private Action[] EmptyLevelClickActions;

        private string[] LoadedLevelMessages;
        private Action[] LoadedLevelActions;
        

        protected override void Awake()
        {
            base.Awake();
            _gameState = _gameStateService.Service<GameState>();
            _levelButtonScrollRect.LevelButtonClicked += OnLevelButtonClicked;
            
//            LoadedLevelMessages = new[] {"Edit Level", "Clear Level", "Cancel"};
//            LoadedLevelActions = new Action[] {EditLevel, ClearLevel, Cancel};
            LoadedLevelMessages = new[] {"Delete Level", "Cancel"};
            LoadedLevelActions = new Action[] {ClearLevel, Cancel};
            
            EmptyLevelClickMessages = new[] {"Download Level", "Create Level", "Cancel"};
            EmptyLevelClickActions = new Action[] {DownloadExistingLevel, CreateNewLevel, Cancel};
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

        private async Task PopulateLevelButton(LevelButton levelButton, CancellationToken cancellationToken)
        {
            levelButton.ShowRating(false);
            levelButton.LoadTextureFromDisk(LevelUtility.LevelImagePath(levelButton.LevelDatum.LevelID));
            levelButton.ShowRating(false);
        }

        private async Task<bool> DownloadNextSet(List<List<LocalLevelDatum>> datumLists)
        {
            if (_catalogDatumIndex >= _gameState.LevelCatalogDatum.LevelIDs.Count) return true;
            
            List<LocalLevelDatum> levelDatumList = new List<LocalLevelDatum>();
            for (int i = 0; i < _levelButtonScrollRect.ColumnCount; ++i)
            {
                if (_catalogDatumIndex >= _gameState.LevelCatalogDatum.LevelIDs.Count) break;

                string levelPath = LevelUtility.LevelPath(_gameState.LevelCatalogDatum.LevelIDs[_catalogDatumIndex]);
                string json = System.IO.File.ReadAllText(levelPath);
                LocalLevelDatum levelDatum = JsonUtility.FromJson<LocalLevelDatum>(json);
                levelDatumList.Add(levelDatum);
                _catalogDatumIndex++;
            }
            datumLists.Add(levelDatumList);

            return _catalogDatumIndex >= _gameState.LevelCatalogDatum.LevelIDs.Count;
        }

        private void FinalButton(LevelButton levelButton)
        {
            levelButton.Image.color = new Color(1,1,1,.5f);
            levelButton.Image.texture = null;
            levelButton.Text.text = "Add\nLevel";
            levelButton.ShowingLevel = false;
            levelButton.Button.interactable = true;
        }

        private async void OnLevelButtonClicked(LevelButton levelButton)
        {
            _selectedLevelButton = levelButton;
            
            if (levelButton.LevelDatum == null)
            {
                DisplaySelectionModal("", EmptyLevelClickMessages, EmptyLevelClickActions, 0);
            }
            else
            {
                DisplaySelectionModal("", LoadedLevelMessages, LoadedLevelActions, 0);
            }
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
            _gameState.RemoveLevelId(_selectedLevelButton.LevelDatum.LevelID);
            _levelButtonScrollRect.Clear();
            _levelButtonScrollRect.Initialize(DownloadNextSet, PopulateLevelButton, FinalButton);
        }
        
        private void EditLevel()
        {
            _gameState.EditingLevelIndex = _selectedLevelButton.TotalIndex(_levelButtonScrollRect.ColumnCount);
            _sceneLoadSystem.Service<SceneLoadSystem>().Load(_mainMenuScene, _newLevelScene);
        }
        
        private void DownloadExistingLevel()
        {
            _gameState.EditingLevelIndex = _selectedLevelButton.TotalIndex(_levelButtonScrollRect.ColumnCount);
            CurrentlyRenderedBy.InstantiateAndDisplayStackUI(_levelDownloadUI);
        }

        private void CreateNewLevel()
        {
            _gameState.EditingLevelIndex = _selectedLevelButton.TotalIndex(_levelButtonScrollRect.ColumnCount);
            _sceneLoadSystem.Service<SceneLoadSystem>().Load(_mainMenuScene, _newLevelScene);
        }
    }
}