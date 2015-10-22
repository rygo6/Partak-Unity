//using UnityEngine;
//using System.Collections;
//
//public class GUIControl : MonoBehaviour 
//{
//	//static accesors
//	[HideInInspector]
//	public GUIControl staticThis;
//	private InputControl inputControl;
//	private ProjectGlobal projectGlobal;
//
//	//Adjustables
//	public bool includeMainCamera = true;
//	public int raycastDisance = 100;
//	
//	//internals
//	private Camera[] guiCamera;
//	private int raycastTouchLayer = (1 << 31) | (1 << 29);		
//	private RaycastTouch[] raycastTouchStore;
//	
//	//stores if anything is in progress, and stops input if there is
//	static public int taskInProgress = 0;	
//	
//	public GameObject cursorParticlePrefab;
//	private ParticleSystem[] cursorParticle;
//	private Transform[] cursorParticleTransform;	
//	private Vector3[] raycastHitPoint;
//	private float[] particleEmitTimer;	
//	private float[] particleEmitStopDelay;
//	private float particleEmitRate = .15f;
//
//	
//	void Awake()
//	{		
//		staticThis =  this;
//		raycastTouchStore = new RaycastTouch[ InputControl.fingerCount ];
//
//		raycastHitPoint = new Vector3[ InputControl.fingerCount ];
//		
//		if (cursorParticlePrefab != null)
//		{
//			cursorParticle = new ParticleSystem[ InputControl.fingerCount ];
//			cursorParticleTransform = new Transform[ InputControl.fingerCount ];
//			particleEmitTimer = new float[ InputControl.fingerCount ];
//			particleEmitStopDelay = new float[ InputControl.fingerCount ];	
//			
//			GameObject temp;
//			for (int i = 0; i < cursorParticle.Length; i++)
//			{
//				particleEmitTimer[i] = 100f;
//				temp  = (GameObject)Instantiate(cursorParticlePrefab,Vector3.zero,Quaternion.identity);	
//				cursorParticle[i] = temp.GetComponent<ParticleSystem>();
//				cursorParticleTransform[i] = cursorParticle[i].transform;
//				cursorParticleTransform[i].renderer.material.renderQueue = 5000;
//			}
//		}
//	}
//	
//	void Start () 
//	{
//		inputControl = InputControl.staticThis;
//		projectGlobal = ProjectGlobal.staticThis;
//	
//		if (includeMainCamera)
//		{
//			guiCamera = new Camera[projectGlobal.guiCamera.Length+1];
//			guiCamera[0] = projectGlobal.mainCamera;
//			for (int i = 0; i < projectGlobal.guiCamera.Length; i++)
//			{
//				guiCamera[i+1] = projectGlobal.guiCamera[i];	
//			}
//		}
//		else
//		{
//			guiCamera = new Camera[projectGlobal.guiCamera.Length];
//			for (int i = 0; i < projectGlobal.guiCamera.Length; i++)
//			{
//				guiCamera[i] = projectGlobal.guiCamera[i];	
//			}			
//		}
//	}
//	
//	private void Update ()
//	{
//		if (taskInProgress == 0)
//		{
//			RaycastTouches();	
//		}
//
//		if (cursorParticlePrefab != null)
//		{
//			int i2;
//			int limit2 = InputControl.fingerCount;		
//			for (i2 = 0; i2 < limit2; i2++)
//			{		
//				if (particleEmitStopDelay[i2] > 0)
//				{
//					particleEmitTimer[i2] += Time.deltaTime;
//					if (particleEmitTimer[i2] > particleEmitRate)
//					{
//						particleEmitTimer[i2] = 0;
//						cursorParticle[i2].Emit(1);
//					}
//					
//					//Debug.Log(particleEmitStopDelay[i2]);
//					particleEmitStopDelay[i2] -= Time.deltaTime;	
//					if (particleEmitStopDelay[i2] <= 0)
//					{
//						particleEmitTimer[i2] = 0;
//					}
//				}
//			}
//		}
//	}
//	
//
//	
//	
//	private void RaycastTouches () 
//	{
//		//recyclables
//		int i;
//		int i2;
//		int limit;
//		int limit2;
//		RaycastHit raycastHit;
//		Ray ray; 
//
//		limit = guiCamera.Length;
//		limit2 = InputControl.fingerCount;		
//		for (i = 0; i < limit; i++)
//		{
//			for (i2 = 0; i2 < limit2; i2++)
//			{	
//				if (inputControl.inputPosUniqueID[i2] != -1)	
//				{				
//					ray = guiCamera[i].ScreenPointToRay( inputControl.inputPos[i2] );						
//					
//					if (Physics.Raycast(ray,out raycastHit,raycastDisance,raycastTouchLayer))
//					{								
//						raycastHitPoint[i2] = raycastHit.point;
//						if (cursorParticlePrefab != null)
//						{
//							cursorParticleTransform[i2].position = raycastHitPoint[i2];
//							particleEmitStopDelay[i2] = .2f;
//						}
//						
//						if (inputControl.inputPhase[i2] == 1)
//						{
//							//Debug.Log("Input "+i2+" Phase 1");
//							raycastTouchStore[i2] = raycastHit.transform.GetComponent<RaycastTouch>();
//							if (raycastTouchStore[i2] != null)
//								raycastTouchStore[i2].DoOnDown(inputControl.inputPos[i2], raycastHitPoint[i2] );
//						}
//						else if (inputControl.inputPhase[i2] == 2 && raycastTouchStore[i2] != null)
//						{
//							//Debug.Log("Input "+i2+" Phase 2");							
//							raycastTouchStore[i2].DoOnHold(inputControl.inputPos[i2], raycastHitPoint[i2], true );
//						}
//						else if (inputControl.inputPhase[i2] == 3 && raycastTouchStore[i2] != null)
//						{
//							raycastTouchStore[i2].DoOnUp(inputControl.inputPos[i2], raycastHitPoint[i2]);
//							raycastTouchStore[i2].inputDragAway = false;
//							raycastTouchStore[i2] = null;
//						}
//					}
//					else
//					{	
//						if(raycastTouchStore[i2] != null)
//						{
//							raycastTouchStore[i2].inputDragAway = true;
//							if (inputControl.inputPhase[i2] == 2)
//							{					
//								raycastTouchStore[i2].DoOnHold(inputControl.inputPos[i2], raycastHitPoint[i2], false );
//							}	
//							else if (inputControl.inputPhase[i2] == 3)
//							{
//								raycastTouchStore[i2].DoOnUp(inputControl.inputPos[i2], raycastHitPoint[i2] );								
//								raycastTouchStore[i2].inputDragAway = false;
//								raycastTouchStore[i2] = null;
//							}							
//						}
//					}
//				}
//				
//				
//			}				
//		}
//	}
//
//	
//}
