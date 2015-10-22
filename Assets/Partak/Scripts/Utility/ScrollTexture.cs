using UnityEngine;
using System.Collections;

public class ScrollTexture : MonoBehaviour 
{
	public bool constantScroll = true;
	public bool instanceMaterial = false;
	
	public float scrollRateX;
	public float scrollRateY;	
	
	private Renderer thisRenderer;
	private Material thisMaterial;
	
	private Vector2 offset;
	
	public Vector2 playOnceStart;
	public Vector2 playOnceEnd;
	public float playOnceOpenTime = 1.0f;
	public float playOnceCloseTime = 1.0f;
	

	
	private void Start () 
	{
		thisRenderer = this.GetComponent<Renderer>();
		if (instanceMaterial)
			thisMaterial = thisRenderer.material;			
		else
			thisMaterial = thisRenderer.sharedMaterial;
		
		offset = playOnceStart;
		thisMaterial.SetTextureOffset("_MainTex", offset);	
		
	}
	

	private void Update () 
	{
		if (constantScroll)
		{
			offset.x += Time.deltaTime * scrollRateX;
			offset.y += Time.deltaTime * scrollRateY;		
			thisMaterial.SetTextureOffset("_MainTex", offset);
		}
	}
	
	
	public void PlayOnceOpen()
	{
		StartCoroutine(PlayOnceCoroutine(playOnceStart,playOnceEnd,playOnceOpenTime));	
	}
	
	public void PlayOnceClose()
	{
		StartCoroutine(PlayOnceCoroutine(playOnceEnd,playOnceStart,playOnceCloseTime));
	}
	
	IEnumerator PlayOnceCoroutine(Vector2 start, Vector2 end, float timeMultiplier)
	{
		float time = 0.0f;
		while (time < 1.0f)
		{
			time += Time.deltaTime * timeMultiplier;
			offset = Vector2.Lerp(start,end,time);		
			thisMaterial.SetTextureOffset("_MainTex", offset);			
			yield return null;
		}		
		thisMaterial.SetTextureOffset("_MainTex", end);	
		
	}
	
	private void OnApplicationQuit()
	{
		thisMaterial.SetTextureOffset("_MainTex", playOnceEnd);

	}
}
