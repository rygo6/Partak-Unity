using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CursorManager : MonoBehaviour 
{
	static public CursorManager staticThis;
	
	
	private ProjectGlobal projectGlobal;
	private InputControl inputControl;
	private ParticleGridControl particleGridControl;
	
	public GameObject cursorPrefab;
	private GameObject[] cursorInstance;
	private Transform[] cursorTransform;
	private Renderer[] cursorRenderer;
	private MorphTargets[] cursorMorph;	
	
//	public GameObject dPadPrefab;
//	private GameObject[] dPadInstance;
//	private DPadControl[] dPadControl;
	
	public float cursorSpeedMultiplier = 4;

	[HideInInspector]	
	public Vector3[] cursorScreenPosition;
	[HideInInspector]		
	public Vector3[] cursorLocalPosition;	
	public float[] cursorSpin;
	private Vector3[] cursorLocalEuler;
	
	private Camera mainCamera;
	private Camera mainGuiCamera;
	
	private Vector3[] dPadStartPos;
	
	[HideInInspector]
	public Vector3 cursorBoundBL; //bottom left
	[HideInInspector]
	public Vector3 cursorBoundUR; //upper right
	
	private bool updateCursors = true;
	
	void Awake()
	{
		staticThis = this;	

		cursorScreenPosition = new Vector3[PlayerData.playerCount];
		cursorLocalPosition = new Vector3[PlayerData.playerCount];		
//		dPadInstance = new GameObject[PlayerData.playerCount];
//		dPadControl = new DPadControl[PlayerData.playerCount];	
		cursorInstance =  new GameObject[PlayerData.playerCount];
		cursorMorph = new MorphTargets[PlayerData.playerCount];
		cursorTransform = new Transform[PlayerData.playerCount];
		cursorRenderer = new Renderer[PlayerData.playerCount];	
		cursorLocalEuler = new Vector3[PlayerData.playerCount];	
		cursorSpin = new float[PlayerData.playerCount];	
	}
	
	
	void Start()
	{
		projectGlobal = ProjectGlobal.staticThis;
		inputControl = InputControl.staticThis;
		mainCamera =  projectGlobal.mainCamera;
//		mainGuiCamera = projectGlobal.guiCamera[0];
		
		aiCycle = new int[PlayerData.playerCount];
		
		List<int> aiList = new List<int>();
		for (int i = 0; i < PlayerData.playerCount; i++)
		{
			if (PlayerData.player[i].ai)
			{
				aiList.Add(i);	
			}	
		}
		aiIndeces = aiList.ToArray();
		aiUpdateRate = aiUpdateRate / aiIndeces.Length;
		
		float cursorDistance = 35f;
		float dPadDistance = 20f;

		float marginX = .08f;
		float marginY = .1f;		
		dPadStartPos = new Vector3[4]
		{
		new Vector3(marginX,1f-marginY,dPadDistance),
		new Vector3(1f-marginX,1f-marginY,dPadDistance),
		new Vector3(marginX,marginY,dPadDistance),
		new Vector3(1f-marginX,marginY,dPadDistance)
		};
		marginX = .13f;
		marginY = .15f;			
		Vector3[] cursorStartPos = new Vector3[4]
		{
		new Vector3(marginX,1f-marginY,cursorDistance),
		new Vector3(1f-marginX,1f-marginY,cursorDistance),
		new Vector3(marginX,marginY,cursorDistance),
		new Vector3(1f-marginX,marginY,cursorDistance)
		};		
		
		cursorBoundBL = mainGuiCamera.ViewportToWorldPoint( new Vector3(0,0,cursorDistance) );
		cursorBoundUR = mainGuiCamera.ViewportToWorldPoint( new Vector3(1,1,cursorDistance) );			
		
		Vector3 pos;
		Quaternion rot = Quaternion.Euler(270,0,0);
		Quaternion rot2 = Quaternion.Euler(0,180,0);
		
		for (int i = 0; i < PlayerData.playerCount; i++)
		{
//			if (!PlayerData.singlePlayer)
//			{
//				if(!PlayerData.player[i].ai)
//				{
//					pos = mainGuiCamera.ViewportToWorldPoint(dPadStartPos[i]);
//					dPadInstance[i] = (GameObject)Instantiate(dPadPrefab,pos,rot);
//					dPadControl[i] = dPadInstance[i].GetComponent<DPadControl>();
//					dPadControl[i].Init(i);
//				}
//			}
//			
			pos = mainGuiCamera.ViewportToWorldPoint(cursorStartPos[i]);			
			cursorInstance[i] = (GameObject)Instantiate(cursorPrefab,pos,rot2);
			cursorMorph[i] = cursorInstance[i].GetComponent<MorphTargets>();
			cursorTransform[i] =  cursorInstance[i].transform;
			cursorScreenPosition[i] = projectGlobal.guiCamera[0].WorldToScreenPoint(cursorTransform[i].position);
			cursorLocalPosition[i] = cursorTransform[i].localPosition;
			cursorLocalEuler[i] = cursorTransform[i].localEulerAngles;
			cursorRenderer[i] = cursorInstance[i].GetComponent<Renderer>();
			cursorRenderer[i].materials[1].color = PlayerData.player[i].startColor;
		}
		
		marginX = 2;
		marginY = 2;		
		dPadEndPos = new Vector3[4]
		{
		new Vector3(dPadStartPos[0].x - marginX,dPadStartPos[0].y + marginY,30f),
		new Vector3(dPadStartPos[1].x + marginX,dPadStartPos[1].y + marginY,30f),
		new Vector3(dPadStartPos[2].x - marginX,dPadStartPos[2].y - marginY,30f),
		new Vector3(dPadStartPos[3].x + marginX,dPadStartPos[3].y - marginY,30f)
		};		
				
	}
	
	private int particeLayer = 1 << 8;	
	private float aiUpdateRate = 1f;
	private float aiUpdateTimer;
	private int aiUpdateIndex;
	private int[] aiIndeces;
	//private bool aiUpdatedForCycle;
	private void Update()
	{
		//if (aiUpdateTimer > aiUpdateRate - (aiUpdateRate/4) && !aiUpdatedForCycle )
		//{
		//	particleGridControl.aiUpdate = true;	
		//	aiUpdatedForCycle = true;
		//}
		
		if (particleGridControl.particlesSpawned && PlayerData.playerWinID == 255 )
		{
			for (int i = 0; i < PlayerData.playerCount; i++)
			{
//				if (PlayerData.player[i].particleCount == 0)
//				{
//					PlayerData.player[i].particleCount = -100;
//					if (dPadControl[i] != null)
//					{
//						dPadControl[i].MoveOffscreen( mainGuiCamera.ViewportToWorldPoint( dPadEndPos[i] ) );
//					}
//					StartCoroutine(LosingCursorCoroutine(i, mainGuiCamera.ViewportToWorldPoint( dPadEndPos[i] ) ) );			
//				}
			}
			
			if (aiUpdateTimer > aiUpdateRate)
			{
				if (PlayerData.player[aiIndeces[aiUpdateIndex]].particleCount != -100) //-100 signify player lost
					SelectRandomPointAI( aiIndeces[aiUpdateIndex] );
				aiUpdateTimer = 0;
				//aiUpdatedForCycle = false;
				
				aiUpdateIndex++;
				if(aiUpdateIndex >= aiIndeces.Length)
				{
					aiUpdateIndex = 0;	
				}
			}
			aiUpdateTimer += Time.deltaTime;
		}
			
		if (updateCursors)
		{
			UpdateCursors();	
		}
	}
	
	private int[] aiEnemyIndex = new int[4]{-1,-1,-1,-1};
	private int[] aiCycle;
	private void SelectRandomPointAI(int aiIndex)
	{
		//check if previously locked on enemy is now dead
		//if (aiEnemyIndex[aiIndex] != -1 && PlayerData.player[aiEnemyIndex[aiIndex]].particleCount == 0)
		//{
		//	aiEnemyIndex[aiIndex] = -1;	
		//}
		
		aiEnemyIndex[aiIndex] = -1; //makes it find new enemy everytime
		
		//if not locked on, find one randomly to lock onto
		if (aiEnemyIndex[aiIndex] == -1)
		{
			int startIndex = Random.Range(0,PlayerData.playerCount);
			
			for (int i = 0; i < PlayerData.playerCount; i++)
			{
				if( PlayerData.player[startIndex].particleCount != -100 && startIndex != aiIndex)
				{
					aiEnemyIndex[aiIndex] = startIndex;	
				}
				else
				{
					startIndex++;
					if (startIndex == PlayerData.playerCount)
						startIndex = 0;
				}
			}			
		}
		
		if (aiEnemyIndex[aiIndex] != -1)
		{
			//Debug.Log(aiEnemyIndex[aiIndex]+"  "+aiCycle[aiIndex]+"   "+aiIndex);
			Vector3 newWorldPos = particleGridControl.aiWorldPosition[aiEnemyIndex[aiIndex],aiCycle[aiIndex]];
			StartCoroutine(TweenCursor(aiIndex, mainCamera.WorldToScreenPoint(newWorldPos)));
		}
			
		aiCycle[aiIndex]++;
		if (aiCycle[aiIndex] == PlayerData.playerCount)
			aiCycle[aiIndex] = 0;
	
	}
	
	private IEnumerator TweenCursor(int index, Vector3 newScreenPos )
	{
		Vector3 oldScreenPos = cursorScreenPosition[index];
		
		float time = 0;
		
		while(time < 1)
		{
			if (PlayerData.player[index].particleCount != -100 && PlayerData.playerWinID == 255) //-100 signify player lost
			{
				time += Time.deltaTime * 1.1f;
				cursorScreenPosition[index] = Vector3.Lerp(oldScreenPos,newScreenPos,time);
				cursorTransform[index].position = mainGuiCamera.ScreenToWorldPoint(
					new Vector3(cursorScreenPosition[index].x,cursorScreenPosition[index].y,35));
				cursorLocalPosition[index] = cursorTransform[index].localPosition;
			}
			yield return null;
		}
		
	}
	
	
	private void UpdateCursors()
	{
		int i;
		int limit = PlayerData.playerCount;
		
		for (i = 0; i < limit; i++)
		{		
			if (PlayerData.player[i].particleCount != -100) //-100 playere lost
			{
				if (i == 0 && PlayerData.singlePlayer && inputControl.inputPhase[0] == 2)
				{		
					//code to deal with touch input on single player
					Vector3 inputPos;
					Ray inputRay;
					RaycastHit hitInfo;
		
					inputPos = inputControl.inputPos[0];
					inputRay = projectGlobal.mainCamera.ScreenPointToRay(inputPos);
					if(Physics.Raycast(inputRay, out hitInfo, projectGlobal.inputRaycastDistance,particeLayer))
					{
						cursorScreenPosition[0] = mainCamera.WorldToScreenPoint(hitInfo.point);
						cursorTransform[0].position = mainGuiCamera.ScreenToWorldPoint(
							new Vector3(cursorScreenPosition[0].x, cursorScreenPosition[0].y, 35) );
						cursorLocalPosition[0] = cursorTransform[0].localPosition;
					}
				}	
				else if (!PlayerData.player[i].ai)
				{
					//place cursors based on dPad updates
//					cursorTransform[i].localPosition = cursorLocalPosition[i];
//					cursorScreenPosition[i] = mainGuiCamera.WorldToScreenPoint(cursorTransform[i].localPosition);
				}
				
				cursorLocalEuler[i].z += cursorSpin[i] * Time.deltaTime;			
				cursorTransform[i].localEulerAngles = cursorLocalEuler[i]; 
				
				if (cursorSpin[i] > 0)
				{
					cursorSpin[i] -= Time.deltaTime / 20f;
					if (cursorSpin[i] < 0)
						cursorSpin[i] = 0;
				}
				else if (cursorSpin[i] < 0)
				{
					cursorSpin[i] += Time.deltaTime / 20f;
					if (cursorSpin[i] > 0)
						cursorSpin[i] = 0;
				}
				
				if(	cursorMorph[i].attributeProgress[0] != PlayerData.player[i].particlePercentage)
				{
					cursorMorph[i].attributeProgress[0] = PlayerData.player[i].particlePercentage;
					cursorMorph[i].SetMorph();
				}
			}
		}
	}
	
	Vector3[] dPadEndPos;
	public void BeginWinSequence(int playerID)
	{
		updateCursors = false;
		
		StartCoroutine(WinningCursorCoroutine(playerID));
		cursorSpin[playerID] = .5f;
		cursorMorph[playerID].attributeProgress[0] = 1;
		cursorMorph[playerID].SetMorph();	
		
//		for (int i = 0; i < dPadControl.Length; i++)
//		{			
//			if (PlayerData.player[i].particleCount != 0)
//			{
//				if (dPadControl[i] != null)
//				{
//					dPadControl[i].MoveOffscreen( mainGuiCamera.ViewportToWorldPoint( dPadEndPos[i] ) );
//				}
//				if (i != playerID)
//				{
//					StartCoroutine(LosingCursorCoroutine(i, mainGuiCamera.ViewportToWorldPoint( dPadEndPos[i] ) ) );
//				}
//			}
//		}
	}
	private IEnumerator WinningCursorCoroutine(int playerID)
	{
		Vector3 startPos = cursorTransform[playerID].position;
		Vector3 endPos = mainGuiCamera.ViewportToWorldPoint( new Vector3(.5f,.5f,5f) );
		
		Vector3 startEuler = cursorTransform[playerID].localEulerAngles;
		startEuler.z = startEuler.z % 360;
		Vector3 endEuler = new Vector3(startEuler.x,startEuler.y,360 * 4);
		
		float time = 0.0f;
		float easedTime = 0.0f;
		while (time < 1.0f)
		{
			time += Time.deltaTime ;
			easedTime = projectGlobal.easeInEaseOut.Evaluate(time);
			cursorTransform[playerID].position = Vector3.Lerp(startPos,endPos,easedTime);	
			cursorTransform[playerID].localEulerAngles = Vector3.Lerp(startEuler,endEuler,easedTime);
			yield return null;
		}		
	}
	private IEnumerator LosingCursorCoroutine(int playerID, Vector3 endPos)
	{
		Vector3 startPos = cursorTransform[playerID].position;
		
		Vector3 startEuler = cursorTransform[playerID].localEulerAngles;
		startEuler.z = startEuler.z % 360;
		Vector3 endEuler = new Vector3(startEuler.x,startEuler.y,360 * 4);
		
		float time = 0.0f;
		float easedTime = 0.0f;
		while (time < 1.0f)
		{
			time += Time.deltaTime / 4f;
			easedTime = projectGlobal.easeInEaseOut.Evaluate(time);
			cursorTransform[playerID].position = Vector3.Lerp(startPos,endPos,easedTime);	
			cursorTransform[playerID].localEulerAngles = Vector3.Lerp(startEuler,endEuler,easedTime);
			yield return null;
		}			
		
	}

}
