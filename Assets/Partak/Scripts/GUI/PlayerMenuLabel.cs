//using UnityEngine;
//using System.Collections;
//
//public class PlayerMenuLabel : MonoBehaviour 
//{
//	private Transform thisTransform;
//	private Renderer thisRenderer;
//	private PlayerWheelRaycast hitPlayerWheelRaycast;
//	private Vector2 hitUV;
//	
//	private int raycastTouchLayer = 1 << 31;		
//	
//	private int materialIndex;	
//	private Texture2D sampleTexture;
//	
//	public int playerID;
//	
//	private Vector3 priorPos;
//	
//	void Awake ()
//	{
//		thisTransform = this.transform;	
//		thisRenderer = this.renderer;
//		
//		int i = 0;
//		foreach (Material material in thisRenderer.sharedMaterials)
//		{
//			if( material.name != "black" )
//			{
//				materialIndex = i;
//			}
//			i++;
//		}
//	}
//	
//	void Update () 
//	{
//		if( hitPlayerWheelRaycast == null || priorPos != thisTransform.localPosition)
//		{		
//			Ray ray = new Ray(thisTransform.position,thisTransform.right);
//			RaycastHit raycastHit;
//			
//			if (Physics.Raycast(ray, out raycastHit, 10f, raycastTouchLayer))
//			{
//				hitPlayerWheelRaycast = raycastHit.transform.GetComponent<PlayerWheelRaycast>();
//				hitUV = raycastHit.textureCoord;
//				foreach (Material material in raycastHit.transform.renderer.materials)
//				{
//					if (material.name.Contains("Scroll"))
//					{
//						sampleTexture = (Texture2D)material.mainTexture;
//					}
//				}
//			}
//			
//			priorPos = thisTransform.localPosition;
//		}
//		else
//		{
//			thisRenderer.materials[materialIndex].color = sampleTexture.GetPixelBilinear((hitUV.x+hitPlayerWheelRaycast.scrollOffset.x), hitUV.y);		
//			PlayerData.player[playerID].startColor = thisRenderer.materials[materialIndex].color;		
//		}
//	}
//	
//
//	
//	
//}
