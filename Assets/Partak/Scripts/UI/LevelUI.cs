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
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Partak
{
    public class LevelUI : StackUI
    {
        [SerializeField] private ServiceReference _gameState;
        [SerializeField] private ServiceReference _sceneLoadSystem;
        [SerializeField] private List<LevelButton> _levelButtons;
        [SerializeField] private AssetReference _newLevelScene;
        [SerializeField] private AssetReference _mainMenuScene;

        private LevelButton _selectedLevelButton;

        public class Level
        {
            public string LevelName;
            public Texture PreviewImage;
        }
        
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
                string levelPath = LevelPath(i);
                bool levelExists = System.IO.File.Exists(levelPath);
                
                _levelButtons[i].Initialize(i);
                if (levelExists)
                {
                    _levelButtons[i].Text.text = i.ToString();
                    _levelButtons[i].Level = true;
                    _levelButtons[i].Button.interactable = true;
                }
                else if (!addLevelSet)
                {
                    addLevelSet = true;
                    _levelButtons[i].Text.text = "Add\nLevel";
                    _levelButtons[i].Level = false;
                    _levelButtons[i].Button.interactable = true;
                }
                else
                {
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
                DisplaySelectionModal(LoadedLevelMessages, LoadedLevelActions, 0);
            }
            else
            {
                DisplaySelectionModal(EmptyLevelClickMessages, EmptyLevelClickActions, 0);
            }
        }
        
        private void ClearLevel()
        {
//            _levels.RemoveAt(_selectedLevelButton.Index);
            LayoutUI();
        }
        
        private void EditLevel()
        {
            _gameState.Service<GameState>().EditingLevelIndex = _selectedLevelButton.Index;
            _sceneLoadSystem.Service<SceneLoadSystem>().Load(_mainMenuScene, _newLevelScene);
        }

        private void DownloadExistingLevel()
        {
            
        }

        private void CreateNewLevel()
        {
            _gameState.Service<GameState>().EditingLevelIndex = _selectedLevelButton.Index;
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
        
        public string LevelPath(int index)
        {
            return System.IO.Path.Combine(Application.persistentDataPath, $"level{index}");
        }
    }
}