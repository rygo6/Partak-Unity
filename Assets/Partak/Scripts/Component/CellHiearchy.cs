using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Partak
{
	public class CellHiearchy : MonoBehaviour
	{
		private int ParentCellGridLevel { get { return _parentCellGridLevel; } }
		[SerializeField]
		private int _parentCellGridLevel = 3;
	    
		private Vector2Int RootDimension { get { return _rootDimension; } }
		[SerializeField]
		private Vector2Int _rootDimension = new Vector2Int(192, 192);
	
		public ParticleCellGrid ParticleCellGrid { get; private set; }

		public CellGroupGrid[] CellGroupGridArray { get; private set; }
	    
		private void Awake()
		{
			ParticleCellGrid = new ParticleCellGrid(RootDimension);
			CellGroupGridArray = new CellGroupGrid[ParentCellGridLevel];
			CellGroupGridArray[0] = new CellGroupGrid(ParticleCellGrid);
			for (int i = 1; i < CellGroupGridArray.Length; ++i)
			{
				CellGroupGridArray[i] = new CellGroupGrid(CellGroupGridArray[i - 1]);
			}
		}
	    
		private void Start()
		{
	    
		}
	    
		private void Update()
		{

		}
	}
}