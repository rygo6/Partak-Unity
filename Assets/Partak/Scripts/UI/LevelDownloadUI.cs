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

namespace Partak
{
    public class LevelDownloadUI : StackUI
    {
        [SerializeField] private ServiceReference _databaseService;
        [SerializeField] private List<LevelButtonRow> _levelButtonRows;

        private LevelButton _selectedLevelButton;
        private PartakDatabase _partakDatabase;

        private Search _search;
        private List<Document> _documentList = new List<Document>();
        
        protected override void Awake()
        {
            base.Awake();
            _partakDatabase = _databaseService.Service<PartakDatabase>();
        }

        private async void LayoutUI()
        {
            int row = 0;
            do
            {
                Debug.Log("Populating documentlsit");
                _documentList = await _search.GetNextSetAsync();
                int collumn = 0;
                foreach (var document in _documentList)
                {
                    _levelButtonRows[row].LevelButtons[collumn].Text.text = document["level_data"];
                    collumn++;
                }

                row++;
            } while (!_search.IsDone && row < _levelButtonRows.Count);
        }

        public override void OnTransitionInStart(UIRenderer uiRenderer)
        {
            base.OnTransitionInStart(uiRenderer);
        }

        public override void OnTransitionInFinish()
        {
            base.OnTransitionInFinish();
            _search = _partakDatabase.QueryLevels(3); 
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