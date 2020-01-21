using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GeoTetra.GTCommon.Components;
using UnityEngine;
using UnityEngine.UI;

namespace GeoTetra.Partak
{
    public class LevelButtonRow : SubscribableBehaviour
    {
        [SerializeField] private List<LevelButton> _levelButtons;

        private ReadOnlyCollection<LevelButton> _levelButtonsReadOnly;
        
        public ReadOnlyCollection<LevelButton> LevelButtons => _levelButtonsReadOnly == null ? _levelButtonsReadOnly = _levelButtons.AsReadOnly() : _levelButtonsReadOnly;

        private void OnValidate()
        {
            if (_levelButtons.Count == 0) _levelButtons = GetComponentsInChildren<LevelButton>(true).ToList();
        }

        public void SetEmpty()
        {
            for (int i = 0; i < _levelButtons.Count; ++i)
            {
                _levelButtons[i].SetEmpty(true);
            }
        }
    }
}