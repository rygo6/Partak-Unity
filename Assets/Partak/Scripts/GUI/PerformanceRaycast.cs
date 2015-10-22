//using UnityEngine;
//using System.Collections;
//
//public class PerformanceRaycast : RaycastTouch 
//{
//	private GameObject thisGameObject;
//	private Transform thisTransform;
//	private Collider thisCollider;
//	
//	public GameObject redDot;
//	private Transform redDotTransform;
//	
//	private int particleCount;
//	private float gameSpeed;
//	
//	private const float minGameSpeed = 1.0f;
//	private const float maxGameSpeed = 4.25f;
//	
//	private const float minParticleCount = 2000f;
//	private const float maxParticleCount = 8000f;	
//	
//	public AnimationCurve performanceCurve = new AnimationCurve(
//		new Keyframe(2000f,4.25f),
//		new Keyframe(3000f,2.8f),
//		new Keyframe(3500f,2.4f),			
//		new Keyframe(4000f,2.0f),
//		new Keyframe(5000f,1.6f),		
//		new Keyframe(6000f,1.4f),		
//		new Keyframe(7000f,1.2f),
//		new Keyframe(8000f,1.0f)		
//		);
//
//	void Start () 
//	{
//		thisGameObject = this.gameObject;
//		Debug.Log("found "+thisGameObject+" from "+this);
//		thisTransform = this.transform;
//		Debug.Log("found "+thisTransform+" from "+this);		
//		thisCollider = this.collider;
//	
//		redDotTransform = redDot.transform;
//		if (PlayerPrefs.HasKey("PerformanceRaycastZ"))
//		{
//			Debug.Log("Loading PlayerPrefs on PeformanceRaycast");
//			Vector3 newPos = redDotTransform.position;
//			newPos.z = PlayerPrefs.GetFloat("PerformanceRaycastZ");
//			newPos.y = PlayerPrefs.GetFloat("PerformanceRaycastY");
//			Debug.Log(newPos.z);
//			redDotTransform.position = newPos;
//		}
//		redDotNewPos = redDotTransform.position;	
//		Update();
//	}
//	
//
//	void Update() 
//	{		
//		float xStart = thisCollider.bounds.center.z-thisCollider.bounds.extents.z;
//		float xEnd = thisCollider.bounds.center.z+thisCollider.bounds.extents.z;
//		
//		float xRatio = ( (redDotNewPos.z-xStart) - (xEnd-redDotNewPos.z) ) / (xEnd-xStart);
//		xRatio = (xRatio+1f)/2f;
//		
//		float yStart = thisCollider.bounds.center.y-thisCollider.bounds.extents.y;
//		float yEnd = thisCollider.bounds.center.y+thisCollider.bounds.extents.y;
//		
//		float yRatio = ( (redDotNewPos.y-yStart) - (yEnd-redDotNewPos.y) ) / (yEnd-yStart);
//		yRatio = (yRatio+1f)/2f;
//		
//		particleCount = (int)(xRatio * (maxParticleCount-minParticleCount) + minParticleCount);
//		if(particleCount % 2 != 0)
//		{	
//			particleCount++;
//		}	
//		//Debug.Log(yRatio);
//		gameSpeed = (yRatio * (maxGameSpeed-minGameSpeed))+1f;
//		
//		//Debug.Log(gameSpeed);
//		
//		if (gameSpeed > performanceCurve.Evaluate(particleCount))
//		{
//			gameSpeed = performanceCurve.Evaluate(particleCount);
//			
//			Vector3 newPos = redDotNewPos;
//			//formulas found by the MULTIVARIABLE ALGEBRA SOLVING
//			yRatio = 1f - ((maxGameSpeed-gameSpeed)/(maxGameSpeed-minGameSpeed));
//			newPos.y = (yEnd - yStart) * yRatio + yStart;
//			redDotTransform.position = newPos;
//			
//		}
//		else
//		{
//			redDotTransform.position =  redDotNewPos;
//		}
//		
//		Time.fixedDeltaTime = 0.03333333333f / gameSpeed;
//		PlayerData.startParticleCount = particleCount;
//	}
//	
//	
//	public override void DoOnDown (Vector3 inputPos, Vector3 raycastHitPoint)
//	{
//
//	}
//	
//	private Vector3 redDotNewPos;
//	public override void DoOnHold (Vector3 inputPos, Vector3 raycastHitPoint, bool hittingTouchzone)
//	{
//		if (hittingTouchzone)
//		{
//			redDotNewPos.z = raycastHitPoint.z;
//			redDotNewPos.y = raycastHitPoint.y;
//		}	
//	}
//	
//
//	public override void DoOnUp (Vector3 inputPos, Vector3 raycastHitPoint)
//	{	
//		PlayerPrefs.SetFloat("PerformanceRaycastZ",redDotNewPos.z);
//		PlayerPrefs.SetFloat("PerformanceRaycastY",redDotNewPos.y);		
//	}		
//	
//}
