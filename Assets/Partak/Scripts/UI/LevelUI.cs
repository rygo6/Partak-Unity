using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GeoTetra.GTCommon.Attributes;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using GeoTetra.GTUI;
using GeoTetra.Partak;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace GeoTetra.Partak
{
    public class LevelUI : StackUI
    {
        [SerializeField] private ServiceReference _gameState;
        [SerializeField] private ServiceReference _sceneLoadSystem;
        [SerializeField] private List<LevelButton> _levelButtons;
        [SerializeField] private AssetReference _newLevelScene;
        [SerializeField] private AssetReference _mainMenuScene;
        [SerializeField] private AssetReference _levelDownloadUI;

        private LevelButton _selectedLevelButton;

        private string[] EmptyLevelClickMessages;
        private Action[] EmptyLevelClickActions;
        
        private string[] LoadedLevelMessages;
        private Action[] LoadedLevelActions;
        
        protected override void Awake()
        {
            base.Awake();
            
            LoadedLevelMessages = new[] {"Edit Level", "Clear Level", "Cancel"};
            LoadedLevelActions = new Action[] {EditLevel, ClearLevel, Cancel};
            
            EmptyLevelClickMessages = new[] {"Download Level", "Create Level", "Cancel"};
            EmptyLevelClickActions = new Action[] {DownloadExistingLevel, CreateNewLevel, Cancel};
            
            for (int i = 0; i < _levelButtons.Count; ++i)
            {
                _levelButtons[i].ButtonClicked += OnLevelButtonClicked;
            }
        }

        private void LayoutUI()
        {
            bool addLevelSet = false;
            for (int i = 0; i < _levelButtons.Count; ++i)
            {
                string levelPath = LevelUtility.LevelPath(i);
                bool levelExists = System.IO.File.Exists(levelPath);
                
                _levelButtons[i].Index0 = i;
                if (levelExists)
                {
                    string imagePath = LevelUtility.LevelImagePath(i);
                    byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
                    Texture2D image = new Texture2D(0,0);
                    image.LoadImage(imageBytes, true);

                    _levelButtons[i].Text.text = "";
                    _levelButtons[i].Image.color = Color.white;
                    _levelButtons[i].Image.texture = image;
                    _levelButtons[i].Level = true;
                    _levelButtons[i].Button.interactable = true;
                }
                else if (!addLevelSet)
                {
                    addLevelSet = true;
                    _levelButtons[i].Image.color = new Color(1,1,1,.5f);
                    _levelButtons[i].Image.texture = null;
                    _levelButtons[i].Text.text = "Add\nLevel";
                    _levelButtons[i].Level = false;
                    _levelButtons[i].Button.interactable = true;
                }
                else
                {
                    _levelButtons[i].Image.color = new Color(1,1,1,.5f);
                    _levelButtons[i].Image.texture = null;
                    _levelButtons[i].Text.text = "";
                    _levelButtons[i].Level = false;
                    _levelButtons[i].Button.interactable = false;
                }
            }
        }

        public override void OnTransitionInStart(UIRenderer uiRenderer)
        {
            base.OnTransitionInStart(uiRenderer);
            LayoutUI();
        }

        private void OnLevelButtonClicked(LevelButton levelButton)
        {
            _selectedLevelButton = levelButton;
            if (_selectedLevelButton.Level)
            {
                DisplaySelectionModal("", LoadedLevelMessages, LoadedLevelActions, 0);
            }
            else
            {
                DisplaySelectionModal("", EmptyLevelClickMessages, EmptyLevelClickActions, 0);
            }
        }
        
        private void ClearLevel()
        {
            string levelPath = LevelUtility.LevelPath(_selectedLevelButton.Index0);
            string imagePath = LevelUtility.LevelImagePath(_selectedLevelButton.Index0);
            System.IO.File.Delete(levelPath);
            System.IO.File.Delete(imagePath);
            
            LayoutUI();
        }
        
        private void EditLevel()
        {
            _gameState.Service<GameState>().EditingLevelIndex = _selectedLevelButton.Index0;
            _sceneLoadSystem.Service<SceneLoadSystem>().Load(_mainMenuScene, _newLevelScene);
        }

        private void DownloadExistingLevel()
        {
            _gameState.Service<GameState>().EditingLevelIndex = _selectedLevelButton.Index0;
            CurrentlyRenderedBy.InstantiateAndDisplayStackUI(_levelDownloadUI);
        }

        private void CreateNewLevel()
        {
            _gameState.Service<GameState>().EditingLevelIndex = _selectedLevelButton.Index0;
            _sceneLoadSystem.Service<SceneLoadSystem>().Load(_mainMenuScene, _newLevelScene);
        }

        private void Cancel()
        {
            
        }

        [ContextMenu("Collect Levels")]
        private void CollectLevels()
        {
            _levelButtons = FindObjectsOfType<LevelButton>().ToList();
        }
    }
}