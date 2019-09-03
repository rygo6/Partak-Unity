using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using GeoTetra.GTCommon.Variables;
using GeoTetra.GTUI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Partak
{
    public class PlayUI : StackUI
    {
        [SerializeField] private GameState _gameState;
        [SerializeField] private Button _startButton;

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
                StartCoroutine(LoadCoroutine());
            }
            else
            {
                DisplayModal("enable atleast two players");
            }
        }

        private IEnumerator LoadCoroutine()
        {
//            _componentContainer.Get<AnalyticsRelay>().MenuLevelLoad(); //TODO hook up
            //done so sound can play 
            yield return new WaitForSeconds(.5f);
//            _componentContainer.Get<AdvertisementDispatch>().ShowAdvertisement(); //TODO hook up
            string levelName = "Level" + (_gameState.LevelIndex + 1);
            SceneManager.LoadScene(levelName);
        }
    }
}