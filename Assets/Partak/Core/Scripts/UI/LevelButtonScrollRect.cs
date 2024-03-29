﻿using System;
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
    public class LevelButtonScrollRect : MonoBehaviour
    {
        [SerializeField] private List<LevelButtonRow> _levelButtonRows;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private int _rowSpacing = 10;
        [SerializeField]  private int _columnCount = 3;
        
        public event Action<LevelButton> LevelButtonClicked;
        private readonly List<List<LocalLevelDatum>> _datumLists = new List<List<LocalLevelDatum>>();
        private LevelButton _selectedLevelButton;
        private int _topRowIndex;
        private Rect _itemRect;
        private CancellationTokenSource _layoutCancelToken;
        private bool _populatingNextSet;
        private bool _isDonePopulating;

        private Func<List<List<LocalLevelDatum>>, CancellationToken, Task<bool>> _populateNextSet;
        private Func<LevelButton, CancellationToken, Task> _populateLevelButton;
        private Action<LevelButton> _finalButton;

        public int ColumnCount => _columnCount;

        protected void Awake()
        {
            _scrollRect.onValueChanged.AddListener(OnScrolled);
            for (int r = 0; r < _levelButtonRows.Count; ++r)
            {
                for (int c = 0; c < _columnCount; ++c)
                {
                    _levelButtonRows[r].LevelButtons[c].ButtonClicked += OnLevelButtonClicked;
                }
            }
        }

        private void Start()
        {
            _itemRect = _levelButtonRows[0].LevelButtons[0].RectTransform.rect;
            Clear();
        }

        /// <summary>
        /// Initialize List
        /// </summary>
        /// <param name="populateNextSet"> Method that returns true once no more items available. </param>
        /// <param name="populateLevelButton"> Method that will populate LevelButton visually. </param>
        /// <param name="finalButton"> An additional optional button to show after all others. </param>
        public void Initialize(
            Func<List<List<LocalLevelDatum>>, CancellationToken, Task<bool>> populateNextSet, 
            Func<LevelButton, CancellationToken, Task> populateLevelButton,
            Action<LevelButton> finalButton)
        {
            _finalButton = finalButton;
            _populateLevelButton = populateLevelButton;
            _populateNextSet = populateNextSet;
            _isDonePopulating = false;
            _populatingNextSet = false;
            LayoutUI();
        }

        public void Clear()
        {
            _layoutCancelToken?.Cancel();
            _layoutCancelToken?.Dispose();
            _layoutCancelToken = null;

            _topRowIndex = 0;
            _isDonePopulating = false;
            _populatingNextSet = false;

            Vector3 newPos = _scrollRect.content.anchoredPosition3D;
            newPos.y = 0;
            _scrollRect.content.anchoredPosition3D = newPos;
            _scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
            
            //Todo recycle nested lists?
            _datumLists.Clear();
            for (int r = 0; r < _levelButtonRows.Count; ++r)
            {
                Vector3 rowPos = _levelButtonRows[r].RectTransform.anchoredPosition3D;
                rowPos.y = -r * (_itemRect.height + _rowSpacing);
                _levelButtonRows[r].RectTransform.anchoredPosition3D = rowPos;
                for (int c = 0; c < _columnCount; ++c)
                {
                    _levelButtonRows[r].LevelButtons[c].SetEmpty();
                }
            }
        }

        public async void LayoutUI()
        {
            _layoutCancelToken?.Cancel();
            _layoutCancelToken?.Dispose();
            _layoutCancelToken = new CancellationTokenSource();
            CancellationToken cancellationToken = _layoutCancelToken.Token;
            
            int documentRowIndex = _topRowIndex;
            for (int r = 0; r < _levelButtonRows.Count; ++r)
            {
                for (int c = 0; c < _columnCount; ++c)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (_levelButtonRows[r].LevelButtons[c].IsIndex(documentRowIndex, c) && _levelButtonRows[r].LevelButtons[c].LevelDatum != null) continue;
                    
                    if (documentRowIndex >= 0 && documentRowIndex < _datumLists.Count && c < _datumLists[documentRowIndex].Count )
                    {
                        _levelButtonRows[r].LevelButtons[c].LevelDatum = _datumLists[documentRowIndex][c];
                        await _populateLevelButton(_levelButtonRows[r].LevelButtons[c], cancellationToken);
                        cancellationToken.ThrowIfCancellationRequested();
                        _levelButtonRows[r].LevelButtons[c].Index0 = documentRowIndex;
                        _levelButtonRows[r].LevelButtons[c].Index1 = c;
                    }
                    else
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (_finalButton != null && (documentRowIndex * ColumnCount) + c == TotalDatumCount())
                        {
                            _levelButtonRows[r].LevelButtons[c].Index0 = documentRowIndex;
                            _levelButtonRows[r].LevelButtons[c].Index1 = c;
                            _finalButton(_levelButtonRows[r].LevelButtons[c]);
                        }
                        else
                        {
                            _levelButtonRows[r].LevelButtons[c].SetEmpty();
                        }
                    }
                }

                documentRowIndex++;
            }
            
            if (documentRowIndex >= _datumLists.Count) PopulateNextSet(cancellationToken);
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

        private async void PopulateNextSet(CancellationToken cancellationToken)
        {
            if (_populatingNextSet || _isDonePopulating) return;
            _populatingNextSet = true;
            
            _isDonePopulating = await _populateNextSet(_datumLists, cancellationToken);
            // if (cancellationToken.IsCancellationRequested) return;

            float verticalSize = (_datumLists.Count + (_isDonePopulating ? 0 : 1) + (_finalButton == null ? 0 : 1)) * (_itemRect.height + _rowSpacing);
            _scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, verticalSize);
            
            _populatingNextSet = false;
            
            LayoutUI();
        }

        private void OnLevelButtonClicked(LevelButton levelButton)
        {
            LevelButtonClicked?.Invoke(levelButton);
        }

        private int TotalDatumCount()
        {
            int count = (_datumLists.Count > 0 ? (_datumLists.Count - 1) * ColumnCount : 0) +
                   (_datumLists.Count > 0 ? _datumLists[_datumLists.Count - 1].Count : 0);
            return count;
        }
    }
}