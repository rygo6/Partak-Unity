using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using GeoTetra.GTBackend;
using GeoTetra.GTCommon.Attributes;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using GeoTetra.GTUI;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace GeoTetra.Partak
{
    public class LevelDownloadUI : StackUI
    {
        [SerializeField] private AssetReference _loadModalUI;
        [SerializeField] private ServiceReference _gameState;
        [SerializeField] private ServiceReference _databaseService;
        [SerializeField] private List<LevelButtonRow> _levelButtonRows;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private int _rowSpacing = 10;
        [SerializeField] private Button _mostPopularButton;
        [SerializeField] private Button _mostRecentButton;
        
        private const int ColumnCount = 3;
        private readonly List<List<Document>> _documentLists = new List<List<Document>>();
        private LevelButton _selectedLevelButton;
        private PartakDatabase _partakDatabase;
        private Search _search;
        private int _topRowIndex;
        private Rect _itemRect;
        private bool _downloading;
        private CancellationTokenSource _layoutCancelToken;
        
        protected override void Awake()
        {
            base.Awake();
            _partakDatabase = _databaseService.Service<PartakDatabase>();
            _scrollRect.onValueChanged.AddListener(OnScrolled);
            _mostPopularButton.onClick.AddListener(OnMostPopularClicked);
            _mostRecentButton.onClick.AddListener(OnMostRecentClicked);
            for (int r = 0; r < _levelButtonRows.Count; ++r)
            {
                for (int c = 0; c < ColumnCount; ++c)
                {
                    _levelButtonRows[r].LevelButtons[c].ButtonClicked += OnLevelButtonClicked;
                }
            }
        }

        private void Start()
        {
            _itemRect = _levelButtonRows[0].LevelButtons[0].Image.rectTransform.rect;
        }

        public override void OnTransitionInStart(UIRenderer uiRenderer)
        {
            base.OnTransitionInStart(uiRenderer);
        }

        public override void OnTransitionInFinish()
        {
            base.OnTransitionInFinish();
            _search = _partakDatabase.QueryLevelsThumbsUp(ColumnCount);
            LayoutUI();
        }

        public override void OnTransitionOutFinish()
        {
            base.OnTransitionOutFinish();
            Clear();
        }

        private void Clear()
        {
            _topRowIndex = 0;

            Vector3 newPos = _scrollRect.content.anchoredPosition3D;
            newPos.y = 0;
            _scrollRect.content.anchoredPosition3D = newPos;
            _scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
            
            //Todo recycle nested lists?
            _documentLists.Clear();
            for (int r = 0; r < _levelButtonRows.Count; ++r)
            {
                Vector3 rowPos = _levelButtonRows[r].RectTransform.anchoredPosition3D;
                rowPos.y = -r * (_itemRect.height + _rowSpacing);
                _levelButtonRows[r].RectTransform.anchoredPosition3D = rowPos;
                for (int c = 0; c < ColumnCount; ++c)
                {
                    _levelButtonRows[r].LevelButtons[c].SetEmpty();
                }
            }
        }

        private void OnMostPopularClicked()
        {
            _search = _partakDatabase.QueryLevelsThumbsUp(ColumnCount);
            Clear();
            LayoutUI();
        }

        private void OnMostRecentClicked()
        {
            _search = _partakDatabase.QueryLevelsCreatedAt(ColumnCount);
            Clear();
            LayoutUI();
        }
        
        private async void LayoutUI()
        {
            if (_layoutCancelToken != null)
            {
                if (!_layoutCancelToken.IsCancellationRequested) _layoutCancelToken.Cancel();
                _layoutCancelToken.Dispose();
            } 
            _layoutCancelToken = new CancellationTokenSource();
            CancellationToken cancellationToken = _layoutCancelToken.Token;
            
            int documentRowIndex = _topRowIndex;
            bool nextSetShouldDownload = false;
            for (int r = 0; r < _levelButtonRows.Count; ++r)
            {
                for (int c = 0; c < ColumnCount; ++c)
                {
                    if (cancellationToken.IsCancellationRequested) return;
                    if (_levelButtonRows[r].LevelButtons[c].IsIndex(r, c)) continue;
                    if (documentRowIndex >= 0 && documentRowIndex < _documentLists.Count && c < _documentLists[documentRowIndex].Count )
                    {
                        _levelButtonRows[r].LevelButtons[c].Index0 = r;
                        _levelButtonRows[r].LevelButtons[c].Index1 = c;
                        await _levelButtonRows[r].LevelButtons[c].DownloadAndDisplayLevelAsync(_partakDatabase, _documentLists[documentRowIndex][c], cancellationToken);
                        _levelButtonRows[r].LevelButtons[c].Text.text = "";
                        _levelButtonRows[r].LevelButtons[c].ShowRating(true);
                        _levelButtonRows[r].LevelButtons[c].ThumbsUpText.text =_documentLists[documentRowIndex][c][PartakDatabase.LevelFields.ThumbsUpKey];
                        _levelButtonRows[r].LevelButtons[c].ThumbsDownText.text =_documentLists[documentRowIndex][c][PartakDatabase.LevelFields.ThumbsDownKey];
                        if (cancellationToken.IsCancellationRequested) return;
                    }
                    else
                    {
                        nextSetShouldDownload = true;
                        _levelButtonRows[r].LevelButtons[c].SetEmpty();
                    }
                }

                documentRowIndex++;
            }
            
            if (nextSetShouldDownload) DownloadNextSet();
        }

        private void OnScrolled(Vector2 delta)
        {
            float previousPosition = _levelButtonRows[0].RectTransform.anchoredPosition.y + _rowSpacing;
            float nextPosition =_levelButtonRows[_levelButtonRows.Count - 1].RectTransform.anchoredPosition3D.y - _itemRect.height - _rowSpacing;
            
            if (previousPosition < -_scrollRect.content.anchoredPosition3D.y && previousPosition < 0)
            {
                _topRowIndex--;
                LevelButtonRow levelButtonRow = _levelButtonRows[_levelButtonRows.Count - 1];
                levelButtonRow.SetEmpty();
                _levelButtonRows.RemoveAt(_levelButtonRows.Count - 1);
                Vector3 newPos = levelButtonRow.RectTransform.anchoredPosition3D;
                newPos.y = previousPosition + _itemRect.height;
                levelButtonRow.RectTransform.anchoredPosition3D = newPos;
                _levelButtonRows.Insert(0, levelButtonRow);
                LayoutUI();
            }
            else if (nextPosition + _scrollRect.content.anchoredPosition3D.y > -_scrollRect.viewport.rect.height && nextPosition > -_scrollRect.content.rect.height)
            {
                _topRowIndex++;
                LevelButtonRow levelButtonRow = _levelButtonRows[0];
                levelButtonRow.SetEmpty();
                _levelButtonRows.RemoveAt(0);
                Vector3 newPos = levelButtonRow.RectTransform.anchoredPosition3D;
                newPos.y = nextPosition;
                levelButtonRow.RectTransform.anchoredPosition3D = newPos;
                _levelButtonRows.Add(levelButtonRow);
                LayoutUI();
            }
        }

        private async void DownloadNextSet()
        {
            if (_downloading || _search.IsDone) return;

            _downloading = true;

            List<Document> documentList = null;
            try
            {
                documentList = await _search.GetNextSetAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

            _documentLists.Add(documentList);
            
            float verticalSize = (_documentLists.Count + (_search.IsDone ? 0 : 1)) * (_itemRect.height + _rowSpacing);
            _scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, verticalSize);
            
            _downloading = false;
            
            LayoutUI();
        }

        private async void OnLevelButtonClicked(LevelButton levelButton)
        {
            Document document = _documentLists[levelButton.Index0][levelButton.Index1];
            CurrentlyRenderedBy.InstantiateAndDisplayModalUI(_loadModalUI);
            await _partakDatabase.DownloadLevel(document, _gameState.Service<GameState>().EditingLevelIndex);
            CurrentlyRenderedBy.CloseModal();
            OnBackClicked();
        }

        private void Cancel()
        {
            
        }
    }
}