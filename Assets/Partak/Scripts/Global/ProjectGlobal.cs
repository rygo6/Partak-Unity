using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProjectGlobal : MonoBehaviour 
{
	static public ProjectGlobal staticThis;
	static public bool isFullVersion = false;
	
	public const int FullVersionOnlyLevelCount = 3;	
	public const int levelCount = 18;	
	
	public Camera mainCamera;
	public Camera[] guiCamera;
	
	public Animation mainGUIAnimation;
	
	public AnimationCurve easeInEaseOut;
	
	public float inputRaycastDistance = 100f;	
	
	public int targetFrameRate = 30;
	
	void Awake()
	{		
		
		Application.targetFrameRate = targetFrameRate;
		
		Input.gyro.enabled = true;
		Input.gyro.enabled = false;
		
		if (PlayerPrefs.HasKey("isFullVersion"))
		{
			isFullVersion = true;	
		}
		
		staticThis = this;	
		
		if(PlayerData.player == null)
		{
			PlayerData.initializePlayerArray();	
		}
		
		//reset necessary playerdata
		PlayerData.playerWinID = 255;		
		PlayerData.totalParticleCount = 0;
		for (int i = 0; i < PlayerData.player.Length; i++)
		{
			PlayerData.player[i].particleCount = 0;
			PlayerData.player[i].startParticleCount = 0;
			PlayerData.player[i].particlePercentage = 0;
		}		
		
		
	}
	
	public void BeginWinSequence(int playerID)
	{
		PauseButton.staticThis.DisablePauseButtons();
		PlayerData.playerWinID = playerID;
//		ParticleGridControl.staticThis.moveChaotic = true;		
		CursorManager.staticThis.BeginWinSequence(playerID);
		CameraOrbit.staticThis.BeginWinSequence();

//		ParticleGridControl.staticThis.EndThreads();
	}	
	
	private void OnApplicationQuit()
	{
//		if (ParticleGridControl.staticThis != null)
//			ParticleGridControl.staticThis.EndThreads();
	}
}
