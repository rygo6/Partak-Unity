/// <summary>
/// Author: Ryan Goodrich
/// Central system which controls particle liquid simulation.
/// Good luck figuring this out again, you were completely 
/// nuts when you came up with this thing and will make no
/// sense again outside of that state of mind.
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class ParticleGridControl : MonoBehaviour 
{
	
	#region Properties;
	
	[System.Serializable]
	public class GridParameters
	{
		public int width;
		public int height;
		[HideInInspector]
		public int length;
	}
	public GridParameters gridParameters;
	
		
	public int attackMultiplier = 1;
	
	public Material surroundMaterial;
	public Color surroundEndColor; // color that surrounding will ping pong to when game time is greater than limit
	public float gameTimeLimit = 60f;
	
	private GridGroup[] gridGroup1;	
	private GridGroup[] gridGroup4;	
	private GridGroup[] gridGroup16;	
	private GridGroup[] gridGroupAll;
	
	#endregion
	
	
	private class GridGroup
	{
		//store index of each cell in each direction, uses direction sbyte as input for array
		public int[] directionalGroupIndex;	
		
		//store averaged position of gridGroup
		public Vector3 worldPosition;
		
		//contains these grid indexes
		public int[] gridContainIndex;
		
		//contains these grid group indexes
		public int[] gridGroupContainIndex;		
		
		//what group this belongs to, -1 if belongs to none
		public int gridGroupBelongToIndex;
		
		//stores how many of each player is contained in group
		public byte[] playerContainCount;
		
		//keeps tab on which cell has had its gradient calculated
		public bool setForCycle;
		
		//averaged color
		public Color32 color = new Color32(10,10,10,255);
	}

	private class Grid
	{
		public void GetValuesGrid(int playerID,
			out int directionPrimary,
			out int[] directionalIndex,
			out int gridBelongTo16Group, 
			out int gridBelongTo4Group,
			out Vector3 gridWorldPosition)
		{
			directionPrimary = this.directionPrimary[playerID];
			directionalIndex = this.directionalIndex;
			gridBelongTo4Group = this.belongTo4Group;
			gridBelongTo16Group = this.belongTo16Group;
			gridWorldPosition = this.worldPosition;
		}
		
		public void GetValuesNextGrid(
			out int inhabitedBy,		
			out int particleIndex,	
			out int belongTo16Group,
			out int belongTo4Group,
			out int x,
			out int y,
			out Vector3 worldPosition)	
		{
			inhabitedBy	= this.inhabitedBy;
			particleIndex = this.particleIndex;
			belongTo16Group = this.belongTo16Group;
			belongTo4Group = this.belongTo4Group;
			x = this.x;
			y = this.y;
			worldPosition = this.worldPosition;
		}		
		
		//position in actual world space
		public Vector3 worldPosition;
		
		//position in grid
		public int x;
		public int y;
		
		//stores X and Y of each cell in each direction, uses direction sbyte as input for array
		public int[] directionalX;
		public int[] directionalY;
		
		//store index of each cell in each direction, uses direction sbyte as input for array
		public int[] directionalIndex;
		
		//used to keep tabs on which cells have had their gradient calculated
		public bool setForCycle;
		
		//whats the largest group index this belong to?
		public int belongToTopGroupIndex;
		
		//stores the primary direction for each players particles
		public int[] directionPrimary;
		
		//store the index of the particle currently in this cell, -1 if no particle
		public int particleIndex;
		
		//255 is wall
		//254 is empty
		//0 1 2 3 player ID
		public int inhabitedBy;
		
		//does this belong to a 4 group?
		public int belongTo4Group;
		//does this belong to a 16 group?
		public int belongTo16Group;		
	}
	private Grid[] grid;
	

	
	public int particleLimit;
	public int particleCount;
	public GameObject particleSystemPrefab;

	private class ParticleSystemGroup
	{
		public ParticleSystem particleSystemInstance;
		public ParticleSystem.Particle[] particleSystemArray;		
		public int count;
	}
	private ParticleSystemGroup[] particleSystemGroup;
	
	private struct ParticleComponent
	{
		public void ChangePlayer(int playerID)
		{
			this.life = 255;
			this.player = playerID;
		}
		
		public void AddLife()
		{
			if (life < 255)
			{
				life ++;
			}
			updateColor = true;
		}
		
		public void SetPosition(int nextgridX, int nextgridY, int nextGridIndex, Vector3 nextWorldPosition)
		{
			this.x = nextgridX;
			this.y = nextgridY;
			this.gridIndex = nextGridIndex;
			this.worldPosition = nextWorldPosition;
		}
		
		//stores position of particle
		public int x;
		public int y;
		//worldPos of particle
		public Vector3 worldPosition;
		//tells if new position needs to be updated in particleSystem
		public bool setPos;
		//in which gridindex is this particle
		public int gridIndex;
		//which player is this particle
		public int player;
		//life of particle
		public int life;
		//tells if a change has been made in this particles color and it needs new color calculated
		public bool updateColor;
		//tells if new color needs to be uploaded to particlesystem
		public bool setColor;
		//stores color to be uploaded to particle system
		public Color32 color;
		
		//remember this unnecessary because it will store the same as gridIndex
		//public int belongTo1Group;
		public int belongTo4Group;
		public int belongTo16Group;
	}
	private ParticleComponent[] particle;
	
	public bool drawDebugGrid;
	public bool drawDebugGroupGrid;
	
	private Thread gradientThread;
	private Thread moveParticlesThread;
	//private Thread setParticleSystemsThread;
	private bool runGradientThread = true;	
	private bool runMoveParticlesThread = true;		
	//private bool runSetParticleSystemsThread = true;	
	
	private byte directionCount = 12;
	
	private Vector3[] inputHitPoint;	
	
	[HideInInspector]
	public bool moveChaotic = false;
	
	[HideInInspector]
	public Color32[] playerColor;
	
	private void Awake()
	{
		
		
	}
	
	private void Start() 
	{
		PlayerData.initializePlayerArray();
		
		
		aiIndex = new int[PlayerData.playerCount, PlayerData.playerCount];
		aiWorldPosition = new Vector3[PlayerData.playerCount, PlayerData.playerCount];


		inputHitPoint = new Vector3[ PlayerData.playerCount];		
	
		playerColor = new Color32[PlayerData.playerCount];
		for (int i = 0; i < playerColor.Length; i++)
		{
			playerColor[i] = PlayerData.player[i].startColor;	
		}
		
		particleCount = PlayerData.startParticleCount;
		if(particleCount % 2 != 0)
		{	
			Debug.LogError("particleCount not multiple of 2");
		}				
		if (particleCount > particleLimit)
			particleCount = particleLimit;
		
		//4 particle systems are used
		// 2 for 1 size particles, which alternate each from in updates - actually only 1 for 1 size particle, the additional particle system kept around is really just deprecated by I may need it at some point so I'm not changing it
		// 1 for 4 size particles, 1 for 16 size particles, which alternate in updates
		particleSystemGroup = new ParticleSystemGroup[4];
		for (int i = 0; i < particleSystemGroup.Length; i++)
		{
			particleSystemGroup[i] = new ParticleSystemGroup();
			GameObject particleSystemGameObject = (GameObject)Instantiate(particleSystemPrefab,new Vector3(0,0,0), Quaternion.identity);
			particleSystemGroup[i].particleSystemInstance = particleSystemGameObject.GetComponent<ParticleSystem>();
			
			if( i == 0 || i == 1)
				particleSystemGroup[i].particleSystemArray = new ParticleSystem.Particle[particleCount];	
			if( i == 2)
				particleSystemGroup[i].particleSystemArray = new ParticleSystem.Particle[particleCount/4];	
			if( i == 3)
				particleSystemGroup[i].particleSystemArray = new ParticleSystem.Particle[particleCount/16];			
			
			for (int i2 = 0; i2 < particleSystemGroup[i].particleSystemArray.Length; i2++)
			{
				if( i == 0 || i == 1)
					particleSystemGroup[i].particleSystemArray[i2].size = .13f;
				if( i == 2)
					particleSystemGroup[i].particleSystemArray[i2].size = .25f;	
				if( i == 3)
					particleSystemGroup[i].particleSystemArray[i2].size = .53f;					
			}
		}
		
		particle = new ParticleComponent[particleCount];			
			
		if(gridParameters.width % 16 != 0)
		{	
			Debug.LogError("width not multiple of 16");
		}
		if(gridParameters.height % 16 != 0)
		{	
			Debug.LogError("height not multiple of 16");		
		}	
		gridParameters.length = gridParameters.width * gridParameters.height;	
	 	grid = new Grid[gridParameters.length];		
		
		//Init Grid
		int x = 0;
		int y = 0;	
		int obstacleLayer = 1 << 9;
		Ray ray = new Ray(Vector3.zero,Vector3.down);
		RaycastHit hitInfo;		
		Vector3 auxVector3 = new Vector3(0,0,0);
		//initialize grid positions and collision
		for(int i = 0; i < grid.Length; i++)
		{		
			grid[i] = new Grid();
			
			auxVector3.x = (float)x / 10f; 
			auxVector3.y = 100; 
			auxVector3.z = (float)y / 10f;
			ray.origin = auxVector3;			
			
			if(Physics.Raycast(ray, out hitInfo, 200,particeLayer))
			{
				grid[i].worldPosition = new Vector3((float)x / 10f, hitInfo.point.y + .05f, (float)y /10f);	
			}			
			else
			{
				grid[i].worldPosition = new Vector3((float)x / 10f, .01f, (float)y /10f);				
			}
				
			grid[i].x = x;
			grid[i].y = y;
			grid[i].particleIndex = -1;
			grid[i].directionPrimary =  new int[ PlayerData.playerCount ];
			grid[i].belongTo4Group = -1;
			grid[i].belongTo16Group = -1;
			
			if(Physics.Raycast(ray,200,obstacleLayer))
			{
				//wall
				grid[i].inhabitedBy = 255;	
			}
			else
			{
				//empty
				grid[i].inhabitedBy = 254;	
			}
			Utility.IncrementXY(ref x, ref y, ref gridParameters.width);
		}
		//initialize direction cache
		int directionCount = this.directionCount;
		int directionOffsetX;
		int directionOffsetY;
		int direction;
		int directionIndex;
		for(int i = 0; i < grid.Length; i++)
		{	
			grid[i].directionalX = new int[directionCount];
			grid[i].directionalY = new int[directionCount];
			grid[i].directionalIndex = new int[directionCount];			
			
			for(direction = 0; direction < directionCount; direction++)
			{
				DirectionToXY(out directionOffsetX, out directionOffsetY, direction);
				
				x = grid[i].x + directionOffsetX;
				y = grid[i].y + directionOffsetY;
				
				directionIndex = CoordinateToGridIndex(x,y);
						
				if( x >= 0 && x < gridParameters.width && y >= 0 && y < gridParameters.height && grid[directionIndex].inhabitedBy != 255)
				{
					grid[i].directionalX[direction] = x;
					grid[i].directionalY[direction] = y;
					grid[i].directionalIndex[direction] = directionIndex;
				}
				else
				{
					grid[i].directionalIndex[direction] = -1;					
				}	
			}
		}
						
		CreateGridGroups();
		
		spawnRate = (particleCount / 200);
		StartCoroutine(SpawnAllParticles());
				
	}
	
	#region Grid Group Initializing
	private class GridGroupInit
	{
		//for debugging really
		public int index;
		//store index of each cell in each direction, uses direction sbyte as input for array
		public int[] directionalGroupIndex;	
		//contains these grid indexes
		public int[] gridContainIndex;
		//contains these grid group indexes
		public int[] gridGroupContainIndex;		
		//what group this belongs to, -1 if belongs to none
		public int gridGroupBelongToIndex;
		//keeps tab on which cell has had its gradient calculated
		public bool setForCycle;
		// is this a 1, 4 or 16 group?
		public byte groupSize;
		//tells if a group has been added to a larger group and should no longer exist
		public bool delete;
		//stores averaged world position
		public Vector3 worldPosition;
	}	
	
	//God help whoever has to decipher this again
	private void CreateGridGroups()
	{
		List<GridGroupInit> gridGroupInitList = new List<GridGroupInit>();
		List<int> group16StartingIndex = new List<int>();
		GridGroupInit gridGroupInitCell = new GridGroupInit();
		
		int currentGridGroupInitCellIndex = 0;
		
		for (int i = 0; i < grid.Length; i++)
		{
			gridGroupInitCell = new GridGroupInit();
			gridGroupInitCell.directionalGroupIndex = new int[12];
			System.Array.Copy( grid[i].directionalIndex, gridGroupInitCell.directionalGroupIndex, grid[i].directionalIndex.Length);
			gridGroupInitCell.gridContainIndex = new int[1]{i};
			gridGroupInitCell.gridGroupBelongToIndex = -1;
			gridGroupInitCell.setForCycle = false;
			gridGroupInitCell.groupSize = 1;
			gridGroupInitCell.index = i;
			gridGroupInitCell.worldPosition = grid[i].worldPosition;
			if(grid[i].inhabitedBy == 255)
				gridGroupInitCell.delete = true;				
			gridGroupInitList.Add(gridGroupInitCell);
			currentGridGroupInitCellIndex++;
		}
		
		Debug.Log("gridGroupInitCell after adding grid: "+currentGridGroupInitCellIndex);					
		
		
		//init GridGroup
		bool runGridGroupCreate = true;
		bool groupHitWall = false;
		int xCount = 0;
		bool evenX = false;
		bool evenY = true;
		int nextGroupStartIndex = 0;
		int[] group4ContainIndex;
		byte[] createGroupRotation = new byte[3]{3,0,9};
		//create 4's groups
		while (runGridGroupCreate)
		{
			//get potential indexes of group
			groupHitWall = false;
			group4ContainIndex = new int[4];
			group4ContainIndex[0] = nextGroupStartIndex;
			for( int i = 0; i < createGroupRotation.Length; i++)
			{
				if(gridGroupInitList[ group4ContainIndex[i] ].directionalGroupIndex[createGroupRotation[i]] != -1)
				{
					group4ContainIndex[i+1] = gridGroupInitList[ group4ContainIndex[i] ].directionalGroupIndex[createGroupRotation[i]];

				}
				else
				{
					groupHitWall = true;	
				}
			}
			
			nextGroupStartIndex += 2;
			xCount += 2;
			if (evenX)
				evenX = false;	
			else
				evenX = true;
			
			if (xCount >= gridParameters.width)
			{
				xCount = 0;
				nextGroupStartIndex += gridParameters.width;
				if (nextGroupStartIndex >= gridParameters.length)
					runGridGroupCreate = false;
				if(evenY)
					evenY = false;
				else
					evenY = true;
			}
			
			if (groupHitWall)
			{
					
			}
			else
			{
				gridGroupInitCell = new GridGroupInit();	
				gridGroupInitCell.gridGroupBelongToIndex = -1;
				gridGroupInitCell.groupSize = 4;
				gridGroupInitCell.gridContainIndex = group4ContainIndex;
				gridGroupInitCell.gridGroupContainIndex = group4ContainIndex;
				gridGroupInitCell.worldPosition = 
					(
						gridGroupInitList[ group4ContainIndex[0] ].worldPosition +
						gridGroupInitList[ group4ContainIndex[1] ].worldPosition +
						gridGroupInitList[ group4ContainIndex[2] ].worldPosition +
						gridGroupInitList[ group4ContainIndex[0] ].worldPosition
					) / 4f;
				gridGroupInitCell.directionalGroupIndex = new int[12];
				
				for (int i = 0; i < 4; i++)
				{
					gridGroupInitList[ group4ContainIndex[i] ].delete = true;	
					gridGroupInitList[ group4ContainIndex[i] ].gridGroupBelongToIndex = currentGridGroupInitCellIndex;
				}
				
				//0 - NNE
				//1 - NE
				//2 - NEE
				//3 - SEE
				//4 - SE
				//5 - SSE
				//6 - SSW
				//7 - SW
				//8 - SWW
				//9 - NWW
				//10 - NW
				//11 - NNW						

				//connect grid group to nearby cells
				gridGroupInitCell.directionalGroupIndex[0] = gridGroupInitList[group4ContainIndex[2]].directionalGroupIndex[0];
				gridGroupInitCell.directionalGroupIndex[1] = gridGroupInitList[group4ContainIndex[2]].directionalGroupIndex[1];	
				gridGroupInitCell.directionalGroupIndex[2] = gridGroupInitList[group4ContainIndex[2]].directionalGroupIndex[2];
				
				gridGroupInitCell.directionalGroupIndex[3] = gridGroupInitList[group4ContainIndex[1]].directionalGroupIndex[3];
				gridGroupInitCell.directionalGroupIndex[4] = gridGroupInitList[group4ContainIndex[1]].directionalGroupIndex[4];	
				gridGroupInitCell.directionalGroupIndex[5] = gridGroupInitList[group4ContainIndex[1]].directionalGroupIndex[5];
				
				gridGroupInitCell.directionalGroupIndex[6] = gridGroupInitList[group4ContainIndex[0]].directionalGroupIndex[6];
				gridGroupInitCell.directionalGroupIndex[7] = gridGroupInitList[group4ContainIndex[0]].directionalGroupIndex[7];
				gridGroupInitCell.directionalGroupIndex[8] = gridGroupInitList[group4ContainIndex[0]].directionalGroupIndex[8];
				
				gridGroupInitCell.directionalGroupIndex[9] = gridGroupInitList[group4ContainIndex[3]].directionalGroupIndex[9];
				gridGroupInitCell.directionalGroupIndex[10] = gridGroupInitList[group4ContainIndex[3]].directionalGroupIndex[10];	
				gridGroupInitCell.directionalGroupIndex[11] = gridGroupInitList[group4ContainIndex[3]].directionalGroupIndex[11];		
				
				//connect nearby cells to new group
				//NNE
				if (gridGroupInitCell.directionalGroupIndex[0] != -1)
				{
					if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[0]].groupSize == 1)
					{
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[0]].directionalGroupIndex[5] = currentGridGroupInitCellIndex;				
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[0]].directionalGroupIndex[6] = currentGridGroupInitCellIndex;				
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[0]].directionalGroupIndex[7] = currentGridGroupInitCellIndex;											
					}
					else if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[0]].groupSize == 4)
					{
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[0]].directionalGroupIndex[5] = currentGridGroupInitCellIndex;									
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[0]].directionalGroupIndex[6] = currentGridGroupInitCellIndex;									
					}
				}
				
				//NE
				if (gridGroupInitCell.directionalGroupIndex[1] != -1)
				{				
					gridGroupInitList[gridGroupInitCell.directionalGroupIndex[1]].directionalGroupIndex[7] = currentGridGroupInitCellIndex;				
				}
				
				//NEE
				if (gridGroupInitCell.directionalGroupIndex[2] != -1)
				{
					if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[2]].groupSize == 1)
					{
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[2]].directionalGroupIndex[9] = currentGridGroupInitCellIndex;				
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[2]].directionalGroupIndex[8] = currentGridGroupInitCellIndex;				
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[2]].directionalGroupIndex[7] = currentGridGroupInitCellIndex;											
					}
					else if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[2]].groupSize == 4)
					{
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[2]].directionalGroupIndex[8] = currentGridGroupInitCellIndex;									
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[2]].directionalGroupIndex[9] = currentGridGroupInitCellIndex;									
					}
				}	
				
				//SEE
				if (gridGroupInitCell.directionalGroupIndex[3] != -1)
				{
					if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[3]].groupSize == 1)
					{
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[3]].directionalGroupIndex[9] = currentGridGroupInitCellIndex;				
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[3]].directionalGroupIndex[8] = currentGridGroupInitCellIndex;				
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[3]].directionalGroupIndex[10] = currentGridGroupInitCellIndex;											
					}
					else if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[3]].groupSize == 4)
					{
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[3]].directionalGroupIndex[8] = currentGridGroupInitCellIndex;									
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[3]].directionalGroupIndex[9] = currentGridGroupInitCellIndex;									
					}
				}	
				
				//SE
				if (gridGroupInitCell.directionalGroupIndex[4] != -1)
				{
					gridGroupInitList[gridGroupInitCell.directionalGroupIndex[4]].directionalGroupIndex[10] = currentGridGroupInitCellIndex;				
				}
				
				//SSE
				if (gridGroupInitCell.directionalGroupIndex[5] != -1)
				{
					if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[5]].groupSize == 1)
					{
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[5]].directionalGroupIndex[0] = currentGridGroupInitCellIndex;				
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[5]].directionalGroupIndex[11] = currentGridGroupInitCellIndex;				
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[5]].directionalGroupIndex[10] = currentGridGroupInitCellIndex;											
					}
					else if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[5]].groupSize == 4)
					{
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[5]].directionalGroupIndex[0] = currentGridGroupInitCellIndex;									
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[5]].directionalGroupIndex[11] = currentGridGroupInitCellIndex;									
					}
				}	
				
				//SSW
				if (gridGroupInitCell.directionalGroupIndex[6] != -1)
				{
					if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[6]].groupSize == 1)
					{
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[6]].directionalGroupIndex[0] = currentGridGroupInitCellIndex;				
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[6]].directionalGroupIndex[11] = currentGridGroupInitCellIndex;				
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[6]].directionalGroupIndex[1] = currentGridGroupInitCellIndex;											
					}
					else if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[6]].groupSize == 4)
					{
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[6]].directionalGroupIndex[0] = currentGridGroupInitCellIndex;									
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[6]].directionalGroupIndex[11] = currentGridGroupInitCellIndex;									
					}
				}	
				
				//SW
				if (gridGroupInitCell.directionalGroupIndex[7] != -1)
				{
					gridGroupInitList[gridGroupInitCell.directionalGroupIndex[7]].directionalGroupIndex[1] = currentGridGroupInitCellIndex;				
				}				

				//SWW
				if (gridGroupInitCell.directionalGroupIndex[8] != -1)
				{
					if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[8]].groupSize == 1)
					{
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[8]].directionalGroupIndex[2] = currentGridGroupInitCellIndex;				
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[8]].directionalGroupIndex[3] = currentGridGroupInitCellIndex;				
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[8]].directionalGroupIndex[1] = currentGridGroupInitCellIndex;											
					}
					else if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[8]].groupSize == 4)
					{
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[8]].directionalGroupIndex[2] = currentGridGroupInitCellIndex;									
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[8]].directionalGroupIndex[3] = currentGridGroupInitCellIndex;									
					}
				}
				
				//NWW
				if (gridGroupInitCell.directionalGroupIndex[9] != -1)
				{
					if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[9]].groupSize == 1)
					{
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[9]].directionalGroupIndex[2] = currentGridGroupInitCellIndex;				
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[9]].directionalGroupIndex[3] = currentGridGroupInitCellIndex;				
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[9]].directionalGroupIndex[4] = currentGridGroupInitCellIndex;											
					}
					else if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[9]].groupSize == 4)
					{
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[9]].directionalGroupIndex[2] = currentGridGroupInitCellIndex;									
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[9]].directionalGroupIndex[3] = currentGridGroupInitCellIndex;									
					}
				}	
				
				//NW
				if (gridGroupInitCell.directionalGroupIndex[10] != -1)
				{
					gridGroupInitList[gridGroupInitCell.directionalGroupIndex[10]].directionalGroupIndex[4] = currentGridGroupInitCellIndex;				
				}					
								
				//NNW
				if (gridGroupInitCell.directionalGroupIndex[11] != -1)
				{
					if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[11]].groupSize == 1)
					{
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[11]].directionalGroupIndex[5] = currentGridGroupInitCellIndex;				
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[11]].directionalGroupIndex[6] = currentGridGroupInitCellIndex;				
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[11]].directionalGroupIndex[4] = currentGridGroupInitCellIndex;											
					}
					else if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[11]].groupSize == 4)
					{
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[11]].directionalGroupIndex[5] = currentGridGroupInitCellIndex;									
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[11]].directionalGroupIndex[6] = currentGridGroupInitCellIndex;									
					}
				}		
				
				gridGroupInitCell.index = currentGridGroupInitCellIndex;
				gridGroupInitList.Add(gridGroupInitCell);
				if (evenX && evenY)
				{
					group16StartingIndex.Add(currentGridGroupInitCellIndex);
				}
				currentGridGroupInitCellIndex++;
			}
		}
		
		Debug.Log("gridGroupInitCell after adding 4's: "+currentGridGroupInitCellIndex);		
		
		bool groupHit1Group = false;
		//stores the value of the 4 group the current cycle started from, iterates each run of while loop
		int current4GroupIndex = 0;
		int index = 0;
		runGridGroupCreate = true;
		//create 16's groups
		while (runGridGroupCreate)
		{

			groupHit1Group = false;
			group4ContainIndex = new int[4];
			
			//add first 4 group
			index = group16StartingIndex[current4GroupIndex];
			
			if (index != -1)
			{
				group4ContainIndex[0] = index;
				
				groupHit1Group = Near1Group(ref gridGroupInitList, index);				
				
				for( int i = 0; i < createGroupRotation.Length; i++)
				{	
					if (groupHit1Group)
					{
						break;	
					}				
					else
					{
						if (index != -1)
						{
							index = gridGroupInitList[ index ].directionalGroupIndex[ createGroupRotation[i] ];
							group4ContainIndex[i+1] =  index;					
							groupHit1Group = Near1Group(ref gridGroupInitList, index);	
						}
						else
						{
							group4ContainIndex[i+1] =  index;					
							groupHit1Group = true;					
						}	
					}
				}
			
	
				if (groupHit1Group)
				{
				
				}
				else
				{
					gridGroupInitCell = new GridGroupInit();		
					gridGroupInitCell.gridGroupBelongToIndex = -1;
					gridGroupInitCell.groupSize = 16;
					gridGroupInitCell.gridGroupContainIndex = group4ContainIndex;
					gridGroupInitCell.worldPosition = 
						(
							gridGroupInitList[ group4ContainIndex[0] ].worldPosition +
							gridGroupInitList[ group4ContainIndex[1] ].worldPosition +
							gridGroupInitList[ group4ContainIndex[2] ].worldPosition +
							gridGroupInitList[ group4ContainIndex[0] ].worldPosition
						) / 4f;					
					gridGroupInitCell.directionalGroupIndex = new int[12];
					
					
					//add all contained Indexes to new 16 group;
					int[] group16ContainIndex = new int[16];
					int counter = 0;
					index = 0;
					for (int i = 0; i < 16; i++)
					{
						group16ContainIndex[i] = gridGroupInitList[ group4ContainIndex[index] ].gridContainIndex[counter];
	
						counter++;
						if (counter == 4)
						{
							counter = 0;
							index++;
						}
					}
					
					gridGroupInitCell.gridContainIndex = group16ContainIndex;
					
					for (int i = 0; i < 4; i++)
					{
						gridGroupInitList[ group4ContainIndex[i] ].delete = true;	
						gridGroupInitList[ group4ContainIndex[i] ].gridGroupBelongToIndex = currentGridGroupInitCellIndex;						
					}				
					
					//connect grid group to nearby cells
					gridGroupInitCell.directionalGroupIndex[0] = gridGroupInitList[group4ContainIndex[2]].directionalGroupIndex[0];
					gridGroupInitCell.directionalGroupIndex[1] = gridGroupInitList[group4ContainIndex[2]].directionalGroupIndex[1];	
					gridGroupInitCell.directionalGroupIndex[2] = gridGroupInitList[group4ContainIndex[2]].directionalGroupIndex[2];
					
					gridGroupInitCell.directionalGroupIndex[3] = gridGroupInitList[group4ContainIndex[1]].directionalGroupIndex[3];
					gridGroupInitCell.directionalGroupIndex[4] = gridGroupInitList[group4ContainIndex[1]].directionalGroupIndex[4];	
					gridGroupInitCell.directionalGroupIndex[5] = gridGroupInitList[group4ContainIndex[1]].directionalGroupIndex[5];
					
					gridGroupInitCell.directionalGroupIndex[6] = gridGroupInitList[group4ContainIndex[0]].directionalGroupIndex[6];
					gridGroupInitCell.directionalGroupIndex[7] = gridGroupInitList[group4ContainIndex[0]].directionalGroupIndex[7];
					gridGroupInitCell.directionalGroupIndex[8] = gridGroupInitList[group4ContainIndex[0]].directionalGroupIndex[8];
					
					gridGroupInitCell.directionalGroupIndex[9] = gridGroupInitList[group4ContainIndex[3]].directionalGroupIndex[9];
					gridGroupInitCell.directionalGroupIndex[10] = gridGroupInitList[group4ContainIndex[3]].directionalGroupIndex[10];	
					gridGroupInitCell.directionalGroupIndex[11] = gridGroupInitList[group4ContainIndex[3]].directionalGroupIndex[11];					
	
					//0 - NNE
					//1 - NE
					//2 - NEE
					//3 - SEE
					//4 - SE
					//5 - SSE
					//6 - SSW
					//7 - SW
					//8 - SWW
					//9 - NWW
					//10 - NW
					//11 - NNW		
					
					//connect nearby cells to new group
					//NNE
					if (gridGroupInitCell.directionalGroupIndex[0] != -1)
					{
						if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[0]].groupSize == 4)
						{
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[0]].directionalGroupIndex[5] = currentGridGroupInitCellIndex;				
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[0]].directionalGroupIndex[6] = currentGridGroupInitCellIndex;				
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[0]].directionalGroupIndex[7] = currentGridGroupInitCellIndex;											
						}
						else if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[0]].groupSize == 16)
						{
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[0]].directionalGroupIndex[5] = currentGridGroupInitCellIndex;									
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[0]].directionalGroupIndex[6] = currentGridGroupInitCellIndex;									
						}
					}
					
					//NE
					if (gridGroupInitCell.directionalGroupIndex[1] != -1)
					{				
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[1]].directionalGroupIndex[7] = currentGridGroupInitCellIndex;				
					}
					
					//NEE
					if (gridGroupInitCell.directionalGroupIndex[2] != -1)
					{
						if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[2]].groupSize == 4)
						{
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[2]].directionalGroupIndex[9] = currentGridGroupInitCellIndex;				
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[2]].directionalGroupIndex[8] = currentGridGroupInitCellIndex;				
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[2]].directionalGroupIndex[7] = currentGridGroupInitCellIndex;											
						}
						else if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[2]].groupSize == 16)
						{
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[2]].directionalGroupIndex[8] = currentGridGroupInitCellIndex;									
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[2]].directionalGroupIndex[9] = currentGridGroupInitCellIndex;									
						}
					}	
					
					//SEE
					if (gridGroupInitCell.directionalGroupIndex[3] != -1)
					{
						if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[3]].groupSize == 4)
						{
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[3]].directionalGroupIndex[9] = currentGridGroupInitCellIndex;				
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[3]].directionalGroupIndex[8] = currentGridGroupInitCellIndex;				
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[3]].directionalGroupIndex[10] = currentGridGroupInitCellIndex;											
						}
						else if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[3]].groupSize == 16)
						{
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[3]].directionalGroupIndex[8] = currentGridGroupInitCellIndex;									
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[3]].directionalGroupIndex[9] = currentGridGroupInitCellIndex;									
						}
					}	
					
					//SE
					if (gridGroupInitCell.directionalGroupIndex[4] != -1)
					{
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[4]].directionalGroupIndex[10] = currentGridGroupInitCellIndex;				
					}
					
					//SSE
					if (gridGroupInitCell.directionalGroupIndex[5] != -1)
					{
						if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[5]].groupSize == 4)
						{
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[5]].directionalGroupIndex[0] = currentGridGroupInitCellIndex;				
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[5]].directionalGroupIndex[11] = currentGridGroupInitCellIndex;				
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[5]].directionalGroupIndex[10] = currentGridGroupInitCellIndex;											
						}
						else if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[5]].groupSize == 16)
						{
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[5]].directionalGroupIndex[0] = currentGridGroupInitCellIndex;									
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[5]].directionalGroupIndex[11] = currentGridGroupInitCellIndex;									
						}
					}	
					
					//SSW
					if (gridGroupInitCell.directionalGroupIndex[6] != -1)
					{
						if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[6]].groupSize == 4)
						{
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[6]].directionalGroupIndex[0] = currentGridGroupInitCellIndex;				
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[6]].directionalGroupIndex[11] = currentGridGroupInitCellIndex;				
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[6]].directionalGroupIndex[1] = currentGridGroupInitCellIndex;											
						}
						else if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[6]].groupSize == 16)
						{
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[6]].directionalGroupIndex[0] = currentGridGroupInitCellIndex;									
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[6]].directionalGroupIndex[11] = currentGridGroupInitCellIndex;									
						}
					}	
					
					//SW
					if (gridGroupInitCell.directionalGroupIndex[7] != -1)
					{
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[7]].directionalGroupIndex[1] = currentGridGroupInitCellIndex;				
					}				
	
					//SWW
					if (gridGroupInitCell.directionalGroupIndex[8] != -1)
					{
						if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[8]].groupSize == 4)
						{
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[8]].directionalGroupIndex[2] = currentGridGroupInitCellIndex;				
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[8]].directionalGroupIndex[3] = currentGridGroupInitCellIndex;				
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[8]].directionalGroupIndex[1] = currentGridGroupInitCellIndex;											
						}
						else if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[8]].groupSize == 16)
						{
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[8]].directionalGroupIndex[2] = currentGridGroupInitCellIndex;									
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[8]].directionalGroupIndex[3] = currentGridGroupInitCellIndex;									
						}
					}
					
					//NWW
					if (gridGroupInitCell.directionalGroupIndex[9] != -1)
					{
						if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[9]].groupSize == 4)
						{
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[9]].directionalGroupIndex[2] = currentGridGroupInitCellIndex;				
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[9]].directionalGroupIndex[3] = currentGridGroupInitCellIndex;				
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[9]].directionalGroupIndex[4] = currentGridGroupInitCellIndex;											
						}
						else if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[9]].groupSize == 16)
						{
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[9]].directionalGroupIndex[2] = currentGridGroupInitCellIndex;									
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[9]].directionalGroupIndex[3] = currentGridGroupInitCellIndex;									
						}
					}	
					
					//NW
					if (gridGroupInitCell.directionalGroupIndex[10] != -1)
					{
						gridGroupInitList[gridGroupInitCell.directionalGroupIndex[10]].directionalGroupIndex[4] = currentGridGroupInitCellIndex;				
					}					
									
					//NNW
					if (gridGroupInitCell.directionalGroupIndex[11] != -1)
					{
						if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[11]].groupSize == 4)
						{
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[11]].directionalGroupIndex[5] = currentGridGroupInitCellIndex;				
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[11]].directionalGroupIndex[6] = currentGridGroupInitCellIndex;				
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[11]].directionalGroupIndex[4] = currentGridGroupInitCellIndex;											
						}
						else if (gridGroupInitList[gridGroupInitCell.directionalGroupIndex[11]].groupSize == 16)
						{
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[11]].directionalGroupIndex[5] = currentGridGroupInitCellIndex;									
							gridGroupInitList[gridGroupInitCell.directionalGroupIndex[11]].directionalGroupIndex[6] = currentGridGroupInitCellIndex;									
						}
					}		
					
					gridGroupInitCell.index = currentGridGroupInitCellIndex;
					gridGroupInitList.Add(gridGroupInitCell);
					currentGridGroupInitCellIndex++;				
				}
			}
			
			current4GroupIndex++;
			if(current4GroupIndex == group16StartingIndex.Count)
				runGridGroupCreate = false;
		}
		
		Debug.Log("gridGroupInitCell after adding 16's: "+currentGridGroupInitCellIndex);
		
		
		//old index , new index
		//Dictionary<int,int> flattenIndexDictionary = new Dictionary<int, int>();
		List<GridGroup> gridGroupList = new List<GridGroup>();
		GridGroup gridGroupCell;
		//int currentGridGroupCellIndex = 0;
		for (int i = 0; i < gridGroupInitList.Count; i++)
		{
			//if (!gridGroupInitList[i].delete)	
			//{
				gridGroupCell = new GridGroup();
				gridGroupCell.directionalGroupIndex = gridGroupInitList[i].directionalGroupIndex;
				gridGroupCell.gridContainIndex = gridGroupInitList[i].gridContainIndex;
				gridGroupCell.gridGroupBelongToIndex = gridGroupInitList[i].gridGroupBelongToIndex;
				gridGroupCell.gridGroupContainIndex = gridGroupInitList[i].gridGroupContainIndex;
				gridGroupCell.playerContainCount = new byte[PlayerData.playerCount];
				gridGroupCell.worldPosition = gridGroupInitList[i].worldPosition;
				gridGroupList.Add(gridGroupCell);
				//flattenIndexDictionary.Add(gridGroupInitList[i].index,currentGridGroupCellIndex);
				//currentGridGroupCellIndex++;
			//}		
			//else
			//{
				//sometimes an old indx would be looked up that got removed, for some reason
				//this is so that removed index will return -1 instead of 0 which would cause issues
				//this shouldn't be happening really
				//flattenIndexDictionary.Add(gridGroupInitList[i].index,-1);	
			//}
		}
		
		
		
		gridGroupAll = gridGroupList.ToArray();
		
		//int outValue;
		for (int i = 0; i < gridGroupAll.Length; i++)
		{
			/*
			for (int d = 0; d < 12; d++)
			{
				if(gridGroup[i].directionalGroupIndex[d] == -1)
				{
					gridGroup[i].directionalGroupIndex[d] = -1;		
				}
				else
				{
					flattenIndexDictionary.TryGetValue(gridGroup[i].directionalGroupIndex[d], out outValue);
					gridGroup[i].directionalGroupIndex[d] = outValue;
				}
			}
			*/
			
			//hook up grid components to their groups
			for (int d = 0; d < gridGroupAll[i].gridContainIndex.Length; d++)
			{			
				grid[gridGroupAll[i].gridContainIndex[d]].belongToTopGroupIndex = i;
			}
			
			if (gridGroupAll[i].gridContainIndex.Length == 16)
			{
				for (int d = 0; d < gridGroupAll[i].gridContainIndex.Length; d++)
				{			
					grid[gridGroupAll[i].gridContainIndex[d]].belongTo16Group = i;
				}
			}
			if (gridGroupAll[i].gridContainIndex.Length == 4)
			{
				for (int d = 0; d < gridGroupAll[i].gridContainIndex.Length; d++)
				{			
					grid[gridGroupAll[i].gridContainIndex[d]].belongTo4Group = i;
				}
			}			
		}
		
		List<GridGroup> gridGroup1List = new List<GridGroup>();
		List<GridGroup> gridGroup4List = new List<GridGroup>();
		List<GridGroup> gridGroup16List = new List<GridGroup>();		
		
		for (int i = 0; i < gridGroupAll.Length; i++)
		{
			if (gridGroupAll[i].gridContainIndex.Length == 16 && gridGroupAll[i].gridGroupBelongToIndex == -1)
				gridGroup16List.Add(gridGroupAll[i]);
			if (gridGroupAll[i].gridContainIndex.Length == 4 && gridGroupAll[i].gridGroupBelongToIndex == -1)
				gridGroup4List.Add(gridGroupAll[i]);
			if (gridGroupAll[i].gridContainIndex.Length == 1 && gridGroupAll[i].gridGroupBelongToIndex == -1)
				gridGroup1List.Add(gridGroupAll[i]);			
		}
		
		gridGroup1 = gridGroup1List.ToArray();
		gridGroup4 = gridGroup4List.ToArray();
		gridGroup16 = gridGroup16List.ToArray();	
	
	}
	
	private bool Near1Group(ref List<GridGroupInit> gridGroupInitList, int index)
	{
		for (int i = 0; i < 12; i++)
		{
			if ( index != -1 && gridGroupInitList[index].directionalGroupIndex[i] != -1 && gridGroupInitList[ gridGroupInitList[index].directionalGroupIndex[i] ].groupSize == 1)
			{
				return true;
			}
		}
		return false;					
	}
	#endregion

	#region Particle Spawning
	private int currentParticleSpawnIndex;
	private int currentParticleSpawnLimit;
	[HideInInspector]
	public int currentParticleSpawnPlayer;
	[HideInInspector]
	public bool particlesSpawned;
	private int spawnRate = 10;
	private int spawnCounter = 0;
	IEnumerator SpawnAllParticles()
	{
		int i;
		int lastAddedIndex;
		
		//init gradient step array
		int expandBy = 2;
		int initSize = (gridParameters.length * expandBy);
		int[] gradientStepArray = new int[initSize];		
		for(i = 0; i < gradientStepArray.Length; i++)
		{
			gradientStepArray[i] = -1;
		}				
		
		//Init particle array
		currentParticleSpawnIndex = 0;	
		for (int playerID = 0; playerID < PlayerData.playerCount; playerID++)
		{			
			for(i = 0; i < grid.Length; i++)
			{		
				grid[i].setForCycle = false;
			}
			lastAddedIndex = 0;		

			currentParticleSpawnLimit = (int)(((float)(particleCount) / (float)(PlayerData.playerCount)) * (float)(playerID+1));
			currentParticleSpawnPlayer = playerID;
					
			i = 0;
			
			int gridIndexSpawn = 0;

				//Figure out the index of the cell we are over
			gridIndexSpawn = WorldPositionToGridIndex(0, 0);						
	


			SpawnParticle( gridIndexSpawn, ref gradientStepArray, ref lastAddedIndex );
			bool spawnParticles = true;
			while(spawnParticles)
			{
				if(gradientStepArray[i] != -1)
				{
					SpawnParticle(gradientStepArray[i], ref gradientStepArray, ref lastAddedIndex);	
					gradientStepArray[i] = -1;
					i++;
				}
				else
				{
					spawnParticles = false;
				}
				
				
				
				spawnCounter++;
				if(spawnCounter == spawnRate)
				{
					particlesMoved = true;
					//particleSystemInstance.SetParticles(particleSystemArray,particleCount);	
					spawnCounter = 0;
					yield return null;
				}
			}	
		}		
		
		particlesSpawned = true;
	}
	
	private void SpawnParticle(int gridIndex, ref int[] gradientStepArray, ref int lastAddedIndex)
	{
		if(currentParticleSpawnIndex >= currentParticleSpawnLimit )
		{
			//all particles spawned
			return;
		}
		
		if(grid[gridIndex].inhabitedBy != 255)
		{
			particle[currentParticleSpawnIndex].x = grid[gridIndex].x;
			particle[currentParticleSpawnIndex].y = grid[gridIndex].y;
			particle[currentParticleSpawnIndex].gridIndex = gridIndex;
			particle[currentParticleSpawnIndex].life = 1;
			particle[currentParticleSpawnIndex].player = currentParticleSpawnPlayer;
			
			particle[currentParticleSpawnIndex].worldPosition = grid[gridIndex].worldPosition;
			particle[currentParticleSpawnIndex].setPos = true;

			particle[currentParticleSpawnIndex].color.r = 0;
			particle[currentParticleSpawnIndex].color.g = 0;
			particle[currentParticleSpawnIndex].color.b = 0;	
			particle[currentParticleSpawnIndex].setColor = true;
			
			particle[currentParticleSpawnIndex].belongTo4Group = grid[gridIndex].belongTo4Group;
			particle[currentParticleSpawnIndex].belongTo16Group = grid[gridIndex].belongTo16Group;	
			if (grid[gridIndex].belongTo4Group != -1)
				gridGroupAll[ grid[gridIndex].belongTo4Group ].playerContainCount[currentParticleSpawnPlayer]++;
			if (grid[gridIndex].belongTo16Group != -1)			
				gridGroupAll[ grid[gridIndex].belongTo16Group ].playerContainCount[currentParticleSpawnPlayer]++;
			
			gridGroupAll[gridIndex].playerContainCount[currentParticleSpawnPlayer]++;
			
			grid[gridIndex].inhabitedBy = currentParticleSpawnPlayer;
			grid[gridIndex].particleIndex = currentParticleSpawnIndex;
						
			PlayerData.player[currentParticleSpawnPlayer].particleCount++;
			PlayerData.player[currentParticleSpawnPlayer].startParticleCount++;		
			PlayerData.totalParticleCount++;		
			currentParticleSpawnIndex++;
		}		
		
		int x = grid[gridIndex].x;
		int y = grid[gridIndex].y;
		
		AddSpawnTest(x+1,y, ref gradientStepArray, ref lastAddedIndex);
		AddSpawnTest(x-1,y, ref gradientStepArray, ref lastAddedIndex);
		AddSpawnTest(x,y+1, ref gradientStepArray, ref lastAddedIndex);
		AddSpawnTest(x,y-1, ref gradientStepArray, ref lastAddedIndex);
	}
				
	private void AddSpawnTest(int x, int y, ref int[] gradientStepArray, ref int lastAddedIndex)
	{
		int gridIndex = CoordinateToGridIndex(x,y);	
		if( x >= 0 && x < gridParameters.width && y >= 0 && y < gridParameters.height && !grid[gridIndex].setForCycle)	
		{
		 	grid[gridIndex].setForCycle = true;
			gradientStepArray[lastAddedIndex] = gridIndex;
			lastAddedIndex++;
		}
	}
	#endregion
	
	private int particeLayer = 1 << 8;	
	private int winningPlayerID;
	//public int moveParticleTicks;
	//public int updateTicks;
	private void Update()
	{
		
		//if (Input.GetKeyDown(KeyCode.Escape))
		//{
		//	projectGlobal.BeginWinSequence(0);	
		//}
		
		
		if (particlesSpawned)
		{
			if(	gradientThread == null)
			{
				gradientThread = new Thread(BuildGroupGradient);
				gradientThread.Priority = System.Threading.ThreadPriority.Lowest;
				gradientThread.Start();
			}	
			
			//MoveParticles();
			if(	moveParticlesThread == null)
			{
				moveParticlesThread = new Thread(MoveParticles);
				moveParticlesThread.Priority = System.Threading.ThreadPriority.Highest;
				moveParticlesThread.Start();
			}									
		
		//if(	setParticleSystemsThread == null)
		//{
		//	setParticleSystemsThread = new Thread(SetParticleSystems);
		//	setParticleSystemsThread.Priority = System.Threading.ThreadPriority.Lowest;
		//	setParticleSystemsThread.Start();
		//}			
		
			//Get input positions for each player
			Vector3 inputPos;
			Ray inputRay;
			RaycastHit hitInfo;
			int lastParticleCount = 0;
			int newWinningPlayerID = 0;
			for(int playerID = 0; playerID < PlayerData.playerCount; playerID++)
			{		
				inputPos =  Vector3.zero;
				inputRay = Camera.main.ScreenPointToRay(inputPos);
				if(Physics.Raycast(inputRay, out hitInfo, 200, particeLayer))
				{
					inputHitPoint[playerID] = hitInfo.point;
				}
				
				//update particle percentage
				PlayerData.player[playerID].particlePercentage =
				(PlayerData.player[playerID].particleCount - PlayerData.player[playerID].startParticleCount) 
				/ 
				(PlayerData.totalParticleCount - PlayerData.player[playerID].startParticleCount);
				PlayerData.player[playerID].particlePercentage = Mathf.Clamp(PlayerData.player[playerID].particlePercentage,0f,1f);						
						
				if (PlayerData.player[playerID].particleCount > lastParticleCount)
				{
					newWinningPlayerID = playerID;
					lastParticleCount = PlayerData.player[playerID].particleCount;
				}
				
//				if (PlayerData.playerWinID == 255 && PlayerData.player[playerID].particleCount == PlayerData.totalParticleCount)
//				{
//					projectGlobal.BeginWinSequence(playerID);
//				}
			}	
			
			winningPlayerID = newWinningPlayerID;
			
		}
		

		if (PlayerData.playerWinID == 255)
		{		
			if (particlesMoved)
			{
				particlesMoved = false;
				SetParticleSystems();
	
				//load new particle positions into particle system
				particleSystemGroup[1].particleSystemInstance.SetParticles(particleSystemGroup[1].particleSystemArray,particleSystemGroup[1].count);	
				particleSystemGroup[2].particleSystemInstance.SetParticles(particleSystemGroup[2].particleSystemArray,particleSystemGroup[2].count);		
				particleSystemGroup[3].particleSystemInstance.SetParticles(particleSystemGroup[3].particleSystemArray,particleSystemGroup[3].count);
			}	
		}
		else
		{
			FloatParticlesUp();
			particleSystemGroup[1].particleSystemInstance.SetParticles(particleSystemGroup[1].particleSystemArray,particleSystemGroup[1].count);	
			particleSystemGroup[2].particleSystemInstance.SetParticles(particleSystemGroup[2].particleSystemArray,particleSystemGroup[2].count);		
			particleSystemGroup[3].particleSystemInstance.SetParticles(particleSystemGroup[3].particleSystemArray,particleSystemGroup[3].count);
		}
		
		gameTimer += Time.deltaTime;
		
		if (gameTimer > gameTimeLimit)
		{
			if (PlayerData.playerWinID == 255)
			{
				surroundMaterial.color = Color.Lerp(Color.white,surroundEndColor, Mathf.PingPong(gameTimer,1f));
			}
			else
			{
				surroundMaterial.color = Color.white;	
			}
		}
	}
	
	void FixedUpdate()
	{
		runParticlesMoveCycle = true;
			
		//Debug.Log("      __________________      ");
		//Debug.Log(aiIndex[0,0]+"   "+aiIndex[0,1]+"   "+aiIndex[0,2]+"    "+aiIndex[0,3]);
		//Debug.Log(aiIndex[1,0]+"   "+aiIndex[1,1]+"   "+aiIndex[1,2]+"    "+aiIndex[1,3]);
		//Debug.Log(aiIndex[2,0]+"   "+aiIndex[2,1]+"   "+aiIndex[2,2]+"    "+aiIndex[2,3]);
		//Debug.Log(aiIndex[3,0]+"   "+aiIndex[3,1]+"   "+aiIndex[3,2]+"    "+aiIndex[3,3]);		
	}
	
	private void FloatParticlesUp()
	{
		Vector3 newPos;
		Color newColor;
		float floatSpeed = 1f;
		float fadeSpeed = 4f;
		int limit;
		int i;
		
		for (int systemIndex = 1; systemIndex < 4; systemIndex++)
		{
			limit = particleSystemGroup[systemIndex].count;
			for (i = 0; i < limit; i++)
			{
				newPos = particleSystemGroup[systemIndex].particleSystemArray[i].position;
				newPos.y += Time.deltaTime * floatSpeed * ((float)i/(float)limit);
				particleSystemGroup[systemIndex].particleSystemArray[i].position = newPos;
				newColor = particleSystemGroup[systemIndex].particleSystemArray[i].color;
				newColor.r += Time.deltaTime/fadeSpeed;
				newColor.g += Time.deltaTime/fadeSpeed;
				newColor.b += Time.deltaTime/fadeSpeed;			
				particleSystemGroup[systemIndex].particleSystemArray[i].color = newColor;
			}
		}
	}
	
	//private bool particleSystemsUpdated = true;
	//private bool updatingParticleSystemArrays = false;
 	private void SetParticleSystems()
	{
		int containIterate;
		int containIterate2;		
		int containIndex;
		int containIndex2;
		int playerID;
		int playerLimit = PlayerData.playerCount;
	
		bool group16Set;
		bool group4Set;
		
		int group16Contain0;
		int group4Contain0;			
		int i;
		int limit;	
		
		//while (runSetParticleSystemsThread)
		//{
			//tests for updateParticleSystems so that this thread will not run twice in the time the main thread has to update itself
			//tests for particlesMoved so that this thread won't run twice in the time the moveparticles thread updates itself
			//while (!particleSystemsUpdated && !particlesMoved)
			//{
			//	Thread.Sleep(0);
			//}
			
			//set so that this thread will wait until particles have moved before running again
			//particlesMoved = false;
			
			//set so that main thread does not update particle system until this is done
			//updatingParticleSystemArrays = true;

			particleSystemGroup[0].count = 0;
			particleSystemGroup[1].count = 0;
			particleSystemGroup[2].count = 0;
			particleSystemGroup[3].count = 0;		
						
			limit = gridGroup16.Length;
			for (i = 0; i < limit; i++)
			{
				group16Set = false;
				group16Contain0 = 0;
				for ( playerID = 0; playerID < playerLimit; playerID++)
				{			
					if (gridGroup16[i].playerContainCount[playerID] > 13)
					{
						particleSystemGroup[3].particleSystemArray[particleSystemGroup[3].count].position = gridGroup16[i].worldPosition;
						particleSystemGroup[3].particleSystemArray[particleSystemGroup[3].count].color = playerColor[playerID];		
						particleSystemGroup[3].count++;
						playerID = playerLimit;
						group16Set = true;
					}
					else if (gridGroup16[i].playerContainCount[playerID] == 0)
					{
						group16Contain0++;
					}
				}
				if (!group16Set && group16Contain0 != playerLimit)
				{
					for (containIterate = 0; containIterate < 4; containIterate++)
					{
						group4Set = false;
						group4Contain0 = 0;
						containIndex = gridGroup16[i].gridGroupContainIndex[containIterate];
						for ( playerID = 0; playerID < playerLimit; playerID++)
						{							
							if ( gridGroupAll[ containIndex ].playerContainCount[playerID] > 3)
							{
								particleSystemGroup[2].particleSystemArray[particleSystemGroup[2].count].position = gridGroupAll[containIndex].worldPosition;
								particleSystemGroup[2].particleSystemArray[particleSystemGroup[2].count].color = playerColor[playerID];								
								particleSystemGroup[2].count++;	
								playerID = playerLimit;
								group4Set = true;
							}	
							else if ( gridGroupAll[ containIndex ].playerContainCount[playerID] == 0)
							{
								group4Contain0++;
							}						
						}
						if (!group4Set && group4Contain0 != playerLimit)
						{
							for (containIterate2 = 0; containIterate2 < 4; containIterate2++)
							{	
								for ( playerID = 0; playerID < playerLimit; playerID++)
								{									
									containIndex2 = gridGroupAll[containIndex].gridGroupContainIndex[containIterate2];
									if (gridGroupAll[containIndex2].playerContainCount[playerID] == 1)
									{
										particleSystemGroup[1].particleSystemArray[particleSystemGroup[1].count].position = gridGroupAll[containIndex2].worldPosition;							
										particleSystemGroup[1].particleSystemArray[particleSystemGroup[1].count].color = playerColor[playerID];				
										particleSystemGroup[1].count++;	
										playerID = playerLimit;
									}
								}
							}						
						}
					}
				}
			}
			
			
			limit = gridGroup4.Length;
			for (i = 0; i < limit; i++)
			{
				group4Set = false;
				group4Contain0 = 0;
				for ( playerID = 0; playerID < playerLimit; playerID++)
				{			
					if (gridGroup4[i].playerContainCount[playerID] > 3)
					{
						particleSystemGroup[2].particleSystemArray[particleSystemGroup[2].count].position = gridGroup4[i].worldPosition;
						particleSystemGroup[2].particleSystemArray[particleSystemGroup[2].count].color = playerColor[playerID];		
						particleSystemGroup[2].count++;
						playerID = playerLimit;
						group4Set = true;
					}
					else if ( gridGroup4[i].playerContainCount[playerID] == 0)
					{
						group4Contain0++;
					}					
				}			
				
				if (!group4Set && group4Contain0 != playerLimit)
				{
					for (containIterate2 = 0; containIterate2 < 4; containIterate2++)
					{	
						for ( playerID = 0; playerID < playerLimit; playerID++)
						{									
							containIndex2 = gridGroup4[i].gridGroupContainIndex[containIterate2];
							if (gridGroupAll[containIndex2].playerContainCount[playerID] == 1)
							{
								particleSystemGroup[1].particleSystemArray[particleSystemGroup[1].count].position = gridGroupAll[containIndex2].worldPosition;
								particleSystemGroup[1].particleSystemArray[particleSystemGroup[1].count].color = playerColor[playerID];						
								particleSystemGroup[1].count++;	
								playerID = playerLimit;
							}
						}
					}						
				}
			}		
			
			limit = gridGroup1.Length;
			for (i = 0; i < limit; i++)
			{
				for ( playerID = 0; playerID < playerLimit; playerID++)
				{			
					if (gridGroup1[i].playerContainCount[playerID] == 1)
					{
						particleSystemGroup[1].particleSystemArray[particleSystemGroup[1].count].position = gridGroup1[i].worldPosition;
						particleSystemGroup[1].particleSystemArray[particleSystemGroup[1].count].color = playerColor[playerID];					
						particleSystemGroup[1].count++;	
					}
				}
			}
			
			//particleSystemsUpdated = false;
			//updatingParticleSystemArrays = false;
	//	}
	}
	
    
    //good luck figuring this out again, you were
    //fucking nuts when your wrote it
    
	private bool runParticlesMoveCycle;
	private bool particlesMoved;
	public int[,] aiIndex;
	public Vector3[,] aiWorldPosition;
	[HideInInspector]
	public bool aiUpdate;
	private float gameTimer;
	private void MoveParticles()
	{
		//recyclables
		int gridIndex;
		int nextGridIndex;
		
		int gridDirectionPrimary;///
		int[] gridDirectionalIndex;///
		int gridBelongTo16Group;
		int gridBelongTo4Group;	
		Vector3 gridWorldPosition;	///	
		
		int ngridInhabitedBy;	///
		int ngridParticleIndex;///	
		int ngridBelongTo16Group;///
		int ngridBelongTo4Group;///	
		int ngridX;///
		int ngridY;///
		Vector3 ngridWorldPosition;	///	
		
		int nparticlePlayer;
		
		int checkDirection;		
		int[] rotateDirectionMove = new int[9]{0,-1,1,-2,2,-3,3,-4,4};		
		int directionLimit = (sbyte)rotateDirectionMove.Length;
		int iDirection;			
		int particleIndex;
		int nextParticleIndex;
		int particleLimit = particleCount;
		int playerID;	
		float gameTimeLimit = this.gameTimeLimit;
		float gameTimer;
		
		//move particles
		int life = 0;
			
		int playerLimit = PlayerData.playerCount;
		int i;
		
		int[] aiIndexInterval = new int[PlayerData.playerCount];
		int[] aiIntervalCount = new int[PlayerData.playerCount];
		int[] intervalindex = new int[PlayerData.playerCount];
		int newInterval;
		
		while (runMoveParticlesThread)
		{
			while (!runParticlesMoveCycle)
			{
			}
			
			runParticlesMoveCycle = false;
			gameTimer =  this.gameTimer;
			
			//get intervals between random selected particles for ai
			for (i = 0; i < playerLimit; i++)
			{

				newInterval = PlayerData.player[i].particleCount-1;
				if (newInterval > 0)
				{
					if (newInterval % 2 != 0)
					{
						newInterval--;	
					}
					newInterval = newInterval / PlayerData.playerCount;
					aiIndexInterval[i] = newInterval;
				}
				else
				{
					aiIndexInterval[i] = 0;	
				}
				
				aiIntervalCount[i] = 0;
				intervalindex[i] = 0;

			}
			
			for(particleIndex = 0; particleIndex < particleLimit; particleIndex++)
			{	
				gridIndex = particle[particleIndex].gridIndex;
				playerID = particle[particleIndex].player;
				//updateColor = false;

				grid[gridIndex].GetValuesGrid(
					playerID, 
					out gridDirectionPrimary,
					out gridDirectionalIndex,
					out gridBelongTo16Group, 
					out gridBelongTo4Group,
					out gridWorldPosition);
				

				if (aiIndexInterval[playerID] != 0 && intervalindex[playerID] < PlayerData.playerCount && aiIntervalCount[playerID] == aiIndexInterval[playerID])
				{
					aiIndex[playerID,intervalindex[playerID]] = particleIndex;
					aiWorldPosition[playerID,intervalindex[playerID]] = gridWorldPosition;
					intervalindex[playerID]++;
					aiIntervalCount[playerID] = 0;
				}
				aiIntervalCount[playerID]++;
				

				
				for(iDirection = 0; iDirection < directionLimit; iDirection++)
				{
					checkDirection = gridDirectionPrimary;
					
					//rotate direction function, copied here to improve performance
					checkDirection += rotateDirectionMove[iDirection];
					if(checkDirection > 11)
					{
						checkDirection = (checkDirection-12);
					}
					else if(checkDirection < 0)
					{
						checkDirection = (12+checkDirection);	
					}
					
					nextGridIndex = gridDirectionalIndex[checkDirection];
					if (nextGridIndex != -1)
					{
						grid[nextGridIndex].GetValuesNextGrid(
							out ngridInhabitedBy,
							out ngridParticleIndex,
							out ngridBelongTo16Group, 
							out ngridBelongTo4Group,
							out ngridX,
							out ngridY,
							out ngridWorldPosition);
					
						//move particle
						if(ngridInhabitedBy == 254)
						{	
							grid[nextGridIndex].inhabitedBy = playerID;
							grid[gridIndex].inhabitedBy = 254;
							grid[nextGridIndex].particleIndex = particleIndex;
							grid[gridIndex].particleIndex = -1;

							particle[particleIndex].SetPosition(ngridX,ngridY,nextGridIndex,ngridWorldPosition);
							
							gridGroupAll[gridIndex].playerContainCount[playerID]--;
							gridGroupAll[nextGridIndex].playerContainCount[playerID]++;					
							
							if ( ngridBelongTo16Group != gridBelongTo16Group)
							{
								if ( gridBelongTo16Group != -1)					
									gridGroupAll[ gridBelongTo16Group ].playerContainCount[playerID]--;
								if (ngridBelongTo16Group != -1)
									gridGroupAll[ ngridBelongTo16Group ].playerContainCount[playerID]++;		
							}

							if (ngridBelongTo4Group != gridBelongTo4Group)
							{
								if (gridBelongTo4Group != -1)							
									gridGroupAll[ gridBelongTo4Group ].playerContainCount[playerID]--;
								if (ngridBelongTo4Group != -1)						
									gridGroupAll[ ngridBelongTo4Group ].playerContainCount[playerID]++;		
							}		
							
							iDirection = directionLimit;
						}
						
						//if cell isnt empty and has other player, take their life
						else if(playerID != ngridInhabitedBy && ngridInhabitedBy != 254)
						{
							nextParticleIndex = ngridParticleIndex;
							
							nparticlePlayer = particle[nextParticleIndex].player;
							
							if (playerID == winningPlayerID && gameTimer > gameTimeLimit)
							{
								life = ( particle[nextParticleIndex].life - ((5-rotateDirectionMove[iDirection])*(attackMultiplier*2)) );
							}
							else
							{
								life = ( particle[nextParticleIndex].life - ((5-rotateDirectionMove[iDirection])*attackMultiplier) );								
							}
							
							//particle died
							if(life <= 0)
							{
								//tick particle counts
								PlayerData.player[playerID].particleCount++;
								PlayerData.player[nparticlePlayer].particleCount--;
								
								gridGroupAll[nextGridIndex].playerContainCount[playerID]++;		
								gridGroupAll[nextGridIndex].playerContainCount[nparticlePlayer]--;
								
								if ( grid[nextGridIndex].belongTo16Group != -1)
								{
									gridGroupAll[ ngridBelongTo16Group ].playerContainCount[playerID]++;
									gridGroupAll[ ngridBelongTo16Group ].playerContainCount[nparticlePlayer]--;	
								}
								
								if ( grid[nextGridIndex].belongTo4Group != -1)
								{
									gridGroupAll[ ngridBelongTo4Group ].playerContainCount[playerID]++;
									gridGroupAll[ ngridBelongTo4Group ].playerContainCount[nparticlePlayer]--;			
								}									
								
								particle[nextParticleIndex].ChangePlayer(playerID);					
								grid[nextGridIndex].inhabitedBy = playerID;
							}		
							else
							{
								particle[nextParticleIndex].life = life;
							}
						
							if (iDirection > 2)
								iDirection = directionLimit;
						}	
						//if other cell is same player, give it additional life boost
						else if( playerID == ngridInhabitedBy )
						{	
							particle[ngridParticleIndex].AddLife();	
							if (iDirection > 2)
								iDirection = directionLimit;
						}
					}
				}
				
				//add life to all particles
				particle[particleIndex].AddLife();
						
			}
			
			particlesMoved = true;
			
		}
	}


	//0 - NNE
	//1 - NE
	//2 - NEE
	//3 - SEE
	//4 - SE
	//5 - SSE
	//6 - SSW
	//7 - SW
	//8 - SWW
	//9 - NWW
	//10 - NW
	//11 - NNW			
	

	private void BuildGroupGradient()
	{
		//recyclables
		int nextGroupIndex;
		int groupIndex;
		int gridIndex;
		int direction;	
		int directionPrimary;
		int directionInvert = 6;
		int i;
		int limit;	
		int i2;
		int limit2;	
		bool executeStepGradientArray = true;
		int currentIndex = 0;	
		int potentialNewGridIndex; 		
		
	  	int[] gradientStepArray;		
		int lastAddedIndex = 0;		
		int[] lastInputGridIndex = new int[ PlayerData.playerCount];	
		int[,] gradientStepPattern = new int[8,6]
		{
		{0,1,8,6,9,10},
		{1,3,4,7,5,11},
		{11,8,1,4,3,0},
		{8,9,7,5,1,3},
		{11,8,10,1,3,4},
		{2,9,11,1,3,7},
		{5,2,0,10,9,6},
		{5,3,1,11,8,7}
		};			
		int gradientStepPatternIndex = 0;
		//random arrangement of numbers
		int[] gradientStepStartPatten = new int[8]{2,3,5,6,1,7,4,0};
		int gradientStepStartPatternIndex = 0;
		
		
		//init gradient step array
		gradientStepArray = new int[gridParameters.length * 2];		
		for(i = 0; i < gradientStepArray.Length; i++)
		{
			gradientStepArray[i] = -1;
		}				
		
		
		while(runGradientThread)
		{			
			Thread.Sleep(0);			
			
			gradientStepPatternIndex = gradientStepStartPatten[gradientStepStartPatternIndex];
			gradientStepStartPatternIndex++;
			if(gradientStepStartPatternIndex == 8)
					gradientStepStartPatternIndex = 0;

			for(int playerID = 0; playerID < PlayerData.playerCount; playerID++)
			{
				//Reset Gradient Grid
				limit = gridGroup1.Length;
				for(i = 0; i < limit; i++)
				{		
					gridGroup1[i].setForCycle = false;
				}
				limit = gridGroup4.Length;
				for(i = 0; i < limit; i++)
				{		
					gridGroup4[i].setForCycle = false;
				}
				limit = gridGroup16.Length;
				for(i = 0; i < limit; i++)
				{		
					gridGroup16[i].setForCycle = false;
				}				
				
				lastAddedIndex = 0;		
			
				//Grab current input point
				potentialNewGridIndex = WorldPositionToGridIndex(inputHitPoint[playerID].x, inputHitPoint[playerID].z);
				if ( grid[potentialNewGridIndex].inhabitedBy != 255 )
				{
					gridIndex = potentialNewGridIndex;
					lastInputGridIndex[playerID] = gridIndex;
				}
				else
				{
					gridIndex = lastInputGridIndex[playerID];
				}
					
				if(gridIndex != -1)
				{
					groupIndex = grid[gridIndex].belongToTopGroupIndex;
					
					//first point added
					gradientStepArray[lastAddedIndex] = groupIndex;
					lastAddedIndex++;		
				
					executeStepGradientArray = true;
					currentIndex = 0;
					while(executeStepGradientArray)
					{
						if(gradientStepArray[currentIndex] != -1)
						{
							groupIndex = gradientStepArray[currentIndex];

							limit = 6;
							for(i = 0; i < limit; i++)
							{
								direction = gradientStepPattern[gradientStepPatternIndex,i];
								nextGroupIndex = gridGroupAll[groupIndex].directionalGroupIndex[ direction ];								
								if (nextGroupIndex != -1 && !gridGroupAll[nextGroupIndex].setForCycle)				
								{				
									directionPrimary = direction;
									
									//rotate direction function, copied here to improve performance
									directionPrimary += directionInvert;
									if(directionPrimary > 11)
									{
										directionPrimary = (directionPrimary-12);
									}
									else if(directionPrimary < 0)
									{
										directionPrimary = (12+directionPrimary);	
									}
									
									//gridGroup[nextGroupIndex].directionPrimary[playerID] = directionPrimary;
									limit2 = gridGroupAll[nextGroupIndex].gridContainIndex.Length;
									for (i2 = 0; i2 < limit2; i2++)
									{
										grid[ gridGroupAll[nextGroupIndex].gridContainIndex[i2] ].directionPrimary[playerID] = directionPrimary;
										
									}
									
									gridGroupAll[nextGroupIndex].setForCycle = true;
	
									gradientStepArray[lastAddedIndex] = nextGroupIndex;
									lastAddedIndex++;		
								}							
							}		
		
							gradientStepPatternIndex++;
							if(gradientStepPatternIndex == 8)
								gradientStepPatternIndex = 0;					
						
						
							gradientStepArray[currentIndex] = -1;
							currentIndex++;
						
							//this is a failsafe to catch end of array, may be unnecessary
							if(currentIndex == gradientStepArray.Length)
								currentIndex = 0;
						}
						else
						{
							executeStepGradientArray = false;	
						}
					}
				}
			}
		}
	}	
	
	private int WorldPositionToGridIndex(float x, float y)
	{		
		//for some reason if it went negative build gradient thread would crash, not sure why, shouldn't be this way
		if ( x < 0 )
			x = 0;
		if ( y < 0 )
			y = 0;
		
		if ( x > ((float)gridParameters.width-1f) / 10f)
			x = ((float)gridParameters.width-1f) / 10f;
		if ( y > ((float)gridParameters.height-1f) / 10f)
			y = ((float)gridParameters.height-1f) / 10f;
		
		int gridX = Mathf.RoundToInt(x * 10f);
		int gridY = Mathf.RoundToInt(y * 10f);		

		return CoordinateToGridIndex(gridX, gridY);
	}
    
    private int CoordinateToGridIndex( int x, int y, int xMax, int yMax )
    {       
        if( x >= xMax || y >= yMax )
            return -1;
        return ( ( y * xMax ) + x );
    }
	
	private int CoordinateToGridIndex( int x, int y )
	{		
		if( x >= gridParameters.width || y >= gridParameters.height )
			return -1;
		
		int gridIndex = ( ( y * gridParameters.width ) + x );
		if ( gridIndex < grid.Length )
		{		
			return gridIndex;
		}
		else
		{
			return ( grid.Length - 1 );	
		}
	}
	
	private void RotateDirection(ref int direction, ref int rotation)
	{
		direction += rotation;
		
		if(direction > 11)
		{
			direction = direction - 12;
		}
		else if(direction < 0)
		{
			direction = 12 + direction;	
		}
	}
	
	
	private void DirectionToXY(out int x, out int y, int direction)
	{
		switch (direction)
		{
		case 0:
			//N
			x = 0;
			y = 1;
			break;
			
		case 1:
			//NE
			x = 1;
			y = 1;					
			break;
			
		case 2:
			//E
			x = 1;
			y = 0;						
			break;
			
		case 3:
			//SE
			x = 1;
			y = -1;							
			break;
			
		case 4:
			//S
			x = 0;
			y = -1;						
			break;
			
		case 5:
			//SW
			x = -1;
			y = -1;						
			break;
			
		case 6:
			//W
			x = -1;
			y = 0;						
			break;
			
		case 7:
			//NW
			x = -1;
			y = -1;						
			break;
			
		default:
			x = 0;
			y = 0;
			break;			
		}			
	}		
	
	
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(new Vector3(((float)gridParameters.width/2f)/10f,0f,((float)gridParameters.height/2f)/10f),new Vector3((float)gridParameters.width/10f,0f,(float)gridParameters.height/10f));
		
		if(Application.isPlaying && drawDebugGrid)
		{		
			Vector3 direction = new Vector3();
			float color = .5f;
			int i;
			int limit = grid.Length;
			for(i = 0; i < limit; i++)
			{		
				//color = (float)grid[i].steps  / ((float)gridParameters.width);
				Gizmos.color = new Color(color,color/2f,color/3f,1f);
				
				int x;
				int y;
				DirectionToXY(out x,out y, grid[i].directionPrimary[0]);				
				
				direction.x = (float)x;
				direction.z = (float)y;
				
				Gizmos.DrawRay(grid[i].worldPosition, direction/10);
			}		
		}
		if(Application.isPlaying && drawDebugGroupGrid)
		{	
			Color color = new Color();
			int colorCounter = 0;
			for(int i = 0; i < gridGroupAll.Length; i++)
			{				
				colorCounter = (int)(Random.value*8);
				
				switch (colorCounter)
				{
				case 0:
					color = Color.black;
					break;
				case 1:
					color = Color.blue;
					break;
				case 2:
					color = Color.cyan;
					break;
				case 3:
					color = Color.green;
					break;
				case 4:
					color = Color.grey;
					break;
				case 5:
					color = Color.magenta;
					break;
				case 6:
					color = Color.red;
					break;
				case 7:
					color = Color.white;
					break;
				case 8:
					color = Color.yellow;
					break;
				default:
					break;
				}
				
				Gizmos.color = color;
				
				for (int i2 = 0; i2 < gridGroupAll[i].gridContainIndex.Length; i2++)
				{
					int containIndex = gridGroupAll[i].gridContainIndex[i2];
					Gizmos.DrawRay(grid[containIndex].worldPosition, Vector3.forward/10);
				}
			}				
		}
	}
	
	public IEnumerator EndGradientThreadDelay(float delay)
	{
		yield return new WaitForSeconds(delay);
		EndThreads();	
	}
	public void EndThreads()
	{

		if(gradientThread != null)
		{
			Debug.Log("Ending Gradient Thread");			
			runGradientThread = false;	
			//gradientThread.Abort();
			while(gradientThread != null && gradientThread.IsAlive)
			{
				//wait till thread dies
			}
		}
		

		if(moveParticlesThread != null)
		{
			Debug.Log("Ending Move Particle Thread");			
			runParticlesMoveCycle = true;
			runMoveParticlesThread = false;	
			//moveParticlesThread.Abort();
			while(moveParticlesThread != null && moveParticlesThread.IsAlive)
			{
				//wait till thread dies
			}
		}			
		
	}
}
