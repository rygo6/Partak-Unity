using UnityEngine;
using System.Collections;

public class PlayAnimationAfterDelay : MonoBehaviour 
{
	public string animationName;
	public float delay;
	
	void Start () 
	{
		StartCoroutine(PlayAnimation());
	}
	
	private IEnumerator PlayAnimation()
	{
		yield return new WaitForSeconds(delay);
		this.GetComponent<Animation>().Play(animationName);
		this.enabled = false;
	}

}
