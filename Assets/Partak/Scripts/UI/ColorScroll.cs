using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Partak {
	public class ColorScroll : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler {

		[SerializeField]
		Material[] setMaterialColors;

		RawImage _rawImage;

		Texture2D _texture;

		PlayerSettings _playerSettings;
	
		readonly Color[] _colors = new Color[4];

		void Start() {
			_rawImage = GetComponent<RawImage>();
			_texture = (Texture2D)_rawImage.texture;
			_playerSettings = Persistent.Get<PlayerSettings>();
			SetColors();
		}

		public void OnBeginDrag(PointerEventData eventData) {
		
		}

		public void OnEndDrag(PointerEventData eventData) {
		
		}

		public void OnDrag(PointerEventData eventData) {
			Rect newRect = _rawImage.uvRect;
			newRect.x -= eventData.delta.x / 1000f;
			_rawImage.uvRect = newRect;
			SetColors();
		}

		void SetColors()
		{
			float u = 0.125f + _rawImage.uvRect.x;
			for (int i = 0; i < _playerSettings.PlayerColors.Length; ++i) {
				_playerSettings.PlayerColors[i] = _texture.GetPixelBilinear(u, .5f);
				setMaterialColors[i].color = _playerSettings.PlayerColors[i];
				u += .25f;
			}
		}
	}
}