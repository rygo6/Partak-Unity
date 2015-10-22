using UnityEngine;
using System.Collections;

public class InputControl : MonoBehaviour 
{
	static public InputControl staticThis;
	public bool useTouch = false;	
	
	static public int fingerCount = 5;
	
	void Awake ()
	{
		staticThis =  this;
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			useTouch = true;	
		}
		if (Application.platform == RuntimePlatform.OSXEditor)
		{
			useTouch = false;	
		}		
		inputPos = new Vector3[fingerCount];
		inputPosUniqueID = new int[fingerCount];
		for (int i = 0; i < inputPosUniqueID.Length; i++)
			inputPosUniqueID[i] = -1;
		inputPhase = new byte[fingerCount];
		inputSetForCycle = new bool[fingerCount];
	}	
	
	//[HideInInspector]
	public Vector3[] inputPos;
	//[HideInInspector]
	public int[] inputPosUniqueID;
	//[HideInInspector]
	//0 = none, 1 = down, 2 = hold, 3 = up		
	public byte[] inputPhase;	
	
	//this is used as extra precuationary in case some devices 
	//may register touchphase began,moved,ended at the same time
	//it may be unnecessary
	private bool[] inputSetForCycle; 
	
	void Update () 
	{		
		if (useTouch)
		{
			Touch[] touches = Input.touches;
			int limit = touches.Length;
			int i;

			int limit2 = fingerCount;			
			int i2;			

			//find fingers that were lifted up previously and reset there position in input array
			for (i2 = 0; i2 < limit2; i2++)
			{
				if (inputPhase[i2] == 3)
				{
					inputPosUniqueID[i2] = -1;						
					inputPhase[i2] = 0;
				}
				inputSetForCycle[i2] = false;
			}	
			
			if( Input.touchCount > 0)	
			{
				//iterate through each touch
				for ( i = 0; i < limit; i++)
				{
					//if finger down
					if (touches[i].phase == TouchPhase.Began)
					{
						//Debug.Log(i+" down - fingerid :"+touches[i].fingerId);
						//find open slot in input and store
						for (i2 = 0; i2 < limit2; i2++)
						{
							if (inputPosUniqueID[i2] == -1)
							{
								inputPosUniqueID[i2] = touches[i].fingerId;
								inputPhase[i2] = 1;
								inputPos[i2].x = touches[i].position.x;
								inputPos[i2].y = touches[i].position.y;
								inputSetForCycle[i2] = true;
								i2 = limit2;
							}
						}
					}
					//if finger moved
					if (touches[i].phase == TouchPhase.Stationary || touches[i].phase == TouchPhase.Moved)
					{
						//Debug.Log(i+" stationary - fingerid :"+touches[i].fingerId);					
						//find some fingerID in input array and update
						for (i2 = 0; i2 < limit2; i2++)
						{
							if (inputPosUniqueID[i2] == touches[i].fingerId && !inputSetForCycle[i2])
							{
								inputPhase[i2] = 2;
								inputPos[i2].x = touches[i].position.x;
								inputPos[i2].y = touches[i].position.y;
								inputSetForCycle[i2] = true;
								i2 = limit2;
							}
						}
					}	
					//if finger up
					if (touches[i].phase == TouchPhase.Ended)
					{
						//Debug.Log(i+" ended - fingerid :"+touches[i].fingerId);					
						//find fingerid in input array and update
						for (i2 = 0; i2 < limit2; i2++)
						{
							if (inputPosUniqueID[i2] == touches[i].fingerId && !inputSetForCycle[i2])
							{
								inputPhase[i2] = 3;
								inputPos[i2].x = touches[i].position.x;
								inputPos[i2].y = touches[i].position.y;
								i2 = limit2;
							}
						}
					}	
				}
			}
		}
		else if (!useTouch)
		{
			int i;
			int limit = 3;
			for (i = 0; i < limit; i++)
			{
				inputSetForCycle[i] = false;
				
				if (inputPhase[i] == 3)
				{
					inputPhase[i] = 0;	
					inputPosUniqueID[i] = -1;
				}				
				
				if (Input.GetMouseButtonDown(i))
				{
					//Debug.Log("MouseDown");
					inputPosUniqueID[i] = i;
					inputPhase[i] = 1;
					inputPos[i] = Input.mousePosition;	
					inputSetForCycle[i] = true;
				}
				if (Input.GetMouseButton(i) && !inputSetForCycle[i])
				{
					//Debug.Log("MouseHold");				
					inputPhase[i] = 2;
					inputPos[i] = Input.mousePosition;	
					inputSetForCycle[i] = true;				
				}	
				if (Input.GetMouseButtonUp(i) && !inputSetForCycle[i])
				{
					//Debug.Log("MouseUp");				
					inputPhase[i] = 3;
					inputPos[i] = Input.mousePosition;	
				}	
			}
		}
	}
}
