using UnityEngine;
using System.Collections;

public class ScrollTexture : MonoBehaviour 
{
	[SerializeField]
	private float scrollRateX;

	[SerializeField]
	private float scrollRateY;	

	private Vector2 _offset;

	private void Update () 
	{
		_offset.x += Time.deltaTime * scrollRateX;
		_offset.y += Time.deltaTime * scrollRateY;		
		GetComponent<Renderer>().material.SetTextureOffset("_MainTex", _offset);
	}

	private void OnDestroy()
	{
		GetComponent<Renderer>().material.SetTextureOffset("_MainTex", Vector2.zero);
	}
}
