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
        [SerializeField] private ServiceReference _sceneLoadSystem;
        [SerializeField] private List<LevelButton> _levelButtons;
        [SerializeField] private AssetReference _newLevelScene;
        [SerializeField] private AssetReference _mainMenuScene;
        private List<Level> _levels = new List<Level>();

        private LevelButton _selectedLevelButton;

        public class Level
        {
            public string LevelName;
            public Texture PreviewImage;
        }
        
        private string[] EmptyLevelClickMessages;
        private Action[] EmptyLevelClickActions;
        
        protected override void Awake()
        {
            base.Awake();
            
            EmptyLevelClickMessages = new[] {"Download Existing Level", "Create New Level", "Cancel"};
            EmptyLevelClickActions = new Action[] {DownloadExistingLevel, CreateNewLevel, Cancel};

            LayoutUI();
        }

        private void LayoutUI()
        {
            for (int i = 0; i < _levelButtons.Count; ++i)
            {
                if (i < _levels.Count)
                {
                    _levelButtons[i].SetLevel(_levels[i]);
                }
                else if (i == _levels.Count)
                {
                    _levelButtons[i].SetLevel(null);
                    _levelButtons[i].Text.text = "Add\nLevel";
                    _levelButtons[i].Button.interactable = true;
                }
                else
                {
                    _levelButtons[i].SetLevel(null);
                }
                _levelButtons[i].ButtonClicked += OnLevelButtonClicked;
            }
        }
        
        private void OnLevelButtonClicked(LevelButton levelButton)
        {
            _selectedLevelButton = levelButton;
            DisplaySelectionModal(EmptyLevelClickMessages, EmptyLevelClickActions, 0);
        }

        private void DownloadExistingLevel()
        {
            
        }

        private void CreateNewLevel()
        {
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