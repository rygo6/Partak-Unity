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

        private Func<List<List<LocalLevelDatum>>, Task<bool>> _populateNextSet;
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
            _itemRect = _levelButtonRows[0].LevelButtons[0].Image.rectTransform.rect;
            Clear();
        }

        public void Initialize(
            Func<List<List<LocalLevelDatum>>, Task<bool>> populateNextSet, 
            Func<LevelButton, CancellationToken, Task> populateLevelButton,
            Action<LevelButton> finalButton)
        {
            _finalButton = finalButton;
            _populateLevelButton = populateLevelButton;
            _populateNextSet = populateNextSet;
            _isDonePopulating = false;
            LayoutUI();
        }

        public void Clear()
        {
            _topRowIndex = 0;
            _isDonePopulating = false;

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
                    _levelButtonRows[r].LevelButtons[c].SetEmpty(true);
                }
            }
        }

        public async void LayoutUI()
        {
            if (_layoutCancelToken != null)
            {
                if (!_layoutCancelToken.IsCancellationRequested) _layoutCancelToken.Cancel();
                _layoutCancelToken.Dispose();
            } 
            _layoutCancelToken = new CancellationTokenSource();
            CancellationToken cancellationToken = _layoutCancelToken.Token;
            
            int documentRowIndex = _topRowIndex;
            for (int r = 0; r < _levelButtonRows.Count; ++r)
            {
                for (int c = 0; c < _columnCount; ++c)
                {
                    if (cancellationToken.IsCancellationRequested) return;
                    if (_levelButtonRows[r].LevelButtons[c].IsIndex(documentRowIndex, c) && _levelButtonRows[r].LevelButtons[c].LevelDatum != null) continue;
                    
                    if (documentRowIndex >= 0 && documentRowIndex < _datumLists.Count && c < _datumLists[documentRowIndex].Count )
                    {
                        _levelButtonRows[r].LevelButtons[c].LevelDatum = _datumLists[documentRowIndex][c];
                        await _populateLevelButton(_levelButtonRows[r].LevelButtons[c], cancellationToken);
                        if (cancellationToken.IsCancellationRequested) return;
                        _levelButtonRows[r].LevelButtons[c].Index0 = documentRowIndex;
                        _levelButtonRows[r].LevelButtons[c].Index1 = c;
                        _levelButtonRows[r].LevelButtons[c].Image.color = Color.white;
                        _levelButtonRows[r].LevelButtons[c].Button.interactable = true;
                        _levelButtonRows[r].LevelButtons[c].ShowingLevel = true;
                        _levelButtonRows[r].LevelButtons[c].Text.text = "";
                    }
                    else
                    {
                        if (_finalButton != null && (documentRowIndex * ColumnCount) + c == TotalDatumCount())
                        {
                            _levelButtonRows[r].LevelButtons[c].Index0 = documentRowIndex;
                            _levelButtonRows[r].LevelButtons[c].Index1 = c;
                            _finalButton(_levelButtonRows[r].LevelButtons[c]);
                        }
                        else
                        {
                            _levelButtonRows[r].LevelButtons[c].SetEmpty(true);
                        }
                    }
                }

                documentRowIndex++;
            }
            
            if (documentRowIndex >= _datumLists.Count) PopulateNextSet();
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

        private async void PopulateNextSet()
        {
            if (_populatingNextSet || _isDonePopulating) return;
            _populatingNextSet = true;
            
            _isDonePopulating = await _populateNextSet(_datumLists);
            
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