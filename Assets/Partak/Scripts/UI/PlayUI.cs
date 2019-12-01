using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using GeoTetra.GTCommon.Attributes;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTUI;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Partak
{
    public class PlayUI : StackUI
    {
        [SerializeField] private SceneLoadSystem _sceneLoadSystem;
        [SerializeField] private GameState _gameState;
        [SerializeField] private Button _startButton;
        [SerializeField] private AssetReference _gameSessionScene;
        [SerializeField] private AssetReference _mainMenuScene;

        protected override void Awake()
        {
            base.Awake();
            _startButton.onClick.AddListener(OnStartClick);
        }

        private void OnStartClick()
        {
            int activeCount = 0;
            for (int i = 0; i < _gameState.PlayerCount(); ++i)
            {
                if (_gameState.PlayerStates[i].PlayerMode != PlayerMode.None)
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
            _sceneLoadSystem.Load(_mainMenuScene, _gameSessionScene);
        }
    }
}