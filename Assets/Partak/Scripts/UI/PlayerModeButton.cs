using UnityEngine;
using System;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Partak.ui {
public class PlayerModeButton : MonoBehaviour, IPointerClickHandler, ISubmitHandler {

	[SerializeField] int _playerIndex;

	void Awake() {
		PlayerMode mode = (PlayerMode)PlayerPrefs.GetInt("PlayerMode" + _playerIndex);
		GetComponent<Button>().GetComponentInChildren<Text>().text = mode.ToString();
		Persistent.Get<MenuConfig>().PlayerModes[_playerIndex] = mode;
	}

	public void OnSubmit(BaseEventData eventData) {
		Submit();
	}

	public void OnPointerClick(PointerEventData eventData) {
		Submit();
	}

	void Submit() {
		String[] messages = new string[] {
			"human",
			"comp",
			"none",
		};
		Action[] actions = new Action[] {
			() => {	SetPlayerMode(PlayerMode.Human); },
			() => {	SetPlayerMode(PlayerMode.Comp); },
			() => {	SetPlayerMode(PlayerMode.None); },
		};
		PopupSelectionUI popupSelectionUI = GameObject.Find("PopupSelectionUI").GetComponent<PopupSelectionUI>();
		popupSelectionUI.Show(messages, actions, (int)Persistent.Get<MenuConfig>().PlayerModes[_playerIndex]);
	}

	void SetPlayerMode(PlayerMode mode) {
		GetComponent<Button>().GetComponentInChildren<Text>().text = mode.ToString();
		Persistent.Get<MenuConfig>().PlayerModes[_playerIndex] = mode;
		PlayerPrefs.SetInt("PlayerMode" + _playerIndex, (int)mode);
	}
}
}