//using UnityEngine;
//using System.Collections;
//
//public class PlayerWheelRaycast : RaycastTouch 
//{
//	private GameObject thisGameObject;
//	private Renderer thisRenderer;
//	private Transform thisTransform;
//	
//	public float scrollDragMultiplier = 1.0f;
//	private Material scrollMaterial;
//	private Vector2 lastDragRatio;
//	[HideInInspector]
//	public Vector2 scrollOffset;	
//	
//	public GameObject playerMenuLabelPrefab;
//	public GameObject aiMenuLabelPrefab;
//	public GameObject removeButtonPrefab;
//	
//	public bool addRobot;
//	public bool addHuman;
//	
//	private const int maxPlayerCount = 4;		
//	private bool[] playerAI = new bool[maxPlayerCount];
//	private int playerMenuLabelCount;
//	
//	public string wheelName;
//	
//	private void Awake ()
//	{
//		//PlayerPrefs.DeleteAll();
//		thisGameObject = this.gameObject;
//		Debug.Log("found "+thisGameObject+" from "+this);		
//		thisRenderer = this.renderer;
//		thisTransform = this.transform;		
//		
//		if (thisRenderer != null)
//		{
//			Material[] materials = thisRenderer.sharedMaterials;
//		
//			for (int i = 0; i < materials.Length; i++)
//			{
//				if (materials[i].name.Contains("Scroll"))
//				{				
//					scrollMaterial = thisRenderer.materials[i];
//				}
//			}
//		}
//		
//		playerMenuLabel = new GameObject[4];
//		playerMenuLabelScript = new PlayerMenuLabel[4];	
//		playerMenuLabelTransform = new Transform[4];	
//		removePlayerButton = new GameObject[4];
//		removePlayerButtonTransform = new Transform[4];
//		removePlayerButtonScript = new RaycastButton[4];
//		playerMenuLabelCount = 0;
//	}
//		
//	private void Start ()
//	{
//		if (PlayerPrefs.HasKey(wheelName+"PlayerMenuLabelCount"))
//		{
//			scrollOffset.x = PlayerPrefs.GetFloat(wheelName+"ScrollOffsetX");
//			scrollMaterial.SetTextureOffset("_MainTex", scrollOffset);				
//			
//			int limit = LoadPlayerPref();	
//			for (int i = 0; i <  limit; i++)
//			{			
//				AddPlayer(playerAI[i]);
//			}		
//		}
//		else
//		{
//			if( addRobot)
//			{
//				AddPlayer(false);
//				AddPlayer(true);
//				AddPlayer(true);
//				AddPlayer(true);	
//			}
//			if( addHuman )
//			{
//				AddPlayer(false);
//				AddPlayer(false);
//				AddPlayer(false);
//				AddPlayer(false);	
//			}
//		}
//	}
//	
//	public override void DoOnDown(Vector3 inputPos, Vector3 raycastHitPoint)
//	{
//		lastDragRatio = Utility.ScreenInputToProportionalRatio( inputPos );
//	}
//	
//	public override void DoOnHold(Vector3 inputPos, Vector3 raycastHitPoint, bool hittingTouchzone)
//	{
//		Vector2 inputRatio = Utility.ScreenInputToProportionalRatio( inputPos );
//		scrollOffset.x += ( lastDragRatio.x - inputRatio.x ) * scrollDragMultiplier;
//		scrollMaterial.SetTextureOffset("_MainTex", scrollOffset);	
//		lastDragRatio = inputRatio;
//	}
//	
//
//	public override void DoOnUp(Vector3 inputPos, Vector3 raycastHitPoint)
//	{
//		PlayerPrefs.SetFloat(wheelName+"ScrollOffsetX",scrollOffset.x);
//	}	
//	
//	private void Update()
//	{	
//		PlayerData.playerCount = playerMenuLabelCount;
//		for (int i = 0; i < maxPlayerCount; i++)
//		{			
//			PlayerData.player[i].ai = playerAI[i];
//		}
//	}
//
//	private GameObject[] removePlayerButton;
//	private RaycastButton[] removePlayerButtonScript;	
//	private Transform[] removePlayerButtonTransform;	
//	
//	private GameObject[] playerMenuLabel;
//	private PlayerMenuLabel[] playerMenuLabelScript;
//	private Transform[] playerMenuLabelTransform;	
//	private GameObject[] playerMenuLabelGameObject;		
//	
//	private const float playerMenuLabelRadius = 2.2f;
//	public void AddPlayer(bool ai)
//	{	
//		//Debug.Log("Adding Player, is AI: "+ai);
//		if (playerMenuLabelCount < maxPlayerCount)
//		{
//			int newPlayerIndex = playerMenuLabelCount;
//			playerMenuLabelCount++;
//			
//			if (ai)
//			{
//				playerMenuLabel[newPlayerIndex] = (GameObject)Instantiate(aiMenuLabelPrefab);
//			}
//			else
//			{
//				playerMenuLabel[newPlayerIndex] = (GameObject)Instantiate(playerMenuLabelPrefab);				
//			}
//			
//			playerMenuLabelScript[newPlayerIndex] = playerMenuLabel[newPlayerIndex].GetComponent<PlayerMenuLabel>();
//			playerMenuLabelScript[newPlayerIndex].playerID = newPlayerIndex;
//			
//			playerMenuLabelTransform[newPlayerIndex] = playerMenuLabel[newPlayerIndex].transform;
//			playerMenuLabelTransform[newPlayerIndex].parent = thisTransform;
//	
//			playerAI[newPlayerIndex] = ai;			
//			
//			Vector2 pos;
//			float distance = playerMenuLabelRadius;
//			float rotation = 360/playerMenuLabelCount;
//			
//			for (int i = 0; i < playerMenuLabelCount; i++)
//			{				
//				pos.x = distance * Mathf.Cos(Mathf.Deg2Rad*(rotation*i));
//				pos.y = distance * Mathf.Sin(Mathf.Deg2Rad*(rotation*i));
//				
//				//Debug.Log(playerMenuLabelTransform[i]);
//				playerMenuLabelTransform[i].localPosition = new Vector3(-.2f,pos.x,pos.y);
//				
//				playerMenuLabelTransform[i].rotation = Quaternion.identity;
//				int xRot = 90+(int)(rotation*i);
//				playerMenuLabelTransform[i].localEulerAngles = new Vector3(xRot,0,0);
//			}
//			
//			if(newPlayerIndex != 0)
//			{
//				removePlayerButton[newPlayerIndex] = (GameObject)Instantiate(removeButtonPrefab);
//				
//				removePlayerButtonTransform[newPlayerIndex] = removePlayerButton[newPlayerIndex].transform;
//				removePlayerButtonTransform[newPlayerIndex].parent = thisTransform;
//				removePlayerButtonTransform[newPlayerIndex].localEulerAngles = new Vector3(0,0,0);	
//				removePlayerButtonScript[newPlayerIndex] = removePlayerButton[newPlayerIndex].GetComponent<RaycastButton>();
//				removePlayerButtonScript[newPlayerIndex].playerWheelRaycastParent = this;
//				removePlayerButtonScript[newPlayerIndex].removePlayerMenuLabelIndex = newPlayerIndex;
//				
//				distance = 4.7f;
//				rotation = 360/playerMenuLabelCount;
//				
//				for (int i = 1; i < playerMenuLabelCount; i++)
//				{
//					pos.x = distance * Mathf.Cos(Mathf.Deg2Rad*(rotation*i));
//					pos.y = distance * Mathf.Sin(Mathf.Deg2Rad*(rotation*i));
//					
//					removePlayerButtonTransform[i].localPosition = new Vector3(-.2f,pos.x,pos.y);
//				}		
//			}
//			
//			SetPlayerPref();
//		}
//	}	
//	
//	public void RemovePlayer(int removeIndex)
//	{	
//		if (playerMenuLabelCount > 1)
//		{
//			
//			playerMenuLabelCount--;
//			Destroy(playerMenuLabel[removeIndex]);
//			Destroy(removePlayerButton[removeIndex]);
//			
//			for (int i = removeIndex; i < playerMenuLabelCount; i++)
//			{
//				playerMenuLabel[i] = playerMenuLabel[i+1];
//				playerMenuLabelScript[i] = playerMenuLabelScript[i+1];
//				playerMenuLabelTransform[i] = playerMenuLabelTransform[i+1];
//				
//				removePlayerButton[i] = removePlayerButton[i+1];
//				removePlayerButtonTransform[i] = removePlayerButtonTransform[i+1];
//				removePlayerButtonScript[i] = removePlayerButtonScript[i+1];
//				
//				playerMenuLabelScript[i].playerID--;
//				removePlayerButtonScript[i].removePlayerMenuLabelIndex--;
//				
//				playerAI[i] = playerAI[i+1]; 
//			}
//			
//			
//			Vector2 pos;
//			float distance = playerMenuLabelRadius;
//			float rotation = 360/playerMenuLabelCount;
//			
//			for (int i = 0; i < playerMenuLabelCount; i++)
//			{
//				pos.x = distance * Mathf.Cos(Mathf.Deg2Rad*(rotation*i));
//				pos.y = distance * Mathf.Sin(Mathf.Deg2Rad*(rotation*i));
//				
//				playerMenuLabelTransform[i].localPosition = new Vector3(-.2f,pos.x,pos.y);
//				
//				int xRot = 90+(int)(rotation*i);
//				playerMenuLabelTransform[i].localEulerAngles = new Vector3(xRot,0,0);			
//			}
//			
//			distance = 4.7f;
//			rotation = 360/playerMenuLabelCount;
//			
//			for (int i = 1; i < playerMenuLabelCount; i++)
//			{
//				pos.x = distance * Mathf.Cos(Mathf.Deg2Rad*(rotation*i));
//				pos.y = distance * Mathf.Sin(Mathf.Deg2Rad*(rotation*i));
//				
//				removePlayerButtonTransform[i].localPosition = new Vector3(-.2f,pos.x,pos.y);
//			}	
//			
//			SetPlayerPref();			
//		}
//	}	
//	
//	private void SetPlayerPref()
//	{
//		Debug.Log("Saving player prefs on: "+wheelName);
//		for (int i = 0; i < maxPlayerCount; i++)
//		{			
//			if (playerAI[i])
//				PlayerPrefs.SetInt(wheelName+"Player"+i,2);
//			else
//				PlayerPrefs.SetInt(wheelName+"Player"+i,1);	
//		}	
//		PlayerPrefs.SetInt(wheelName+"PlayerMenuLabelCount",playerMenuLabelCount);		
//	}
//	
//	
//	private int LoadPlayerPref()
//	{
//		Debug.Log("Loading player prefs on: "+wheelName);		
//		for (int i = 0; i < maxPlayerCount; i++)
//		{			
//			if (PlayerPrefs.GetInt(wheelName+"Player"+i) == 2)
//				playerAI[i] = true;
//			else if (PlayerPrefs.GetInt(wheelName+"Player"+i) == 1)
//				playerAI[i] = false;
//			//Debug.Log("is ai: "+playerAI[i]);
//		}
//		return PlayerPrefs.GetInt(wheelName+"PlayerMenuLabelCount");
//	}	
//}
