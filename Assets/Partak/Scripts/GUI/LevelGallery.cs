//using UnityEngine;
//using System.Collections;
//
//public class LevelGallery : MonoBehaviour 
//{
//	public bool instanceMaterial = false;
//	
//	private Renderer thisRenderer;
//	private Material thisMaterial;
//	
//	private Vector2 offset;
//	
//	public int gallerySize = 3;
//	public int galleryIndex = 0;
//	public float galleryTransitionSpeed = 2f;
//	
//	public string galleryName;
//	
//	public GameObject fullVersionOnlyLabelPrefab;
//	public Vector3 fullVersionLabelEuler;
//	public Vector3 fullVersionLabelOffset;
//	private GameObject fullVersionOnlyLabel;
//	private Transform fullVersionOnlyLabelTransform;	
//	private Renderer fullVersionOnlyLabelRenderer;		
//	
//	private void Start () 
//	{
//		thisRenderer = this.renderer;
//		if (instanceMaterial)
//			thisMaterial = thisRenderer.material;			
//		else
//			thisMaterial = thisRenderer.sharedMaterial;
//		
//		//gallerySize = levelName.Length;
//		
//		galleryIndex = PlayerPrefs.GetInt(galleryName+"GalleryIndex");
//		
//		thisMaterial.SetTextureOffset("_MainTex", offset);	
//		thisMaterial.SetTexture("_MainTex", (Texture)Resources.Load("LevelPreviews/"+(galleryIndex+1)));	
//		
//		if (fullVersionOnlyLabelPrefab != null)
//		{
//			Debug.Log("Adding Full Version Only label on "+this.name);
//			fullVersionOnlyLabel = (GameObject)Instantiate(fullVersionOnlyLabelPrefab,Vector3.zero,Quaternion.identity);
//			fullVersionOnlyLabelTransform = fullVersionOnlyLabel.transform;
//			fullVersionOnlyLabelRenderer = fullVersionOnlyLabel.renderer;
//			fullVersionOnlyLabelTransform.parent = this.transform;
//			fullVersionOnlyLabelTransform.localPosition = fullVersionLabelOffset;
//			fullVersionOnlyLabelTransform.localEulerAngles = fullVersionLabelEuler;	
//			fullVersionOnlyLabelRenderer.enabled = false;
//		}
//	}
//	
//	private void Update()
//	{
//		PlayerData.selectedLevelName = (galleryIndex+1).ToString();
//	}
//	
//	public void GalleryRight()
//	{
//		GUIControl.taskInProgress++;
//		StartCoroutine(GalleryTransitionCoroutine(1));			
//	}
//	
//	public void GalleryLeft()
//	{
//		GUIControl.taskInProgress++;		
//		StartCoroutine(GalleryTransitionCoroutine(-1));				
//	}
//	
//	IEnumerator GalleryTransitionCoroutine(int direction)
//	{			
//		galleryIndex += direction;
//		if (galleryIndex == gallerySize)
//			galleryIndex = 0;
//		else if (galleryIndex == -1)
//			galleryIndex = gallerySize-1;
//		
//		PlayerPrefs.SetInt(galleryName+"GalleryIndex",galleryIndex);
//		
//		thisMaterial.SetTexture("_Texture2", (Texture)Resources.Load("LevelPreviews/"+(galleryIndex+1)));	
//		yield return null;
//		yield return null;
//		yield return null;
//		yield return null;
//		Vector2 start = new Vector2(0,0);
//		Vector2 end = new Vector2(direction,0);		
//		Vector2 texture2Start = new Vector2(end.x*-1,end.y);
//		Vector2 texture2End = new Vector2(start.x,start.y);
//		Vector2 texture2Offset;
//		float fade;
//		
//		float time = 0.0f;
//		float easedTime = 0.0f;
//		while (time < 1.0f)
//		{
//			time += Time.deltaTime * galleryTransitionSpeed;
//			easedTime = ProjectGlobal.staticThis.easeInEaseOut.Evaluate(time);
//			offset = Vector2.Lerp(start,end,easedTime);	
//			texture2Offset = Vector2.Lerp(texture2Start,texture2End,easedTime);
//			fade = Mathf.Lerp(0,1,easedTime);
//			thisMaterial.SetFloat("_Blend",fade);
//			thisMaterial.SetTextureOffset("_MainTex", offset);	
//			thisMaterial.SetTextureOffset("_Texture2", texture2Offset);		
//			
//			yield return null;
//		}		
//		
//		thisMaterial.SetTextureOffset("_Texture2", end);			
//		thisMaterial.SetTextureOffset("_MainTex", start);
//		thisMaterial.SetTexture("_MainTex" ,thisMaterial.GetTexture("_Texture2"));
//		thisMaterial.SetFloat("_Blend",0);		
//		thisMaterial.SetTexture("_Texture2", null);
//		Resources.UnloadUnusedAssets();
//		
//		if (galleryIndex >= ProjectGlobal.FullVersionOnlyLevelCount && !ProjectGlobal.isFullVersion)
//		{
//			fullVersionOnlyLabelRenderer.enabled = true;
//		}
//		else
//		{
//			fullVersionOnlyLabelRenderer.enabled = false;			
//		}
//		
//		GUIControl.taskInProgress--;		
//	}	
//	
//	private void OnApplicationQuit()
//	{
//		thisMaterial.SetTexture("_MainTex", null);
//		thisMaterial.SetTexture("_Texture2", null);
//	}
//}
