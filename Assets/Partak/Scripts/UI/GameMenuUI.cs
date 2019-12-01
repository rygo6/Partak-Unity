using System;
using System.Collections;
using GeoTetra.GTCommon.Attributes;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTUI;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Partak.UI
{
    public class GameMenuUI : StackUI
    {
        [SerializeField] private SceneLoadSystem _sceneLoadSystem;
        [SerializeField] private ComponentContainer _componentContainer;
        [SerializeField] private AnalyticsRelay _analyticsRelay;
        [SerializeField] private GameState _gameState;
        [SerializeField] private Button[] _pauseButtons;
        [SerializeField] private AssetReference _mainMenuScene;
        [SerializeField] private AssetReference _gameSessionScene;
        
        private string[] PauseMessages;
        private Action[] PauseActions;

        protected override void Awake()
        {
            _pauseButtons[0].onClick.AddListener(ShowPauseMenu);
            _pauseButtons[1].onClick.AddListener(ShowPauseMenu);

            PauseMessages = new[] {"main menu", "resume", "skip level"};
            PauseActions = new Action[] {MainMenu, Resume, Skip};
        }

        protected void Start()
        {
            _componentContainer.Get<CellParticleStore>().WinEvent += ShowWinMenu;
        }

        private void ShowPauseMenu()
        {
            DisplaySelectionModal(PauseMessages, PauseActions, 0);
            _componentContainer.Get<CellParticleEngine>().Pause = true;
        }

        private void ShowWinMenu()
        {
            Debug.Log("Win");
        }

        private void Resume()
        {
            _componentContainer.Get<CellParticleEngine>().Pause = false;
        }

        private void MainMenu()
        {
            CurrentlyRenderedBy.Flush(() =>
            {
                _sceneLoadSystem.Load(_gameSessionScene, _mainMenuScene);
            });
        }

        private void Replay()
        {
            string levelName = "Level" + (PlayerPrefs.GetInt("LevelIndex") + 1);
            Debug.Log("Replaying " + levelName);
            _analyticsRelay.ReplayLevel();

            CurrentlyRenderedBy.Flush(() =>
            {
                _sceneLoadSystem.Load(_gameSessionScene, _gameSessionScene);
            });
        }

        private void Skip()
        {
            _gameState.LevelIndex++;
            PlayerPrefs.SetInt("LevelIndex",
                (int) Mathf.Repeat(PlayerPrefs.GetInt("LevelIndex") + 1, 18)); //this is bad 
            string levelName = "Level" + (PlayerPrefs.GetInt("LevelIndex") + 1);
            Debug.Log("Skipping " + levelName);
            _analyticsRelay.SkipLevel();
            
            CurrentlyRenderedBy.Flush(() =>
            {
                _sceneLoadSystem.Load(_gameSessionScene, _gameSessionScene);
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
                _sceneLoadSystem.Load(_gameSessionScene, _gameSessionScene);
            });
        }
    }
}