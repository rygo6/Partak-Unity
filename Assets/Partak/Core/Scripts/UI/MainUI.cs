using GeoTetra.GTUI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace GeoTetra.Partak
{
    public class MainUI : StackUI
    {
        [SerializeField] private PartakStateRef _partakState;
        [SerializeField] private Button _playButton;
        [SerializeField] private AssetReference _playUI;
        [SerializeField] private Button _levelButton;
        [SerializeField] private AssetReference _levelUI;
        [SerializeField] private Button _optionButton;
        [SerializeField] private AssetReference _optionsUI;

        protected override async void Awake()
        {
            await _partakState.Cache();
            
            _playButton.onClick.AddListener(() =>
            {
                if (_partakState.Service.LevelCatalogDatum.LevelIDs.Count == 0)
                {
                    CurrentlyRenderedBy.DisplayMessageModal("You have no levels loaded! Go into the level editor to download or create levels.");
                }
                else
                {
                    InstantiateAndDisplayStackUI(_playUI);
                }
            });
            InstantiateAndDisplayStackUIOnClick(_levelButton, _levelUI);
            InstantiateAndDisplayStackUIOnClick(_optionButton, _optionsUI);
        }
    }
}