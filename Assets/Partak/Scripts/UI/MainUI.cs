using System.Collections;
using GeoTetra.GTUI;
using UnityEngine;
using UnityEngine.UI;

namespace Partak
{
    public class MainUI : StackUI
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private StackUI _playUI;
        [SerializeField] private Button _levelButton;
        [SerializeField] private StackUI _levelUI;
        [SerializeField] private Button _optionButton;
        [SerializeField] private StackUI _optionsUI;

        private void Awake()
        {
            base.Awake();
            InstantiateAndDisplayStackUIOnClick(_playButton, _playUI);
            InstantiateAndDisplayStackUIOnClick(_levelButton, _levelUI);
            InstantiateAndDisplayStackUIOnClick(_optionButton, _optionsUI);
        }
    }
}