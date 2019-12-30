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

            if (activeCount >= 2)
            {
                CurrentlyRenderedBy.Flush(Load);
            }
            else
            {
                DisplayModal("enable atleast two players");
            }
        }

        private void Load()
        {
            _loadSystem.Service<SceneLoadSystem>().Load(_mainMenuScene, _gameSessionScene);
        }
    }
}