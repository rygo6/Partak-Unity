using System;
using UnityEngine;
using UnityEngine.UI;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using GeoTetra.GTSnapper;
using GeoTetra.GTUI;
using GeoTetra.Partak;
using UnityEngine.AddressableAssets;

namespace Partak
{
    public class LevelCreateUI : StackUI
    {
        [SerializeField] private ServiceReference _gameState;
        [SerializeField] private ServiceReference _sceneLoadSystem;
        [SerializeField] private ServiceReference _componentContainer;
        [SerializeField] private AssetReference _mainMenuScene;
        [SerializeField] private AssetReference _newLevelScene;
        [SerializeField] private ItemCatalogUI _itemCatalogUI;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Button _changeSizeButton;
        [SerializeField] private Button _changeSpeedButton;

        private string[] ChangeSizeMessages;
        private Action[] ChangeSizeActions;
        
        protected override void Awake()
        {
            base.Awake();
            
            ChangeSizeMessages = new[] {"128x128", "192x192", "256x144", "256x256"};
            ChangeSizeActions = new Action[]
            {
                () => SizeChanged(new Vector2Int(128,128)), 
                () => SizeChanged(new Vector2Int(192,192)),
                () => SizeChanged(new Vector2Int(256,144)),
                () => SizeChanged(new Vector2Int(256,256))
            };
            
            _saveButton.onClick.AddListener(OnClickSave);
            _cancelButton.onClick.AddListener(OnClickCancel);
            _changeSizeButton.onClick.AddListener(OnClickChangeSize);
        }

        public override void OnTransitionInFinish()
        {
            base.OnTransitionInFinish();
            _itemCatalogUI.Initialize();
            
            _componentContainer.Service<ComponentContainer>().Get<LevelConfig>().Deserialize(_gameState.Service<GameState>().EditingLevelIndex);
        }

        public override void OnTransitionOutStart()
        {
            base.OnTransitionOutStart();
            _itemCatalogUI.Deinitialize();
        }

        private void OnClickSave()
        {
            _componentContainer.Service<ComponentContainer>().Get<LevelConfig>().Serialize(_gameState.Service<GameState>().EditingLevelIndex);

            OnBackClicked();
            _sceneLoadSystem.Service<SceneLoadSystem>().Load(_newLevelScene, _mainMenuScene);
        }

        private void OnClickCancel()
        {
            OnBackClicked();
            _sceneLoadSystem.Service<SceneLoadSystem>().Load(_newLevelScene, _mainMenuScene);
        }

        private void OnClickChangeSize()
        {
            DisplaySelectionModal(ChangeSizeMessages, ChangeSizeActions, 0);
        }

        private void SizeChanged(Vector2Int newSize)
        {
            _componentContainer.Service<ComponentContainer>().Get<LevelConfig>().SetLevelSize(newSize);
        }
    }
}