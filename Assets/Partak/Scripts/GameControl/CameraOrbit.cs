using UnityEngine;
using System.Collections;

public class CameraOrbit : MonoBehaviour 
{
	public static CameraOrbit staticThis;
	
	public float rotateMultiplier = 4f;
	public float tweenMainCameraDivider = 8f;
	
	private Transform thisTransform;
	[HideInInspector]
	public Transform childCameraTransform;
	
	ProjectGlobal projectGlobal;
	
	void Awake()
	{
		staticThis = this;	
	}
	
	void Start () 
	{
		projectGlobal = ProjectGlobal.staticThis;
		thisTransform = this.transform;
		childCameraTransform = thisTransform.GetComponentsInChildren<Transform>()[1];
	}
	
	public void BeginWinSequence()
	{
		StartCoroutine(TweenMainCameraToOrbit());
		StartCoroutine(BeginOrbit());
	}
	private IEnumerator TweenMainCameraToOrbit()
	{
		Vector3 startPos = projectGlobal.mainCamera.transform.position;
		Quaternion startRot = projectGlobal.mainCamera.transform.rotation;
		
		float time = 0.0f;
		float easedTime = 0.0f;
		while (time < 1.0f)
		{
			time += Time.deltaTime / tweenMainCameraDivider;
			easedTime = projectGlobal.easeInEaseOut.Evaluate(time);
			projectGlobal.mainCamera.transform.position = Vector3.Lerp(startPos,childCameraTransform.position,easedTime);	
			projectGlobal.mainCamera.transform.rotation = Quaternion.Slerp(startRot,childCameraTransform.rotation,easedTime);
			yield return null;
		}	
		
		while (true)
		{
			projectGlobal.mainCamera.transform.position = childCameraTransform.position;
			projectGlobal.mainCamera.transform.rotation = childCameraTransform.rotation;	
			yield return null;
		}
	}
	private IEnumerator BeginOrbit()
	{
		while (true)
		{
			Vector3 newEuler = thisTransform.localEulerAngles;
			newEuler.y += Time.deltaTime * rotateMultiplier;
			thisTransform.localEulerAngles = newEuler;	
			yield return null;
		}
	}
}
