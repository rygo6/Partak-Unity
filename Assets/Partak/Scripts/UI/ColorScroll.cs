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

	MenuConfig _playerSettings;

	void Start() {
		_rawImage = GetComponent<RawImage>();
		_texture = (Texture2D)_rawImage.texture;
		_playerSettings = Persistent.Get<MenuConfig>();
		Rect newRect = _rawImage.uvRect;
		newRect.x = PlayerPrefs.GetFloat("ColorScrollX", -.125f);
		_rawImage.uvRect = newRect;
		SetColors();
	}

	public void StartScrollRight() {
		InvokeRepeating("ScrollRight", 0f, 0.02f);
	}

	public void StartScrollLeft() {
		InvokeRepeating("ScrollLeft", 0f, 0.02f);
	}

	public void StopScroll() {
		CancelInvoke("ScrollRight");
		CancelInvoke("ScrollLeft");
	}

	public void ScrollRight() {
		Scroll(-0.002f);
	}

	public void ScrollLeft() { 
		Scroll(0.002f);
	}

	void Scroll(float amount) {
		Rect newRect = _rawImage.uvRect;
		newRect.x += amount;
		_rawImage.uvRect = newRect;
		SetColors();
	}

	void SetColors() {
		float u = 0.125f + _rawImage.uvRect.x;
		for (int i = 0; i < _playerSettings.PlayerColors.Length; ++i) {
			_playerSettings.PlayerColors[i] = _texture.GetPixelBilinear(u, .5f);
			setMaterialColors[i].color = _playerSettings.PlayerColors[i];
			u += .25f;
		}
	}

	public void OnBeginDrag(PointerEventData eventData) {
		
	}

	public void OnEndDrag(PointerEventData eventData) {
		PlayerPrefs.SetFloat("ColorScrollX", _rawImage.uvRect.x);
	}

	public void OnDrag(PointerEventData eventData) {
		Scroll(eventData.delta.x / -1000f);
	}
}
}