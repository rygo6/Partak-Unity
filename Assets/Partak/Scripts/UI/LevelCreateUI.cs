using System;
using System.Collections;
using GeoTetra.GTBackend;
using UnityEngine;
using UnityEngine.UI;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using GeoTetra.GTSnapper;
using GeoTetra.GTUI;
using GeoTetra.Partak;
using UnityEngine.AddressableAssets;

namespace GeoTetra.Partak
{
    public class LevelCreateUI : StackUI
    {
        [SerializeField] private ServiceReference _gameState;
        [SerializeField] private ServiceReference _sceneLoadSystem;
        [SerializeField] private ServiceReference _componentContainer;
        [SerializeField] private ServiceReference _database;
        [SerializeField] private AssetReference _mainMenuScene;
        [SerializeField] private AssetReference _newLevelScene;
        [SerializeField] private AssetReference _loadModalUI;
        [SerializeField] private ItemCatalogUI _itemCatalogUI;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Button _validateButton;

        private string[] _changeSizeMessages;
        private Action[] _changeSizeActions;
        
        private string[] _saveMessages;
        private Action[] _saveActions;

        private LevelConfig _levelConfig;
        private LevelTester _levelTester;
        private string _editingLevelId;

        protected override void Awake()
        {
            base.Awake();
            
            _changeSizeMessages = new[] {"128x128", "192x192", "256x144", "256x256"};
            _changeSizeActions = new Action[]
            {
                () => SizeChanged(new Vector2Int(128,128)), 
                () => SizeChanged(new Vector2Int(192,192)),
                () => SizeChanged(new Vector2Int(256,144)),
                () => SizeChanged(new Vector2Int(256,256))
            };
            
            _saveMessages = new[] {"Yes", "No"};
            _saveActions = new Action[]
            {
                SaveToAWS,
                SerializeLevel,
            };
            
            _saveButton.onClick.AddListener(OnClickSave);
            _cancelButton.onClick.AddListener(OnCloseClick);
            _validateButton.onClick.AddListener(OnValidateClick);
        }

        public override void OnTransitionInFinish()
        {
            base.OnTransitionInFinish();
            _levelConfig = _componentContainer.Service<ComponentContainer>().Get<LevelConfig>();
            _levelTester = _componentContainer.Service<ComponentContainer>().Get<LevelTester>();
            _itemCatalogUI.Initialize();

            _editingLevelId = _gameState.Service<GameState>().GetEditingLevelId();
            string levelPath = LevelUtility.LevelPath(_editingLevelId);
            
            if (!System.IO.File.Exists(levelPath))
            {
                DisplaySelectionModal("Select Size", _changeSizeMessages, _changeSizeActions, 0);
            }
            else
            {
                _componentContainer.Service<ComponentContainer>().Get<LevelConfig>().Deserialize(_editingLevelId, true);   
            }
        }

        public override void OnTransitionOutStart()
        {
            base.OnTransitionOutStart();
            _itemCatalogUI.Deinitialize();
        }

        private void OnClickSave()
        {
            StartCoroutine(OnClickSaveCoroutine());
        }

        private IEnumerator OnClickSaveCoroutine()
        {
            yield return StartCoroutine(RunTestCoroutine());
            if (_levelTester.Result == LevelTester.TestResult.Success)
            {
                _levelConfig.Datum.Shared = false;
                if (_levelConfig.Datum.Shared)
                {
                    SerializeLevel();
                    OnCloseClick();
                }
                else
                {
                    DisplaySelectionModal("Share Level Online?", _saveMessages, _saveActions, 0);
                }
            }
            else
            {
                DisplayFailMessages();
            }
        }
        
        private void OnCloseClick()
        {
            OnBackClicked();
            _sceneLoadSystem.Service<SceneLoadSystem>().Load(_newLevelScene, _mainMenuScene);
        }

        private void SerializeLevel()
        {
            _levelConfig.Serialize(_editingLevelId);
            _gameState.Service<GameState>().AddLevelId(_editingLevelId);
        }

        private async void SaveToAWS()
        {
            _levelConfig.Datum.Shared = true;
            SerializeLevel();
            await _database.Service<PartakDatabase>().SaveLevel(_editingLevelId);
            
            OnCloseClick();
        }

        private void OnValidateClick()
        {
            StartCoroutine(OnValidateClickCoroutine());
        }

        private IEnumerator OnValidateClickCoroutine()
        {
            yield return StartCoroutine(RunTestCoroutine());
            if (_levelTester.Result == LevelTester.TestResult.Success)
            {
                DisplayModal("Test Successful.", null);
            }
            else
            {
                DisplayFailMessages();
            }
        }

        private void DisplayFailMessages()
        {
            if (_levelTester.Result == LevelTester.TestResult.CursorsBlocked)
            {
                DisplayModal("An object is blocking the spawn position of a particle. Remove any objects beneath a player cursor.", null);
            }
            else if (_levelTester.Result == LevelTester.TestResult.SpawnBlocked)
            {
                DisplayModal("There is not enough room for all the particles to spawn. Move objects away from where the particles spawn.", null);
            } 
            else if (_levelTester.Result == LevelTester.TestResult.ParticlesBlocked)
            {
                DisplayModal("All of the particles could not reach each other. Ensure there are open paths for each player's particles to reach every other player's particles.", null);
            }
        }
        
        private IEnumerator RunTestCoroutine()
        {
            CurrentlyRenderedBy.InstantiateAndDisplayModalUI(_loadModalUI);
            yield return StartCoroutine(_levelTester.RunTest());
            CurrentlyRenderedBy.CloseModal();
        }

        private void SizeChanged(Vector2Int newSize)
        {
            _componentContainer.Service<ComponentContainer>().Get<LevelConfig>().SetLevelSize(newSize, true);
        }
    }
}