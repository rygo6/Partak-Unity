using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Partak {
public class TimeSelect : MonoBehaviour, IPointerClickHandler {

	[SerializeField]
	private Text _minutesText;

	private int _minutes = 1;

	void Start() {
		_minutes = PlayerPrefs.GetInt("GameTime", 3);
		_minutesText.text = _minutes.ToString();
		Persistent.Get<MenuConfig>().TimeLimitMinutes = _minutes;
	}

	public void OnPointerClick(PointerEventData eventData) {
		switch (_minutes) {
		case 1:
			_minutes = 3;
			break;
		case 3:
			_minutes = 6;
			break;
		case 6:
			_minutes = 1;
			break;
		}
		Persistent.Get<MenuConfig>().TimeLimitMinutes = _minutes;
		_minutesText.text = _minutes.ToString();
		PlayerPrefs.SetInt("GameTime", _minutes);
	}
}
}