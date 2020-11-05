using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using GeoTetra.GTCommon.Attributes;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using GeoTetra.GTUI;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace GeoTetra.Partak
{
    public class PlayUI : StackUI
    {
        [SerializeField] 
        [AssetReferenceComponentRestriction(typeof(SceneLoadSystem))]
        private SceneLoadSystemReference _loadSystem;
        
        [SerializeField]
        [AssetReferenceComponentRestriction(typeof(GameState))]
        private GameStateReference _gameState;
        
        [SerializeField] private AssetReference _gameSessionScene;
        [SerializeField] private AssetReference _mainMenuScene;
        [SerializeField] private Button _startButton;

        protected override void Awake()
        {
            base.Awake();
            _startButton.onClick.AddListener(OnStartClick);
        }

        private void OnStartClick()
        {
            int activeCount = 0;
            for (int i = 0; i < _gameState.Service.PlayerCount(); ++i)
            {
                if (_gameState.Service.PlayerStates[i].PlayerMode != PlayerMode.None)
                    activeCount++;
            }

            string levelId = _gameState.Service.GetSelectedLevelId();

            if (activeCount < 2)
            {
                CurrentlyRenderedBy.DisplayMessageModal("Enable at least two players.");
            }
            else if (string.IsNullOrEmpty(levelId))
            {
                CurrentlyRenderedBy.DisplayMessageModal("No Level Selected. Download or Create a new Level in the Level Editor UI.");
            }
            else
            {
                CurrentlyRenderedBy.Flush(Load);
            }
        }

        private void Load()
        {
            _loadSystem.Service.Load(_mainMenuScene, _gameSessionScene);
        }
    }
}