using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GeoTetra.GTUI
{
    public class StackUI : BaseUI
    {
        [FormerlySerializedAs("_messageModalUiPrefab")] [SerializeField] private MessageModalUI _messageModalUIPrefab;
        [FormerlySerializedAs("_selectionModalUi")] [SerializeField] private SelectionModalUI _selectionModalUIPrefab;
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
        
        protected void InstantiateAndDisplayStackUIOnClick(Button button, StackUI stackUI)
        {
            button.onClick.AddListener(() => InstantiateAndDisplayStackUI(stackUI));
        }
        
        protected void InstantiateAndDisplayStackUI(StackUI stackUI)
        {
            CurrentlyRenderedBy.InstantiateAndDisplayStackUI(stackUI);
        }
        
        protected void DisplaySelectionModal(string[] messages, Action[] actions, int focusIndex)
        {
            SelectionModalUI messageModalUi = Instantiate(_selectionModalUIPrefab);
            messageModalUi.Init(messages, actions, focusIndex);
            CurrentlyRenderedBy.DisplayModalUI(messageModalUi);
        }

        protected void DisplayModal(string message, Action action = null)
        {
            MessageModalUI messageModalUi = Instantiate(_messageModalUIPrefab);
            messageModalUi.Init(message, action);
            CurrentlyRenderedBy.DisplayModalUI(messageModalUi);
        }
        
        private void OnBackClicked()
        {
            CurrentlyRenderedBy.GoBack();
        }
    }
}