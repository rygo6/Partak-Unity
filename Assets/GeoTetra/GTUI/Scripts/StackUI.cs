using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;
using UnityEngine.UI;
using GeoTetra.GTPooling;

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
        
        [Obsolete("Use UIRenderer")]
        public async void DisplaySelectionModal(string mainMessage, string[] messages, Action[] actions, int focusIndex)
        {
            SelectionModalUI messageModalUi = await CurrentlyRenderedBy.Pool.PoolInstantiateAsync<SelectionModalUI>(_selectionModalUiReference);
            messageModalUi.Init(mainMessage, messages, actions, focusIndex);
            CurrentlyRenderedBy.DisplayModalUI(messageModalUi);
        }

        [Obsolete("Use UIRenderer")]
        public async void DisplayModal(string message, Action action = null)
        {
            MessageModalUI messageModalUi = await CurrentlyRenderedBy.Pool.PoolInstantiateAsync<MessageModalUI>(_messageModalUiReference);
            messageModalUi.Init(message, action);
            CurrentlyRenderedBy.DisplayModalUI(messageModalUi);
        }
        
        protected void OnBackClicked()
        {
            CurrentlyRenderedBy.GoBack();
        }
    }
}