using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using GeoTetra.GTCommon.Attributes;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using GeoTetra.GTSnapper;
using GeoTetra.GTUI;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Partak
{
    public class LevelCreateUI : StackUI
    {
        [SerializeField] private ServiceReference _gameState;
        [SerializeField] private ServiceReference _sceneLoadSystem;
        [SerializeField] private AssetReference _mainMenuScene;
        [SerializeField] private AssetReference _newLevelScene;
        [SerializeField] private ItemCatalogUI _itemCatalogUI;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Button _changeSizeButton;

        protected override void Awake()
        {
            base.Awake();
            _saveButton.onClick.AddListener(OnClickSave);
            _cancelButton.onClick.AddListener(OnClickCancel);
            _changeSizeButton.onClick.AddListener(OnClickChangeSize);
        }

        private void OnClickSave()
        {
            _itemCatalogUI.ItemRoot.Serialize(_gameState.Service<GameState>().EditingLevelPath());
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
            
        }
    }
}