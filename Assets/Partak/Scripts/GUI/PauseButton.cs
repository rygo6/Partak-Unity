using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PauseButton : MonoBehaviour 
{
	static public PauseButton staticThis;
	
	public GameObject pausePrefab;
	private GameObject pause;
	private Transform pauseTransform;
	
	public bool top;
	public bool right;
	public bool bottom;
	public bool left;
	
	Camera mainGuiCamera;
	
	private List<GameObject> pauseButton = new List<GameObject>();
	
	void Awake ()
	{
		staticThis = this;
	}
	
	void Start () 
	{
		mainGuiCamera = ProjectGlobal.staticThis.guiCamera[0];		
		
		float pauseDistance = 15f;
		float marginX = .02f;
		float marginY = .03f;		
		Vector3[] pausePos = new Vector3[4]
		{
		new Vector3(.5f,1f-marginY,pauseDistance),
		new Vector3(1f-marginX,.5f,pauseDistance),
		new Vector3(.5f,marginY,pauseDistance),
		new Vector3(marginX,.5f,pauseDistance)
		};
		
		Vector3 pos;
		Quaternion rot = Quaternion.Euler(0,0,0);
				
		if (top)
		{
			pos = mainGuiCamera.ViewportToWorldPoint(pausePos[0]);	
			pauseButton.Add( (GameObject)Instantiate(pausePrefab,pos,rot) );
		}
		if (right)
		{
			pos = mainGuiCamera.ViewportToWorldPoint(pausePos[1]);	
			pauseButton.Add( (GameObject)Instantiate(pausePrefab,pos,rot) );
		}		
		if (bottom)
		{
			pos = mainGuiCamera.ViewportToWorldPoint(pausePos[2]);	
			pauseButton.Add( (GameObject)Instantiate(pausePrefab,pos,rot) );
		}				
		if (left)
		{
			pos = mainGuiCamera.ViewportToWorldPoint(pausePos[3]);	
			pauseButton.Add( (GameObject)Instantiate(pausePrefab,pos,rot) );
		}
	}
	
	public void DisablePauseButtons()
	{
		for (int i = 0; i < pauseButton.Count; i++)
		{
			pauseButton[i].SetActive(false);
		}
	}	
	
	public void EnablePauseButtons()
	{
		for (int i = 0; i < pauseButton.Count; i++)
		{
			pauseButton[i].SetActive(true);
		}
	}		
	
}
