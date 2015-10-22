using UnityEngine;
using System.Collections;

public class MusicControl : MonoBehaviour 
{
	private ParticleGridControl particleGridControl;
	
	private AudioClip[,] music;
	
	private AudioSource[,] audioSource;
	
	private int[] playingClip;
	
	private GameObject thisGameobject;
	
	private int[] particleInterval;
	
	private int pauseIndex;
	
	void Start () 
	{
		thisGameobject = this.gameObject;
		
		music = new AudioClip[PlayerData.playerCount,3];
		audioSource = new AudioSource[PlayerData.playerCount,3];
		playingClip = new int[PlayerData.playerCount];
		int rand;
		
		for (int i = 0; i < PlayerData.playerCount; i++)
		{
			rand = Random.Range(0,4);
			//Debug.Log(rand);
			for (int i2 = 0; i2 < 3; i2++)
			{
				//Debug.Log("Music/"+rand+"-"+i+"-"+i2);
				music[i,i2] = (AudioClip)Resources.Load("Music/"+rand+"-"+i+"-"+i2);	
				audioSource[i,i2] = (AudioSource)thisGameobject.AddComponent<AudioSource>();
				audioSource[i,i2].clip = music[i,i2];
				audioSource[i,i2].Play();
				audioSource[i,i2].volume = 0;
				audioSource[i,i2].loop = true;
			}

			playingClip[i] = -1;
		}
		
		particleInterval = new int[3];
		particleInterval[0] = PlayerData.startParticleCount - (PlayerData.startParticleCount/3);
		particleInterval[1] = PlayerData.startParticleCount - (PlayerData.startParticleCount/8);
		particleInterval[2] = PlayerData.startParticleCount;
		Debug.Log(particleInterval[0]+"   "+particleInterval[1]+"   "+particleInterval[2]);
	}
	
	private bool resetSounds = true;
	void Update () 
	{
		
		if (particleGridControl.particlesSpawned)
		{
			int i;
			int limit = PlayerData.playerCount;
			
			if (resetSounds)
			{		
				resetSounds = false;
				for (i = 0; i < limit; i++)
				{
					audioSource[i,0].mute = true;				
					audioSource[i,1].mute = true;
					audioSource[i,2].mute = true;					
					playingClip[i] = -1;	
				}
			}
			
			for (i = 0; i < limit; i++)
			{
				if (PlayerData.player[i].particleCount == -100)
				{
					audioSource[i,0].mute = true;				
					audioSource[i,1].mute = true;
					audioSource[i,2].mute = true;					
				}
				else if (playingClip[i] != 0 && PlayerData.player[i].particleCount > 0 && PlayerData.player[i].particleCount < particleInterval[0])
				{
					//Debug.Log(i+" playing 0");
					audioSource[i,0].volume = .5f;
					audioSource[i,0].mute = false;				
					audioSource[i,1].mute = true;
					audioSource[i,2].mute = true;					
					playingClip[i] = 0;
				}
				else if (playingClip[i] != 1 && PlayerData.player[i].particleCount > particleInterval[0] && PlayerData.player[i].particleCount < particleInterval[1])
				{
					//Debug.Log(i+" playing 1");	
					audioSource[i,0].mute = true;
					audioSource[i,1].volume = .4f;
					audioSource[i,1].mute = false;				
					audioSource[i,2].mute = true;	
					playingClip[i] = 1;
				}	
				else if (playingClip[i] != 2 && PlayerData.player[i].particleCount > particleInterval[1] && PlayerData.player[i].particleCount < particleInterval[2])
				{
					//Debug.Log(i+" playing 2");	
					audioSource[i,0].mute = true;
					audioSource[i,1].mute = true;
					audioSource[i,2].volume = .4f;	
					audioSource[i,2].mute = false;				
					playingClip[i] = 2;
				}	
			}
		}
		else
		{
			int i;
			int limit = PlayerData.playerCount;
			
			for (i = 0; i < limit; i++)
			{
				if (particleGridControl.currentParticleSpawnPlayer == i)
				{
					audioSource[i,2].volume = .5f;
					audioSource[i,0].mute = true;				
					audioSource[i,1].mute = true;
					audioSource[i,2].mute = false;					
					playingClip[i] = 0;
				}
				else
				{
					audioSource[i,0].mute = true;				
					audioSource[i,1].mute = true;
					audioSource[i,2].mute = true;					
					playingClip[i] = -1;					
				}
			}			
			
		}
		


	}
}
