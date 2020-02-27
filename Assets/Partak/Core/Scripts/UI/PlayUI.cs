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
        [SerializeField] private ServiceReference _loadSystem;
        [SerializeField] private ServiceReference _gameState;
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
            for (int i = 0; i < _gameState.Service<GameState>().PlayerCount(); ++i)
            {
                if (_gameState.Service<GameState>().PlayerStates[i].PlayerMode != PlayerMode.None)
                    activeCount++;
            }

            string levelId = _gameState.Service<GameState>().GetSelectedLevelId();

            if (activeCount < 2)
            {
                DisplayModal("Enable at least two players.");
            }
            else if (string.IsNullOrEmpty(levelId))
            {
                DisplayModal("No Level Selected. Download or Create a new Level in the Level Editor UI.");
            }
            else
            {
                CurrentlyRenderedBy.Flush(Load);
            }
        }

        private void Load()
        {
            _loadSystem.Service<SceneLoadSystem>().Load(_mainMenuScene, _gameSessionScene);
        }
    }
}