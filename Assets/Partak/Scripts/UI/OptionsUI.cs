using UnityEngine;
using UnityEngine.UI;
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
			"You are now being sent to the geotetra Facebook page. 8circuit is the studio behind geotetra. Follow 8circuit on Facebook to see upcoming featues, games and provide feedback.",
			() => {
				Application.OpenURL("https://www.facebook.com/geotetra/");
			} 
		);
	}

	public void FocusButton() {
		_focusButton.Select();
	}
}
}