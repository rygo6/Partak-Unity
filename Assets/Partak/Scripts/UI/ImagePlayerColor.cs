using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Partak {
public class ImagePlayerColor : MonoBehaviour {

	[SerializeField]
	int _playerIndex;

	[SerializeField]
	bool _constantUpdate;

	PlayerSettings _playerSettings;

	Image _image;

	void Start() {
		_playerSettings = Persistent.Get<PlayerSettings>();
		_image = GetComponent<Image>();
		_image.color = _image.color.SetRGB(_playerSettings.PlayerColors[_playerIndex]);
		if (_constantUpdate)
			StartCoroutine(UpdateColor());
	}

	IEnumerator UpdateColor() {
		while (true) {
			if (!_image.color.RGBEquals(_playerSettings.PlayerColors[_playerIndex]))
				_image.color = _image.color.SetRGB(_playerSettings.PlayerColors[_playerIndex]);
			yield return null;
		}
	}
}
}