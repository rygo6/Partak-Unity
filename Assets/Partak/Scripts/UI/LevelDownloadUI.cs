using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private int topRow;
        private int currentRow;
        private int currentCollumn;
        
        protected override void Awake()
        {
            base.Awake();
            _partakDatabase = _databaseService.Service<PartakDatabase>();
        }

        private async void LayoutUI()
        {
            do
            {
                currentRow++;
                List<Document> documentList = await _search.GetNextSetAsync();
                _documentLists.Add(documentList);
                currentCollumn = -1;
                foreach (Document document in documentList)
                {
                    currentCollumn++;
                    Texture levelImage = await _partakDatabase.DownloadLevelImage(document);
                    _levelButtonRows[currentRow].LevelButtons[currentCollumn].Button.interactable = true;
                    _levelButtonRows[currentRow].LevelButtons[currentCollumn].Text.text = "Download";
                    _levelButtonRows[currentRow].LevelButtons[currentCollumn].Text.color = new Color(1, 1, 1, .1f);
                    _levelButtonRows[currentRow].LevelButtons[currentCollumn].Image.color = Color.white;
                    _levelButtonRows[currentRow].LevelButtons[currentCollumn].Image.texture = levelImage;

                }
            } while (!_search.IsDone && currentRow < _levelButtonRows.Count);

            float verticalSize = _documentLists.Count * (_levelButtonRows[0].LevelButtons[0].Image.rectTransform.rect.height + _rowSpacing);
            _scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, verticalSize);
            
            for (int r = currentRow; r < _levelButtonRows.Count; ++r)
            {
                for (int c = currentRow == r ? currentCollumn + 1 : 0; c < CollumnCount; ++c)
                {
                    _levelButtonRows[r].LevelButtons[c].Text.text = "";
                    _levelButtonRows[r].LevelButtons[c].Button.interactable = false;
                }
            }
        }

        public override void OnTransitionInStart(UIRenderer uiRenderer)
        {
            base.OnTransitionInStart(uiRenderer);
            _search = _partakDatabase.QueryLevels(CollumnCount);
            topRow = 0;
            currentRow = -1;
            LayoutUI();
        }

        public override void OnTransitionInFinish()
        {
            base.OnTransitionInFinish();
        }

        public override void OnTransitionOutFinish()
        {
            base.OnTransitionOutFinish();
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

        private void OnLevelButtonClicked(LevelButton levelButton)
        {

        }

        private void Cancel()
        {
            
        }
    }
}