using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using GeoTetra.GTCommon.Attributes;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTUI;
using UnityEngine.SceneManagement;

namespace Partak
{
    public class PlayUI : StackUI
    {
        [SerializeField] private ComponentContainer _componentContainer;
        [SerializeField] private GameState _gameState;
        [SerializeField] private Button _startButton;
        [ScenePath] [SerializeField] private string _unloadScene;
        [ScenePath] [SerializeField] private string _loadScene;

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
            _componentContainer.Get<SceneLoadSystem>().Load(_unloadScene, _loadScene);
        }
    }
}