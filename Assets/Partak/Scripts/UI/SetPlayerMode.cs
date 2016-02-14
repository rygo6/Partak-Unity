using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Partak.UI {
public class SetPlayerMode : MonoBehaviour {

	[SerializeField] int _playerIndex;
	[SerializeField] PlayerMode _playerMode;

	void Start() {
		_playerMode = (PlayerMode)PlayerPrefs.GetInt("PlayerMode" + _playerIndex);
		switch (_playerMode) {
		case PlayerMode.Human:
			_playerMode = PlayerMode.Human;
			GetComponent<Button>().GetComponentInChildren<Text>().text = "human";
			break;
		case PlayerMode.Comp:
			_playerMode = PlayerMode.Comp;
			GetComponent<Button>().GetComponentInChildren<Text>().text = "comp";
			break;
		case PlayerMode.None:
			_playerMode = PlayerMode.None;
			GetComponent<Button>().GetComponentInChildren<Text>().text = "none";
			break;
		}
		GetComponent<Button>().onClick.AddListener(OnClick);
	}

	void OnClick() {
		switch (_playerMode) {
		case PlayerMode.Human:
			_playerMode = PlayerMode.Comp;
			GetComponent<Button>().GetComponentInChildren<Text>().text = "comp";
			break;
		case PlayerMode.Comp:
			_playerMode = PlayerMode.None;
			GetComponent<Button>().GetComponentInChildren<Text>().text = "none";
			break;
		case PlayerMode.None:
			_playerMode = PlayerMode.Human;
			GetComponent<Button>().GetComponentInChildren<Text>().text = "human";
			break;
		}
		PlayerPrefs.SetInt("PlayerMode" + _playerIndex, (int)_playerMode);
		Persistent.Get<MenuConfig>().PlayerModes[_playerIndex] = _playerMode;
	}
}
}