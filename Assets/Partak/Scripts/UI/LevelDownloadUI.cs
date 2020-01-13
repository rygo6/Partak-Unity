using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private ServiceReference _databaseService;
        [SerializeField] private List<LevelButtonRow> _levelButtonRows;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private int _rowSpacing = 10;

        private const int CollumnCount = 3;
        
        private LevelButton _selectedLevelButton;
        private PartakDatabase _partakDatabase;

        private Search _search;
        private List<List<Document>> _documentLists = new List<List<Document>>();
        private int _topRowIndex;
        private int _lastCollumnIndex;
        private Rect _itemRect;
        private bool _downloading;
        private bool _layingOut;
        
        protected override void Awake()
        {
            base.Awake();
            _partakDatabase = _databaseService.Service<PartakDatabase>();
            _scrollRect.onValueChanged.AddListener(OnScrolled);
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
            _search = _partakDatabase.QueryLevels(CollumnCount);
            _topRowIndex = 0;
            _lastCollumnIndex = 0;
            LayoutUI();
        }

        public override void OnTransitionOutFinish()
        {
            base.OnTransitionOutFinish();
            //Todo recycle nested lists?
            _documentLists.Clear();
            for (int r = 0; r < _levelButtonRows.Count; ++r)
            {
                for (int c = 0; c < CollumnCount; ++c)
                {
                    _levelButtonRows[r].LevelButtons[c].Text.text = "";
                    _levelButtonRows[r].LevelButtons[c].Image.texture = null;
                    _levelButtonRows[r].LevelButtons[c].Button.interactable = false;
                }
            }
        }
        
        private async void LayoutUI()
        {
            if (_layingOut) return;
            _layingOut = true;
            int documentRowIndex = _topRowIndex;
            bool nextSetShouldDownload = false;
            for (int r = 0; r < _levelButtonRows.Count; ++r)
            {
                for (int c = 0; c < CollumnCount; ++c)
                {
                    if (!_levelButtonRows[r].LevelButtons[c].IsIndex(r, c))
                    {
                        if (documentRowIndex < _documentLists.Count && c < _documentLists[documentRowIndex].Count )
                        {
                            _levelButtonRows[r].LevelButtons[c].Index0 = r;
                            _levelButtonRows[r].LevelButtons[c].Index1 = c;
                            await _levelButtonRows[r].LevelButtons[c].DownloadAndDisplayLevelAsync(_partakDatabase, _documentLists[documentRowIndex][c]);
                            _lastCollumnIndex = c;
                        }
                        else
                        {
                            nextSetShouldDownload = true;
                            _levelButtonRows[r].LevelButtons[c].SetEmpty();
                        }
                    }
                }

                documentRowIndex++;
            }
            
            if (nextSetShouldDownload) DownloadNextSet();
            _layingOut = false;
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
            
            List<Document> documentList = await _search.GetNextSetAsync();
            _documentLists.Add(documentList);
            
            float verticalSize = (_documentLists.Count + (_search.IsDone ? 0 : 1)) * (_itemRect.height + _rowSpacing);
            _scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, verticalSize);
            
            _downloading = false;
            
            LayoutUI();
        }

        private void OnLevelButtonClicked(LevelButton levelButton)
        {

        }

        private void Cancel()
        {
            
        }
    }
}