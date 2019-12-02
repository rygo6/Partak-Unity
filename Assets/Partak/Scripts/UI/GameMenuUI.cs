using System;
using System.Collections;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using GeoTetra.GTUI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace Partak.UI
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
        
        private string[] _pauseMessages;
        private Action[] _pauseActions;
        
        protected override void Awake()
        {
            base.Awake();
            _pauseButtons[0].onClick.AddListener(ShowPauseMenu);
            _pauseButtons[1].onClick.AddListener(ShowPauseMenu);

            _pauseMessages = new[] {"main menu", "resume", "skip level"};
            _pauseActions = new Action[] {MainMenu, Resume, Skip};
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
            DisplaySelectionModal(_pauseMessages, _pauseActions, 0);
            _componentContainer.Service<ComponentContainer>().Get<CellParticleEngine>().Pause = true;
        }

        private void ShowWinMenu()
        {
            Debug.Log("Win");
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
            string levelName = "Level" + (PlayerPrefs.GetInt("LevelIndex") + 1);
            Debug.Log("Replaying " + levelName);
            _analyticsRelay.ReplayLevel();

            CurrentlyRenderedBy.Flush(() =>
            {
                _sceneLoadSystem.Service<SceneLoadSystem>().Load(_gameSessionScene, _gameSessionScene);
            });
        }

        private void Skip()
        {
            _gameState.Service<GameState>().LevelIndex++;
            PlayerPrefs.SetInt("LevelIndex",
                (int) Mathf.Repeat(PlayerPrefs.GetInt("LevelIndex") + 1, 18)); //this is bad 
            string levelName = "Level" + (PlayerPrefs.GetInt("LevelIndex") + 1);
            Debug.Log("Skipping " + levelName);
            _analyticsRelay.SkipLevel();
            
            CurrentlyRenderedBy.Flush(() =>
            {
                _sceneLoadSystem.Service<SceneLoadSystem>().Load(_gameSessionScene, _gameSessionScene);
            });
        }

        private void Next()
        {
//            _componentContainer.Get<AdvertisementDispatch>().ShowAdvertisement(); //TODO HOOKUP
            PlayerPrefs.SetInt("LevelIndex",
                (int) Mathf.Repeat(PlayerPrefs.GetInt("LevelIndex") + 1, 18)); //this is bad 
            _analyticsRelay.NextLevel();
            
            CurrentlyRenderedBy.Flush(() =>
            {
                _sceneLoadSystem.Service<SceneLoadSystem>().Load(_gameSessionScene, _gameSessionScene);
            });
        }
    }
}