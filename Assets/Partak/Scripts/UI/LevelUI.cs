using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GeoTetra.GTCommon.Attributes;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTUI;
using UnityEngine.SceneManagement;

namespace Partak
{
    public class LevelUI : StackUI
    {
        [SerializeField] private List<LevelButton> _levelButtons;
        
        private string[] EmptyLevelClickMessages;
        private Action[] EmptyLevelClickActions;
        
        protected override void Awake()
        {
            base.Awake();
            
            EmptyLevelClickMessages = new[] {"Download Existing Level", "Create New Level", "Cancel"};
            EmptyLevelClickActions = new Action[] {DownloadExistingLevel, CreateNewLevel, Cancel};
            
            for (int i = 0; i < _levelButtons.Count; ++i)
            {
                _levelButtons[i].Text.text = "";
                _levelButtons[i].ButtonClicked += OnButtonClicked;
            }
        }

        private void OnButtonClicked(LevelButton levelButton)
        {
            DisplaySelectionModal(EmptyLevelClickMessages, EmptyLevelClickActions, 0);
        }

        private void DownloadExistingLevel()
        {
            
        }

        private void CreateNewLevel()
        {
            
        }

        private void Cancel()
        {
            
        }

        protected override void Reset()
        {
            base.Reset();
            _levelButtons = FindObjectsOfType<LevelButton>().ToList();
        }
    }
}