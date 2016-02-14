using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;
using System.Collections;

namespace Partak {
public class OptionsUI : MonoBehaviour {

	[SerializeField] private Button _focusButton;

	public void RestorePurchases() {
		Persistent.Get<Store>().RestorePurchases();
	}

	public void Mute() {
		switch (PlayerPrefs.GetInt("muted")) {
		case 0:
		case 1:
			AudioListener.volume = 0f;
			PlayerPrefs.SetInt("muted", 2);
			GameObject.Find("PopupUI").GetComponent<PopupUI>().Show("sound muted");
			break;
		case 2:
			AudioListener.volume = 1f;
			PlayerPrefs.SetInt("muted", 1);
			GameObject.Find("PopupUI").GetComponent<PopupUI>().Show("sound enabled");
			break;
		}
	}

	public void Facebook() {
		GameObject.Find("PopupUI").GetComponent<PopupUI>().Show(
			"You are now being sent to the 8circuit Facebook page. 8circuit is the studio behind partak. Follow 8circuit on Facebook to see upcoming featues, games and provide feedback.",
			() => {
				Application.OpenURL("https://www.facebook.com/eightcircuit/");
			} 
		);
	}

	public void FocusButton() {
		_focusButton.Select();
	}
}
}