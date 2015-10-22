using UnityEngine;
using System.Collections;

public class Utility 
{
	public class ReferenceCache
	{
		public Transform transform;
		public Renderer renderer;
		public Material material;
		public MeshFilter meshFilter;
	}
	
	static public void CacheObjectReferences(ref GameObject gameObject,ref ReferenceCache referenceCache)
	{
		referenceCache = new ReferenceCache();
		if(gameObject)
		{
			referenceCache.transform = gameObject.transform;
		
			referenceCache.renderer = gameObject.GetComponent<Renderer>();
		
			referenceCache.material = gameObject.GetComponent<Renderer>().material;		
			
			referenceCache.meshFilter = gameObject.GetComponent<MeshFilter>();
		}		
	}
	
	static public void IncrementXY(ref int x, ref int y, ref int width)
	{
		x++;
		if(x == width)
		{
			x = 0;
			y++;
		}			
	}

	static public Vector2 ScreenInputToProportionalRatio(Vector3 input)
	{
		Vector2 output = new Vector2();
		
		output.x = input.x / (float)Screen.width;
		output.y = input.y / (float)Screen.width;
		
		return output;
	}
}
