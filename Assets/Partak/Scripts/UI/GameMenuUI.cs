using System;
using System.Collections;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using GeoTetra.GTUI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace GeoTetra.Partak.UI
{
    public class GameMenuUI : StackUI
    {
        [SerializeField] private ServiceReference _componentContainer;
        [SerializeField] private ServiceReference _sceneLoadSystem;
        [SerializeField] private AnalyticsRelay _analyticsRelay;
        [SerializeField] private ServiceReference _gameState;
        [SerializeField] private Button[] _pauseButtons;
        [SerializeField] private AssetReference _mainMenuScene;
        [SerializeField] private AssetReference _gameSessionScene;
        [SerializeField] private InputPadGroup _inputPadGroup;
        [SerializeField] private DisableIn _disableIn;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _replayButton;
        [SerializeField] private Button _nextButton;
        
        private string[] _pauseMessages;
        private Action[] _pauseActions;
        
        protected override void Awake()
        {
            base.Awake();
            _pauseButtons[0].onClick.AddListener(ShowPauseMenu);
            _pauseButtons[1].onClick.AddListener(ShowPauseMenu);

            _pauseMessages = new[] {"Main Menu", "Skip Level", "Resume"};
            _pauseActions = new Action[] {MainMenu, Skip, Resume};
            
            _mainMenuButton.gameObject.SetActive(false);
            _replayButton.gameObject.SetActive(false);
            _nextButton.gameObject.SetActive(false);
            
            _mainMenuButton.onClick.AddListener(MainMenu);
            _replayButton.onClick.AddListener(Replay);
            _nextButton.onClick.AddListener(Next);
        }

        public override void OnTransitionInStart(UIRenderer uiRenderer)
        {
            base.OnTransitionInStart(uiRenderer);
            _inputPadGroup.Initialize();
            _componentContainer.Service<ComponentContainer>().Get<CellParticleStore>().WinEvent += ShowWinMenu;
            StartCoroutine(InitializeDelay());
        }

        private IEnumerator InitializeDelay()
        {
            yield return null;
            _disableIn.Initialize();
        }

        private void ShowPauseMenu()
        {
            DisplaySelectionModal("", _pauseMessages, _pauseActions, 0);
            _componentContainer.Service<ComponentContainer>().Get<CellParticleEngine>().Pause = true;
        }

        private void ShowWinMenu()
        {
            _mainMenuButton.gameObject.SetActive(true);
            _replayButton.gameObject.SetActive(true);
            _nextButton.gameObject.SetActive(true);
        }

        private void Resume()
        {
            _componentContainer.Service<ComponentContainer>().Get<CellParticleEngine>().Pause = false;
        }

        private void MainMenu()
        {
            CurrentlyRenderedBy.Flush(() =>
            {
                _sceneLoadSystem.Service<SceneLoadSystem>().Load(_gameSessionScene, _mainMenuScene);
            });
        }

        private void Replay()
        {
            _analyticsRelay.GamePlayerCount();
            CurrentlyRenderedBy.Flush(() =>
            {
                _sceneLoadSystem.Service<SceneLoadSystem>().Load(_gameSessionScene, _gameSessionScene);
            });
        }

        private void Skip()
        {
            _gameState.Service<GameState>().LevelIndex++;
            _analyticsRelay.GamePlayerCount();
            
            CurrentlyRenderedBy.Flush(() =>
            {
                _sceneLoadSystem.Service<SceneLoadSystem>().Load(_gameSessionScene, _gameSessionScene);
            });
        }

        private void Next()
        {
            _gameState.Service<GameState>().LevelIndex++;
            _analyticsRelay.GamePlayerCount();
            
            CurrentlyRenderedBy.Flush(() =>
            {
                _sceneLoadSystem.Service<SceneLoadSystem>().Load(_gameSessionScene, _gameSessionScene);
            });
        }
    }
}