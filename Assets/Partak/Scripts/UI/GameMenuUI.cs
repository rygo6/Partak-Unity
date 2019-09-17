using System;
using System.Collections;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTUI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Partak.UI
{
    public class GameMenuUI : StackUI
    {
        [SerializeField] private ComponentContainer _componentContainer;
        [SerializeField] private AnalyticsRelay _analyticsRelay;
        [SerializeField] private GameState _gameState;
        [SerializeField] public Button[] _pauseButtons;

        private string[] PauseMessages;
        private Action[] PauseActions;
        
        protected override void Awake()
        {
            _pauseButtons[0].onClick.AddListener(ShowPauseMenu);
            _pauseButtons[1].onClick.AddListener(ShowPauseMenu);

            PauseMessages = new [] {"main menu", "resume", "skip level"};
            PauseActions = new Action[] {MainMenu, Resume, Skip};
            
            FindObjectOfType<CellParticleStore>().WinEvent += ShowWinMenu;
        }

        private void ShowPauseMenu()
        {
            DisplaySelectionModal(PauseMessages, PauseActions, 0);
            _componentContainer.Get<CellParticleEngine>().Pause = true;
        }

        public void ShowWinMenu()
        {
            GetComponent<Animator>().Play("SlideIn");
        }

        private void Resume()
        {
            _componentContainer.Get<CellParticleEngine>().Pause = false;
        }

        private void MainMenu()
        {
            StartCoroutine(LoadCoroutine("OpenMenu"));
        }

        private void Replay()
        {
            string levelName = "Level" + (PlayerPrefs.GetInt("LevelIndex") + 1);
            Debug.Log("Replaying " + levelName);
            _analyticsRelay.ReplayLevel();
            StartCoroutine(LoadCoroutine("Level" + (PlayerPrefs.GetInt("LevelIndex") + 1)));
        }

        private void Skip()
        {
            _gameState.LevelIndex++;
            PlayerPrefs.SetInt("LevelIndex",
                (int) Mathf.Repeat(PlayerPrefs.GetInt("LevelIndex") + 1, 18)); //this is bad 
            string levelName = "Level" + (PlayerPrefs.GetInt("LevelIndex") + 1);
            Debug.Log("Skipping " + levelName);
            _analyticsRelay.SkipLevel();
            StartCoroutine(LoadCoroutine(levelName));
        }

        private void Next()
        {
//            _componentContainer.Get<AdvertisementDispatch>().ShowAdvertisement(); //TODO HOOKUP
            PlayerPrefs.SetInt("LevelIndex",
                (int) Mathf.Repeat(PlayerPrefs.GetInt("LevelIndex") + 1, 18)); //this is bad 
            _analyticsRelay.NextLevel(); 
            StartCoroutine(LoadCoroutine("Level" + (PlayerPrefs.GetInt("LevelIndex") + 1)));
        }

        private IEnumerator LoadCoroutine(string levelName)
        {
            //done so sound can play
            yield return new WaitForSeconds(.5f);
            SceneManager.LoadScene(levelName);
        }
    }
}