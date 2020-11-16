using System;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using GeoTetra.GTUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GeoTetra.Partak.UI
{
    public class PlayerModeButton : MonoBehaviour, IPointerClickHandler, ISubmitHandler
    {
        [SerializeField] 
        private GameStateRef _gameStateRef;
        
        [SerializeField] private StackUI _parentStackUI;
        [SerializeField] private SelectionModalUI _selectionModalUi;
        [SerializeField] private int _playerIndex;
        [SerializeField] private Text _text;

        public void OnPointerClick(PointerEventData eventData)
        {
            Submit();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            Submit();
        }

        private async void Awake()
        {
            await _gameStateRef.Cache();
            PlayerMode mode = (PlayerMode) PlayerPrefs.GetInt("PlayerMode" + _playerIndex);
            _text.text = mode.ToString();
            _gameStateRef.Service.PlayerStates[_playerIndex].PlayerMode = mode;
        }
        
        private void OnValidate()
        {
            if (_parentStackUI == null) _parentStackUI = GetComponentInParent<StackUI>();
        }

        private void Submit()
        {
            string[] messages =
            {
                "Human",
                "Computer",
                "None"
            };
            Action[] actions =
            {
                () => { SetPlayerMode(PlayerMode.Human); },
                () => { SetPlayerMode(PlayerMode.Comp); },
                () => { SetPlayerMode(PlayerMode.None); }
            };
            _parentStackUI.CurrentlyRenderedBy.DisplaySelectionModal("Player Type:", messages, actions, (int)  _gameStateRef.Service.PlayerStates[_playerIndex].PlayerMode);
        }

        private void SetPlayerMode(PlayerMode mode)
        {
            GetComponent<Button>().GetComponentInChildren<Text>().text = mode.ToString();
            _gameStateRef.Service.PlayerStates[_playerIndex].PlayerMode = mode;
            PlayerPrefs.SetInt("PlayerMode" + _playerIndex, (int) mode);
        }
    }
}