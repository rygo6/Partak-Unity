using UnityEngine;
using System.Collections;

public class RenderQueue : MonoBehaviour 
{
	public int renderQueue = 0;
	
	void Awake () 
	{
		this.GetComponent<Renderer>().sharedMaterial.renderQueue = renderQueue;
	}
}
