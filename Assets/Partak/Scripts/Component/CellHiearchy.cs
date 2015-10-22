using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Partak
{
	public class CellHiearchy : MonoBehaviour 
	{
		private int parentCellGridLevel { get { return _parentCellGridLevel; } }
		[SerializeField]
		private int _parentCellGridLevel = 3;
	    
	    private Vector2Int rootDimension { get { return _rootDimension; } }
		[SerializeField]
		private Vector2Int _rootDimension = new Vector2Int(192, 192);
	
		public ParticleCellGrid particleCellGrid { get; private set; }

		public CellGroupGrid[] cellGroupGridArray { get; private set; }
	    
	    private void Awake()
	    {
			particleCellGrid = new ParticleCellGrid(rootDimension);
			cellGroupGridArray = new CellGroupGrid[parentCellGridLevel];
			cellGroupGridArray[0] = new CellGroupGrid(particleCellGrid);
			for (int i = 1; i < cellGroupGridArray.Length; ++i)
			{
				cellGroupGridArray[i] = new CellGroupGrid(cellGroupGridArray[i - 1]);
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