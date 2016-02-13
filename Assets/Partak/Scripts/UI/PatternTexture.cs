using UnityEngine;
using System.Collections;

namespace Partak {
public class PatternTexture : MonoBehaviour {

	[SerializeField]
	Material _applyMaterial;

	Texture2D _texture2D;

	MenuConfig _playerSettings;

	Color _testColor = Color.black;

	readonly Color[] PixelColors = new Color[SquareSize * SquareSize];

	const int TextureSize = 256;

	const int Divisions = 8;

	const int SquareSize = TextureSize / Divisions;

	void Start() {
		_texture2D = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false, false);
		_applyMaterial.mainTexture = _texture2D;
		_playerSettings = Persistent.Get<MenuConfig>();
	}

	void Update() {
		if (!_testColor.RGBEquals(_playerSettings.PlayerColors[0])) {
			_testColor = _playerSettings.PlayerColors[0];
			SetTexture();
		}
	}

	void SetTexture() {
		bool xblack = false;
		bool yBlack = false;
		int playerIndex = 0;
		for (int y = 0; y < TextureSize; y += SquareSize) {
			for (int x = 0; x < TextureSize; x += SquareSize) {
				if (yBlack) {
					SetPixelColors(Color.black);
				} else if (xblack) {
					SetPixelColors(Color.black);
					xblack = false;
				} else {
					SetPixelColors(_playerSettings.PlayerColors[playerIndex]);
					playerIndex++;
					if (playerIndex == _playerSettings.PlayerColors.Length)
						playerIndex = 0;
					xblack = true;
				}
				_texture2D.SetPixels(x, y, SquareSize, SquareSize, PixelColors);
			}
			if (yBlack)
				yBlack = false;
			else {
				yBlack = true;
				if (xblack)
					xblack = false;
				else
					xblack = true;
			}
		}
		_texture2D.Apply();
	}

	void SetPixelColors(Color color) {
		for (int i = 0; i < PixelColors.Length; ++i) {
			PixelColors[i] = color;
		}
	}
}
}