using System;
using System.Collections;
using GeoTetra.GTBackend;
using GeoTetra.GTPooling;
using GeoTetra.GTUI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace GeoTetra.Partak.UI
{
    public class GameMenuUI : StackUI
    {
        [SerializeField] 
        [AssetReferenceComponentRestriction(typeof(ComponentContainer))]
        private ComponentContainerReference _componentContainer;
        
        [SerializeField] 
        [AssetReferenceComponentRestriction(typeof(SceneLoadSystem))]
        private SceneLoadSystemReference _sceneLoadSystem;
        
        [SerializeField] 
        [AssetReferenceComponentRestriction(typeof(AnalyticsRelay))]
        private AnalyticsRelayReference _analyticsRelay;
        
        [SerializeField] 
        [AssetReferenceComponentRestriction(typeof(GameState))]
        private GameStateReference _gameState;
        
        [SerializeField] 
        [AssetReferenceComponentRestriction(typeof(PartakDatabase))]
        private PartakDatabaseReference _partakDatabase;
        
        [SerializeField] private AssetReference _mainMenuScene;
        [SerializeField] private AssetReference _gameSessionScene;
        [SerializeField] private InputPadGroup _inputPadGroup;
        [SerializeField] private DisableIn _disableIn;
        [SerializeField] private Button[] _pauseButtons;
        [SerializeField] private GameObject _winMenu;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _replayButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private GameObject _rateMenu;
        [SerializeField] private Button _thumbsUp;
        [SerializeField] private Button _thumbsDown;

        private string[] _pauseMessages;
        private Action[] _pauseActions;

        private LevelConfig _levelConfig;
        
        protected override void Awake()
        {
            base.Awake();
            _pauseButtons[0].onClick.AddListener(ShowPauseMenu);
            _pauseButtons[1].onClick.AddListener(ShowPauseMenu);

            _pauseMessages = new[] {"Main Menu", "Skip Level", "Resume"};
            _pauseActions = new Action[] {MainMenu, Skip, Resume};
            
            _winMenu.gameObject.SetActive(false);
            _rateMenu.gameObject.SetActive(false);
            // _mainMenuButton.gameObject.SetActive(false);
            // _replayButton.gameObject.SetActive(false);
            // _nextButton.gameObject.SetActive(false);
            
            _mainMenuButton.onClick.AddListener(MainMenu);
            _replayButton.onClick.AddListener(Replay);
            _nextButton.onClick.AddListener(Next);
            
            _thumbsUp.onClick.AddListener(ThumbsUp);
            _thumbsDown.onClick.AddListener(ThumbsDown);
        }

        private void Start()
        {

        }

        public override void OnTransitionInStart(UIRenderer uiRenderer)
        {
            base.OnTransitionInStart(uiRenderer);
            _levelConfig = _componentContainer.Service.Get<LevelConfig>();
            _inputPadGroup.Initialize();
            _componentContainer.Service.Get<CellParticleStore>().WinEvent += ShowWinMenu;
            StartCoroutine(InitializeDelay());
            _winMenu.gameObject.SetActive(false);
            _rateMenu.gameObject.SetActive(false);
        }
        
        public override void OnTransitionOutStart()
        {
            base.OnTransitionOutStart();
            _componentContainer.Service.Get<CellParticleStore>().WinEvent -= ShowWinMenu;
            _inputPadGroup.Deinitialize();
        }

        private IEnumerator InitializeDelay()
        {
            yield return null;
            //gotta wait one frame for them to be placed.
            _disableIn.Initialize();
        }

        private void ThumbsUp()
        {
            _partakDatabase.Service.IncrementThumbsUp(_levelConfig.Datum.LevelID, true);
            _levelConfig.Datum.Rated = true;
            _levelConfig.Serialize(_levelConfig.Datum.LevelID, false);
            _rateMenu.gameObject.SetActive(false);
        }

        private void ThumbsDown()
        {
            _partakDatabase.Service.IncrementThumbsDown(_levelConfig.Datum.LevelID, true);
            _levelConfig.Datum.Rated = true;
            _levelConfig.Serialize(_levelConfig.Datum.LevelID, false);
            _rateMenu.gameObject.SetActive(false);
        }
        
        private void ShowPauseMenu()
        {
            CurrentlyRenderedBy.DisplaySelectionModal("", _pauseMessages, _pauseActions, 0);
            _componentContainer.Service.Get<CellParticleEngine>().Pause = true;
        }

        private void ShowWinMenu()
        {
            _winMenu.gameObject.SetActive(true);
            if (!_levelConfig.Datum.Rated)
            {
                _rateMenu.gameObject.SetActive(true);
            }
        }

        private void Resume()
        {
            _componentContainer.Service.Get<CellParticleEngine>().Pause = false;
        }

        private void MainMenu()
        {
            CurrentlyRenderedBy.Flush(() =>
            {
                _sceneLoadSystem.Service.Load(_gameSessionScene, _mainMenuScene);
            }, UIRenderer.TransitionType.Fade);
        }

        private void Replay()
        {
            _analyticsRelay.Service.GamePlayerCount();
            CurrentlyRenderedBy.Flush(() =>
            {
                _sceneLoadSystem.Service.Load(_gameSessionScene, _gameSessionScene);
            }, UIRenderer.TransitionType.Fade);
        }

        private void Skip()
        {
            _gameState.Service.LevelIndex++;
            _analyticsRelay.Service.GamePlayerCount();
            
            CurrentlyRenderedBy.Flush(() =>
            {
                _sceneLoadSystem.Service.Load(_gameSessionScene, _gameSessionScene);
            }, UIRenderer.TransitionType.Fade);
        }

        private void Next()
        {
            _gameState.Service.LevelIndex++;
            _analyticsRelay.Service.GamePlayerCount();
            
            CurrentlyRenderedBy.Flush(() =>
            {
                _sceneLoadSystem.Service.Load(_gameSessionScene, _gameSessionScene);
            }, UIRenderer.TransitionType.Fade);
        }
    }
}