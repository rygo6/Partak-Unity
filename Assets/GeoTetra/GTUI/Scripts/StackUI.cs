using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace GeoTetra.GTUI
{
    public class StackUI : BaseUI
    {
        [SerializeField] private AssetReference _messageModalUiReference;
        [SerializeField] private AssetReference _selectionModalUiReference;
        [SerializeField] private Button _backButton;
        
        protected override void Awake()
        {
            base.Awake();
            if (_backButton != null) _backButton.onClick.AddListener(OnBackClicked);
        }

        protected override void Reset()
        {
            base.Reset();

            Button[] buttons = transform.GetComponentsInChildren<Button>();
            foreach (Button button in buttons)
            {
                if (button.gameObject.name == "BackButton") _backButton = button;
            }
        }
        
        public void InstantiateAndDisplayStackUIOnClick(Button button, AssetReference stackUI)
        {
            button.onClick.AddListener(() => InstantiateAndDisplayStackUI(stackUI));
        }
        
        public void InstantiateAndDisplayStackUI(AssetReference stackUI)
        {
            CurrentlyRenderedBy.InstantiateAndDisplayStackUI(stackUI);
        }

        protected void OnBackClicked()
        {
            CurrentlyRenderedBy.GoBack();
        }
    }
}