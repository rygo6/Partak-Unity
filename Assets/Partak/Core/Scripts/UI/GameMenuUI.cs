using System;
using System.Threading.Tasks;
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
        private ComponentContainerRef _componentContainer;
        
        [SerializeField]
        private SceneTransitRef _sceneTransit;
        
        [SerializeField]
        private AnalyticsServiceRef _analyticsService;
        
        [SerializeField]
        private PartakStateRef _partakState;
        
        [SerializeField]
        private PartakAWSRef _partakAWS;
        
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
        private CellParticleStore _cellParticleStore;
        
        protected override void Awake()
        {
            base.Awake();
            _pauseButtons[0].onClick.AddListener(ShowPauseMenu);
            _pauseButtons[1].onClick.AddListener(ShowPauseMenu);
            
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

        protected override async Task StartAsync()
        {
            await Task.WhenAll(_componentContainer.Cache(this),
                _partakState.Cache(this),
                _sceneTransit.Cache(this),
                _analyticsService.Cache(this),
                _componentContainer.Cache(this));
            
            _pauseMessages = new[] {"Main Menu", "Skip Level", "Resume"};
            _pauseActions = new Action[] {MainMenu, Skip, Resume};

            await base.StartAsync();
        }

        public override async void OnTransitionInStart(UIRenderer uiRenderer)
        {
            base.OnTransitionInStart(uiRenderer);
            
            _winMenu.gameObject.SetActive(false);
            _rateMenu.gameObject.SetActive(false);
            
            await Starting;
            _levelConfig = await _componentContainer.AwaitRegister<LevelConfig>() as LevelConfig;
            _cellParticleStore = await _componentContainer.AwaitRegister<CellParticleStore>() as CellParticleStore;
            
            _cellParticleStore.WinEvent += ShowWinMenu;
            
            _inputPadGroup.Initialize();
            await Task.Yield();
            await Task.Yield();
            _disableIn.Initialize();
        }
        
        public override void OnTransitionOutStart()
        {
            base.OnTransitionOutStart();
            _cellParticleStore.WinEvent -= ShowWinMenu;
            _inputPadGroup.Deinitialize();
        }

        private async void ThumbsUp()
        {
            await _partakAWS.Cache(this);
            await _partakAWS.Ref.IncrementThumbsUp(_levelConfig.Datum.LevelID);
            _levelConfig.Datum.Rated = true;
            _levelConfig.Serialize(_levelConfig.Datum.LevelID, false);
            _rateMenu.gameObject.SetActive(false);
        }

        private async void ThumbsDown()
        {
            await _partakAWS.Cache(this);
            await _partakAWS.Ref.IncrementThumbsDown(_levelConfig.Datum.LevelID);
            _levelConfig.Datum.Rated = true;
            _levelConfig.Serialize(_levelConfig.Datum.LevelID, false);
            _rateMenu.gameObject.SetActive(false);
        }
        
        private void ShowPauseMenu()
        {
            CurrentlyRenderedBy.DisplaySelectionModal("", _pauseMessages, _pauseActions, 0);
            _componentContainer.Ref.Get<CellParticleEngine>().Pause = true;
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
            _componentContainer.Ref.Get<CellParticleEngine>().Pause = false;
        }

        private void MainMenu()
        {
            CurrentlyRenderedBy.Flush(() =>
            {
                _sceneTransit.Ref.Load(_gameSessionScene, _mainMenuScene);
            }, UIRenderer.TransitionType.Fade);
        }

        private void Replay()
        {
            _analyticsService.Ref.GamePlayerCount();
            CurrentlyRenderedBy.Flush(() =>
            {
                _sceneTransit.Ref.Load(_gameSessionScene, _gameSessionScene);
            }, UIRenderer.TransitionType.Fade);
        }

        private void Skip()
        {
            _partakState.Ref.LevelIndex++;
            _analyticsService.Ref.GamePlayerCount();
            
            CurrentlyRenderedBy.Flush(() =>
            {
                _sceneTransit.Ref.Load(_gameSessionScene, _gameSessionScene);
            }, UIRenderer.TransitionType.Fade);
        }

        private void Next()
        {
            _partakState.Ref.LevelIndex++;
            _analyticsService.Ref.GamePlayerCount();
            
            CurrentlyRenderedBy.Flush(() =>
            {
                _sceneTransit.Ref.Load(_gameSessionScene, _gameSessionScene);
            }, UIRenderer.TransitionType.Fade);
        }
    }
}