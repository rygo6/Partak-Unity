using System;
using GeoTetra.GTUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Partak.UI
{
    public class PlayerModeButton : MonoBehaviour, IPointerClickHandler, ISubmitHandler
    {
        [SerializeField] private ModalSelectionUI _modalSelectionUI;
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
            Persistent.Get<MenuConfig>().PlayerModes[_playerIndex] = mode;
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
            ModalSelectionUI modalSelectionUi = Instantiate(_modalSelectionUI);
            modalSelectionUi.Init(messages, actions, (int) Persistent.Get<MenuConfig>().PlayerModes[_playerIndex]);
            transform.root.GetComponent<BaseUI>().CurrentlyRenderedBy.DisplayModalUI(modalSelectionUi);
        }

        private void SetPlayerMode(PlayerMode mode)
        {
            GetComponent<Button>().GetComponentInChildren<Text>().text = mode.ToString();
            Persistent.Get<MenuConfig>().PlayerModes[_playerIndex] = mode;
            PlayerPrefs.SetInt("PlayerMode" + _playerIndex, (int) mode);
        }
    }
}