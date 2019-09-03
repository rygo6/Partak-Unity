using System;
using GeoTetra.GTCommon.Variables;
using GeoTetra.GTUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Partak.UI
{
    public class PlayerModeButton : MonoBehaviour, IPointerClickHandler, ISubmitHandler
    {
        [SerializeField] private GameState _gameState;
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

        private void Awake()
        {
            PlayerMode mode = (PlayerMode) PlayerPrefs.GetInt("PlayerMode" + _playerIndex);
            _text.text = mode.ToString();
            _gameState.PlayerStates[_playerIndex].PlayerMode = mode;
        }

        private void Submit()
        {
            string[] messages =
            {
                "human",
                "comp",
                "none"
            };
            Action[] actions =
            {
                () => { SetPlayerMode(PlayerMode.Human); },
                () => { SetPlayerMode(PlayerMode.Comp); },
                () => { SetPlayerMode(PlayerMode.None); }
            };
            SelectionModalUI selectionModalUi = Instantiate(_selectionModalUi);
            selectionModalUi.Init(messages, actions, (int)  _gameState.PlayerStates[_playerIndex].PlayerMode);
            transform.root.GetComponent<BaseUI>().CurrentlyRenderedBy.DisplayModalUI(selectionModalUi);
        }

        private void SetPlayerMode(PlayerMode mode)
        {
            GetComponent<Button>().GetComponentInChildren<Text>().text = mode.ToString();
            _gameState.PlayerStates[_playerIndex].PlayerMode = mode;
            PlayerPrefs.SetInt("PlayerMode" + _playerIndex, (int) mode);
        }
    }
}