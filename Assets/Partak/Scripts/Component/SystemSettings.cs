using UnityEngine;
using System.Collections;

public class SystemSettings : MonoBehaviour 
{
	public bool FullVersion { get; set; }

	private void Awake()
	{
		if (PlayerPrefs.HasKey("isFullVersion"))
		{
			FullVersion = true;	
		}
	}
}
