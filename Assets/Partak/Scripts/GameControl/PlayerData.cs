using UnityEngine;
using System.Collections;

public class PlayerData 
{
	static public int playerCount = 4;


	static public int startParticleCount = 200;
	static public float totalParticleCount;
	static public int playerWinID = 255;
	static public bool singlePlayer = false;
	static public string selectedLevelName;
	
	public class Player
	{		
		public bool ai;
		public string name;
		public Color32 startColor;
		public int particleCount;	
		public int startParticleCount;
		public float particlePercentage;
		public int spawnPointX;
		public int spawnPointY;		
	}
	static public Player[] player;

	
	static public void initializePlayerArray()
	{
		player = new Player[playerCount];	
		
		for (int i = 0; i < player.Length; i++)
		{
			player[i] = new Player();
			player[i].startColor = new Color(Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f), Random.Range(0.0f,1.0f));	
		}
		
		selectedLevelName = Application.loadedLevelName;
		
		//player[0].ai = true;		
//		player[1].ai = true;
//		player[2].ai = true;
//		player[3].ai = true;
//		
		Debug.Log("PlayerData Initialized");
	}

}
