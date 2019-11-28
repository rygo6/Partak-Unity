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

#if UNITY_EDITOR
//            string[] modalPrefabGUID = AssetDatabase.FindAssets("MessageModalUI");
//            if (modalPrefabGUID.Length > 0)
//            {
//                string modalPrefabPath = AssetDatabase.GUIDToAssetPath(modalPrefabGUID[0]);
//                _messageModalUIPrefab = AssetDatabase.LoadAssetAtPath<MessageModalUI>(modalPrefabPath);  
//            }
//
//            string[] selectionModalPrefabPath = AssetDatabase.FindAssets("SelectionModalUI");
//            if (selectionModalPrefabPath.Length > 0)
//            {
//                string selectionModalPrefabGUID = AssetDatabase.GUIDToAssetPath(selectionModalPrefabPath[0]);
//                _selectionModalUIPrefab = AssetDatabase.LoadAssetAtPath<SelectionModalUI>(selectionModalPrefabGUID);
//            }
#endif
            
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
        
        public async void DisplaySelectionModal(string[] messages, Action[] actions, int focusIndex)
        {
            SelectionModalUI messageModalUi = await CurrentlyRenderedBy.Pool.PoolInstantiateAsync<SelectionModalUI>(_selectionModalUiReference);
            messageModalUi.Init(messages, actions, focusIndex);
            CurrentlyRenderedBy.DisplayModalUI(messageModalUi);
        }

        public async void DisplayModal(string message, Action action = null)
        {
            MessageModalUI messageModalUi = await CurrentlyRenderedBy.Pool.PoolInstantiateAsync<MessageModalUI>(_messageModalUiReference);
            messageModalUi.Init(message, action);
            CurrentlyRenderedBy.DisplayModalUI(messageModalUi);
        }
        
        private void OnBackClicked()
        {
            CurrentlyRenderedBy.GoBack();
        }
    }
}