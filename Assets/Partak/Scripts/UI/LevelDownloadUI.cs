using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using GeoTetra.GTBackend;
using GeoTetra.GTPooling;
using GeoTetra.GTUI;
using UnityEngine.AddressableAssets;

namespace GeoTetra.Partak
{
    public class LevelDownloadUI : StackUI
    {
        [SerializeField] private AssetReference _loadModalUI;
        [SerializeField] private ServiceReference _gameState;
        [SerializeField] private ServiceReference _databaseService;
        [SerializeField] private Button _mostPopularButton;
        [SerializeField] private Button _mostRecentButton;
        [SerializeField] private LevelButtonScrollRect _levelButtonScrollRect;
        
        private PartakDatabase _partakDatabase;
        private Search _search;
        
        protected override void Awake()
        {
            base.Awake();
            _partakDatabase = _databaseService.Service<PartakDatabase>();
            _mostPopularButton.onClick.AddListener(OnMostPopularClicked);
            _mostRecentButton.onClick.AddListener(OnMostRecentClicked);
            _levelButtonScrollRect.LevelButtonClicked += OnLevelButtonClicked;
        }


        public override void OnTransitionInStart(UIRenderer uiRenderer)
        {
            base.OnTransitionInStart(uiRenderer);
        }

        public override void OnTransitionInFinish()
        {
            base.OnTransitionInFinish();
            _search = _partakDatabase.QueryLevelsThumbsUp(_levelButtonScrollRect.ColumnCount);
            _levelButtonScrollRect.Initialize(DownloadNextSet, PopulateLevelButton, null);
        }

        public override void OnTransitionOutFinish()
        {
            base.OnTransitionOutFinish();
            _levelButtonScrollRect.Clear();
            _search = null;
        }

        private void OnMostPopularClicked()
        {
            _search = _partakDatabase.QueryLevelsThumbsUp(_levelButtonScrollRect.ColumnCount);
            _levelButtonScrollRect.Clear();
            _levelButtonScrollRect.Initialize(DownloadNextSet, PopulateLevelButton, null);
        }


        private void OnMostRecentClicked()
        {
            _search = _partakDatabase.QueryLevelsCreatedAt(_levelButtonScrollRect.ColumnCount);
            _levelButtonScrollRect.Clear();
            _levelButtonScrollRect.Initialize(DownloadNextSet, PopulateLevelButton, null);
        }

        private async Task PopulateLevelButton(LevelButton levelButton, CancellationToken cancellationToken)
        {
            await levelButton.DownloadAndDisplayLevelAsync(_partakDatabase, levelButton.LevelDatum.LevelID, cancellationToken);
            levelButton.ShowRating(true);
            levelButton.ThumbsUpText.text = levelButton.LevelDatum.ThumbsUp.ToString();
        }

        private async Task<bool> DownloadNextSet(List<List<LocalLevelDatum>> datumLists)
        {
            if (_search.IsDone) return true;

            List<Document> documentList = null;
            try
            {
                documentList = await _search.GetNextSetAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

            List<LocalLevelDatum> levelDatumList = new List<LocalLevelDatum>();
            for (int i = 0; i < documentList.Count; ++i)
            {
                LocalLevelDatum levelDatum = new LocalLevelDatum
                {
                    LevelID = documentList[i][PartakDatabase.LevelFields.IdKey].AsString(),
                    ThumbsUp = documentList[i][PartakDatabase.LevelFields.ThumbsUpKey].AsInt()
                };
                levelDatumList.Add(levelDatum);
            }

            datumLists.Add(levelDatumList);

            return _search.IsDone;
        }

        private async void OnLevelButtonClicked(LevelButton levelButton)
        {
            CurrentlyRenderedBy.InstantiateAndDisplayModalUI(_loadModalUI);
            await _partakDatabase.DownloadLevel(levelButton.LevelDatum.LevelID);
            _gameState.Service<GameState>().AddLevelId(levelButton.LevelDatum.LevelID);
            CurrentlyRenderedBy.CloseModal();
            OnBackClicked();
        }

        private void Cancel()
        {
            
        }
    }
}