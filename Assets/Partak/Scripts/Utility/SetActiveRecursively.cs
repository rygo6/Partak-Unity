using UnityEngine;
using System.Collections;

public class SetActiveRecursively : MonoBehaviour 
{
	public bool state = false;
	
	void Start () 
	{
		this.gameObject.SetActive(state);
	}
	
	public void SetActive(bool state)
	{
		this.gameObject.SetActive(state);
	}

}
