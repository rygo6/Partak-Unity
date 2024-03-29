﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using GeoTetra.GTBackend;
using GeoTetra.GTUI;
using UnityEngine.AddressableAssets;

namespace GeoTetra.Partak
{
    public class LevelDownloadUI : StackUI
    {
        [SerializeField]
        private PartakAWSRef _partakAWS;
        
        [SerializeField]
        private PartakStateRef _partakState;
        
        [SerializeField]
        private AdServiceRef _adService;
        
        [SerializeField]
        private AnalyticsServiceRef _analyticsService;

        [SerializeField] private AssetReference _fullPurchaseUI;
        [SerializeField] private Button _mostPopularButton;
        [SerializeField] private Button _mostRecentButton;
        [SerializeField] private LevelButtonScrollRect _levelButtonScrollRect;

        private const int maxCache = 100;

        private Search _search;
        private OrderedDictionary _textureCache = new OrderedDictionary();
        private LevelButton _selectedLevelButton;

        private const string FullVersionMessage ="Ad will play while level downloads.";
        private string[] _fullVersionDialogLabels;
        private Action[] _fullVersionDialogActions;

        protected override void Awake()
        {
            base.Awake();
            _mostPopularButton.onClick.AddListener(OnMostPopularClicked);
            _mostRecentButton.onClick.AddListener(OnMostRecentClicked);
            _levelButtonScrollRect.LevelButtonClicked += OnLevelButtonClicked;
            
            _fullVersionDialogLabels = new[] {"Disable All Ads", "Download Level and Watch Ad", "Cancel"};
            _fullVersionDialogActions = new Action[] {PurchaseFullVersion, PlayAd, Cancel};
        }
        
        protected override async Task StartAsync()
        {
            await Task.WhenAll(
                _partakState.Cache(this),
                _partakAWS.Cache(this),
                _analyticsService.Cache(this)
            );
            await base.StartAsync();
        }

        public override async void OnTransitionInFinish()
        {
            base.OnTransitionInFinish();
            await Starting;
            _search = await _partakAWS.Ref.QueryLevelsThumbsUp(_levelButtonScrollRect.ColumnCount);
            _levelButtonScrollRect.Initialize(DownloadNextSet, PopulateLevelButton, null);
        }

        public override void OnTransitionOutStart()
        {
            base.OnTransitionOutStart();
        }

        public override void OnTransitionOutFinish()
        {
            base.OnTransitionOutFinish();
            _textureCache.Clear();
            _levelButtonScrollRect.Clear();
            _search = null;
            Resources.UnloadUnusedAssets();
        }

        private async void OnMostPopularClicked()
        {
            _search = await _partakAWS.Ref.QueryLevelsThumbsUp(_levelButtonScrollRect.ColumnCount);
            _levelButtonScrollRect.Clear();
            _levelButtonScrollRect.Initialize(DownloadNextSet, PopulateLevelButton, null);
        }


        private async void OnMostRecentClicked()
        {
            _search = await _partakAWS.Ref.QueryLevelsCreatedAt(_levelButtonScrollRect.ColumnCount);
            _levelButtonScrollRect.Clear();
            _levelButtonScrollRect.Initialize(DownloadNextSet, PopulateLevelButton, null);
        }

        private async Task PopulateLevelButton(LevelButton levelButton, CancellationToken ct)
        {
            levelButton.Image.color = Color.clear;
            if (_textureCache.Contains(levelButton.LevelDatum.LevelID))
            {
                levelButton.Image.texture = (Texture2D)_textureCache[levelButton.LevelDatum.LevelID];
                levelButton.SizeImageFromRatio();
            }
            else
            {
                Texture2D texture2D = new Texture2D(0,0, TextureFormat.RGBA32, 0, false);
                _textureCache.Add(levelButton.LevelDatum.LevelID, texture2D);
                if (_textureCache.Count > maxCache) _textureCache.RemoveAt(0);
                
                //Store datum before downloading and ensure it is the same afterwards in case the user scrolled fast
                // enough as to make the download operation irrelevant.
                LocalLevelDatum datum = levelButton.LevelDatum;
                
                //Don't cancel downloads so they are available on scroll back.
                await _partakAWS.Ref.DownloadLevelPreview(levelButton.LevelDatum.LevelID, texture2D);
                
                if (datum != levelButton.LevelDatum) return;
                
                levelButton.Image.texture = texture2D;
                levelButton.SizeImageFromRatio();
            }
            
            if (_partakState.Ref.LevelCatalogDatum.LevelIDs.Contains(levelButton.LevelDatum.LevelID))
            {
                levelButton.Text.text = "Downloaded";
                levelButton.Image.color = Color.gray;
                levelButton.Button.interactable = false;
            }
            else
            {
                levelButton.Text.text = "";
                levelButton.Image.color = Color.white;
                levelButton.Button.interactable = true;
            }
            levelButton.ShowRating(true);
            levelButton.ThumbsDownText.text = levelButton.LevelDatum.ThumbsDown.ToString();
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

            
            // cancellationToken.ThrowIfCancellationRequested(); // You do NOT want to abort if the cancel token fires because the results need to get added to list!

            List<LocalLevelDatum> levelDatumList = new List<LocalLevelDatum>();
            for (int i = 0; i < documentList.Count; ++i)
            {
                documentList[i].TryGetValue(PartakAWS.LevelFields.IdKey, out DynamoDBEntry levelId);
                documentList[i].TryGetValue(PartakAWS.LevelFields.ThumbsUpKey, out DynamoDBEntry thumbsUp);
                documentList[i].TryGetValue(PartakAWS.LevelFields.ThumbsDownKey, out DynamoDBEntry thumbsDown);
                
                LocalLevelDatum levelDatum = new LocalLevelDatum
                {
                    LevelID = levelId ?? string.Empty,
                    ThumbsUp = thumbsUp?.AsInt() ?? 0,
                    ThumbsDown = thumbsDown?.AsInt() ?? 0
                };

                _partakAWS.Ref.UpdateAggregate(levelId, thumbsUp.AsInt() - thumbsDown.AsInt());
                
                levelDatumList.Add(levelDatum);
            }

            datumLists.Add(levelDatumList);

            return _search.IsDone;
        }

        private void OnLevelButtonClicked(LevelButton levelButton)
        {
            _selectedLevelButton = levelButton;

            if (_partakState.Ref.FullVersion)
            {
                CurrentlyRenderedBy.DisplayLoadModal("Downloading Level...");
                DownloadLevel();
            }
            else
            {
                CurrentlyRenderedBy.DisplaySelectionModal(null, 
                    _fullVersionDialogLabels, 
                    _fullVersionDialogActions, 
                    0);
            }
        }

        private async void PurchaseFullVersion()
        {
            await CurrentlyRenderedBy.InstantiateAndDisplayModalUI(_fullPurchaseUI);
        }
        
        private async void PlayAd()
        {
            await CurrentlyRenderedBy.DisplayLoadModal("Playing Ad...");
            await Starting;
            await _adService.Cache(this);
            await _adService.Ref.ShowRewardedAd();
            DownloadLevel();
        }

        private async void DownloadLevel()
        {
            await _partakAWS.Ref.DownloadLevel(_selectedLevelButton.LevelDatum.LevelID);
            
            //Reset level index so play menu doesn't load on empty level.
            _partakState.Ref.LevelIndex = 0;
            
            _partakState.Ref.AddLevelId(_selectedLevelButton.LevelDatum.LevelID);
            CurrentlyRenderedBy.CloseModal();
            OnBackClicked();
            _analyticsService.Ref.LevelDownloaded();
        }

        private void Cancel()
        {
            
        }
    }
}