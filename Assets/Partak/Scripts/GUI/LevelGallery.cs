using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelGallery : MonoBehaviour
{
	[SerializeField]
	public Button _rightButton;

	[SerializeField]
	public Button _leftButton;

	private Material _material;

	public int _galleryIndex = 0;

	[SerializeField]
	public int _galleryLength = 3;

	[SerializeField]
	public float _galleryTransitionSpeed = 4f;

	private void Start()
	{		
		_leftButton.onClick.AddListener(GalleryLeft);
		_rightButton.onClick.AddListener(GalleryRight);

		_material = GetComponent<RawImage>().material;
		_material.SetTexture("_Texture1", (Texture)Resources.Load("LevelPreviews/" + _galleryIndex));	
	}

	public void GalleryRight()
	{
		StartCoroutine(GalleryTransitionCoroutine(1));			
	}
	
	public void GalleryLeft()
	{
		StartCoroutine(GalleryTransitionCoroutine(-1));				
	}
	
	private IEnumerator GalleryTransitionCoroutine(int direction)
	{			
		_rightButton.interactable = false;
		_leftButton.interactable = false;

		_galleryIndex += direction;
		_galleryIndex = (int)Mathf.Repeat(_galleryIndex + direction, _galleryLength);

		_material.SetTexture("_Texture2", (Texture)Resources.Load("LevelPreviews/" + _galleryIndex));	

		yield return null;
		yield return null;

		Vector2 mainStart = new Vector2(0, 0);
		Vector2 mainEnd = new Vector2(direction, 0);	
		Vector2 mainOffset;

		Vector2 texture2Start = new Vector2(mainEnd.x * -1, mainEnd.y);
		Vector2 texture2End = new Vector2(mainStart.x, mainStart.y);
		Vector2 texture2Offset;

		float fade;
		
		float time = 0.0f;

		while (time < 1.0f)
		{
			time += Time.deltaTime * _galleryTransitionSpeed;

			mainOffset = Vector2.Lerp(mainStart, mainEnd, time);	
			texture2Offset = Vector2.Lerp(texture2Start, texture2End, time);

			fade = Mathf.Lerp(0, 1, time);

			_material.SetFloat("_Blend", fade);
			_material.SetTextureOffset("_Texture1", mainOffset);	
			_material.SetTextureOffset("_Texture2", texture2Offset);		
			
			yield return null;
		}		
		Resources.UnloadAsset(_material.GetTexture("_Texture1"));	
		_material.SetTexture("_Texture1", _material.GetTexture("_Texture2"));
		_material.SetFloat("_Blend", 0);	
		_material.SetTextureOffset("_Texture1", mainStart);	
		_material.SetTextureOffset("_Texture2", mainEnd);			
		_material.SetTexture("_Texture2", null);

		_rightButton.interactable = true;
		_leftButton.interactable = true;
	}
	
	private void OnApplicationQuit()
	{
		_material.SetTexture("_Texture1", null);
		_material.SetTexture("_Texture2", null);
		_material.SetTextureOffset("_Texture1", Vector2.zero);			
		_material.SetTextureOffset("_Texture2", Vector2.zero);	
	}
}
