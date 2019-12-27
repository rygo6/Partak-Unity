﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Partak
{
    public class LevelButtonRow : MonoBehaviour
    {
        [SerializeField] private List<LevelButton> _levelButtons;

        private ReadOnlyCollection<LevelButton> _levelButtonsReadOnly;
        
        public ReadOnlyCollection<LevelButton> LevelButtons => _levelButtonsReadOnly == null ? _levelButtonsReadOnly = _levelButtons.AsReadOnly() : _levelButtonsReadOnly;

        private void OnValidate()
        {
            if (_levelButtons.Count == 0) _levelButtons = GetComponentsInChildren<LevelButton>(true).ToList();
        }
    }
}