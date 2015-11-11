using UnityEngine;
using System.Collections;

public class SystemSettings : MonoBehaviour 
{
	private void Awake()
	{
		Application.targetFrameRate = 60;
	}
}
