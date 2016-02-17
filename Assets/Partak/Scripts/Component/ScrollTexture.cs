using UnityEngine;
using System.Collections;

public class ScrollTexture : MonoBehaviour {

	[SerializeField] float scrollRateX;

	[SerializeField] float scrollRateY;

	Vector2 _offset;

	void Update() {
		_offset.x += Time.deltaTime * scrollRateX;
		_offset.y += Time.deltaTime * scrollRateY;		
		GetComponent<Renderer>().material.SetTextureOffset("_MainTex", _offset);
	}

	void OnDestroy() {
		GetComponent<Renderer>().material.SetTextureOffset("_MainTex", Vector2.zero);
	}
}
