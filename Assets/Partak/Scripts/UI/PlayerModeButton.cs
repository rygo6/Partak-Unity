using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Partak.ui
{
    public class PlayerModeButton : MonoBehaviour, IPointerClickHandler, ISubmitHandler
    {
        [SerializeField] private int _playerIndex;

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
            GetComponent<Button>().GetComponentInChildren<Text>().text = mode.ToString();
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
            PopupSelectionUI popupSelectionUI = GameObject.Find("PopupSelectionUI").GetComponent<PopupSelectionUI>();
            popupSelectionUI.Show(messages, actions, (int) Persistent.Get<MenuConfig>().PlayerModes[_playerIndex]);
        }

        private void SetPlayerMode(PlayerMode mode)
        {
            GetComponent<Button>().GetComponentInChildren<Text>().text = mode.ToString();
            Persistent.Get<MenuConfig>().PlayerModes[_playerIndex] = mode;
            PlayerPrefs.SetInt("PlayerMode" + _playerIndex, (int) mode);
        }
    }
}