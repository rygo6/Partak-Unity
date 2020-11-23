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
        [SerializeField]
        private SceneTransitRef _sceneTransitRef;
        
        [SerializeField]
        private PartakStateRef _partakStateRef;
        
        [SerializeField] private AssetReference _gameSessionScene;
        [SerializeField] private AssetReference _mainMenuScene;
        [SerializeField] private Button _startButton;

        protected override async void Awake()
        {
            base.Awake();
            await _partakStateRef.Cache();
            _startButton.onClick.AddListener(OnStartClick);
        }

        private void OnStartClick()
        {
            int activeCount = 0;
            for (int i = 0; i < _partakStateRef.Service.PlayerCount(); ++i)
            {
                if (_partakStateRef.Service.PlayerStates[i].PlayerMode != PlayerMode.None)
                    activeCount++;
            }

            string levelId = _partakStateRef.Service.GetSelectedLevelId();

            if (activeCount < 2)
            {
                CurrentlyRenderedBy.DisplayMessageModal("Enable at least two players.");
            }
            else if (string.IsNullOrEmpty(levelId))
            {
                CurrentlyRenderedBy.DisplayMessageModal("No Level Selected. Download or Create a new Level in the Level Editor UI.");
            }
            else
            {
                CurrentlyRenderedBy.Flush(Load);
            }
        }

        private async void Load()
        {
            await _sceneTransitRef.Cache();
            _sceneTransitRef.Service.Load(_mainMenuScene, _gameSessionScene);
        }
    }
}