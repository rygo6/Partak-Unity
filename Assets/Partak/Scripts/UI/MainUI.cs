using System.Collections;
using GeoTetra.GTUI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Partak
{
    public class MainUI : StackUI
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private AssetReference _playUI;
        [SerializeField] private Button _levelButton;
        [SerializeField] private AssetReference _levelUI;
        [SerializeField] private Button _optionButton;
        [SerializeField] private AssetReference _optionsUI;

        protected override void Awake()
        {
            base.Awake();
            InstantiateAndDisplayStackUIOnClick(_playButton, _playUI);
            InstantiateAndDisplayStackUIOnClick(_levelButton, _levelUI);
            InstantiateAndDisplayStackUIOnClick(_optionButton, _optionsUI);
        }
    }
}