using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using GeoTetra.GTBackend;
using GeoTetra.GTPooling;
using GeoTetra.GTUI;
using UnityEditor.iOS;
using UnityEngine.AddressableAssets;
using Debug = UnityEngine.Debug;

namespace GeoTetra.Partak
{
    public class LevelDownloadUI : StackUI
    {
        [SerializeField] private AssetReference _loadModalUI;
        [SerializeField] private GameStateReference _gameState;
        [SerializeField] private ServiceReference _databaseService;
        [SerializeField] private Button _mostPopularButton;
        [SerializeField] private Button _mostRecentButton;
        [SerializeField] private LevelButtonScrollRect _levelButtonScrollRect;

        private const int maxCache = 100;
        private PartakDatabase _partakDatabase;
        private Search _search;
        private OrderedDictionary _textureCache = new OrderedDictionary();

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
            _textureCache.Clear();
            _levelButtonScrollRect.Clear();
            _search = null;
            Resources.UnloadUnusedAssets();
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
            levelButton.Image.color = Color.clear;
            if (_textureCache.Contains(levelButton.LevelDatum.LevelID))
            {
                levelButton.Image.texture = (Texture2D)_textureCache[levelButton.LevelDatum.LevelID];
            }
            else
            {
                Texture2D texture2D = new Texture2D(0,0, TextureFormat.RGBA32, 0, false);
                _textureCache.Add(levelButton.LevelDatum.LevelID, texture2D);
                if (_textureCache.Count > maxCache) _textureCache.RemoveAt(0);
                LocalLevelDatum datum = levelButton.LevelDatum;
                await _partakDatabase.DownloadLevelPreview(levelButton.LevelDatum.LevelID, texture2D);
                if (datum != levelButton.LevelDatum) return;
                levelButton.Image.texture = texture2D;
            }

            if (_gameState.Service.LevelCatalogDatum.LevelIDs.Contains(levelButton.LevelDatum.LevelID))
            {
                levelButton.Text.text = "Downloaded";
                levelButton.Image.color = Color.red;
                levelButton.Button.interactable = false;
            }
            else
            {
                levelButton.Text.text = "";
                levelButton.Image.color = Color.green;
                levelButton.Button.interactable = true;
            }
            levelButton.ShowRating(true);
            levelButton.ThumbsUpText.text = levelButton.LevelDatum.ThumbsUp.ToString();
        }

        private IEnumerator SetImage(LevelButton levelButton, Texture2D texture2D)
        {
            yield return null;
            levelButton.Image.texture = texture2D;
        }

        private async Task<bool> DownloadNextSet(List<List<LocalLevelDatum>> datumLists, CancellationToken cancellationToken)
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

            if (cancellationToken.IsCancellationRequested) return false;

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
            _gameState.Service.AddLevelId(levelButton.LevelDatum.LevelID);
            CurrentlyRenderedBy.CloseModal();
            OnBackClicked();
        }

        private void Cancel()
        {
            
        }
    }
}