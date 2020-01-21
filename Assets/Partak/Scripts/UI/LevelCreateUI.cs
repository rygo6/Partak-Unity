using System;
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
        [SerializeField] private ItemCatalogUI _itemCatalogUI;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _cancelButton;
        
        private string[] _changeSizeMessages;
        private Action[] _changeSizeActions;
        
        private string[] _saveMessages;
        private Action[] _saveActions;

        private LevelConfig _levelConfig;
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
            _cancelButton.onClick.AddListener(CloseLevelMaker);
        }

        public override void OnTransitionInFinish()
        {
            base.OnTransitionInFinish();
            _levelConfig = _componentContainer.Service<ComponentContainer>().Get<LevelConfig>();
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
            _levelConfig.Datum.Shared = false;
            if (_levelConfig.Datum.Shared)
            {
                SerializeLevel();
                CloseLevelMaker();
            }
            else
            {
                DisplaySelectionModal("Share Level Online?", _saveMessages, _saveActions, 0);
            }
        }

        private void CloseLevelMaker()
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
            int levelIndex = _gameState.Service<GameState>().EditingLevelIndex;
            _levelConfig.Datum.Shared = true;
            SerializeLevel();
            await _database.Service<PartakDatabase>().SaveLevel(_editingLevelId);
            
            CloseLevelMaker();
        }

        private void OnClickChangeSize()
        {

        }

        private void SizeChanged(Vector2Int newSize)
        {
            _componentContainer.Service<ComponentContainer>().Get<LevelConfig>().SetLevelSize(newSize, true);
        }
    }
}