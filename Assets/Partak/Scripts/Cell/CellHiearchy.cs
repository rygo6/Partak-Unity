using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Partak
{
	/// <summary>
	/// Cell hiearchy.
	/// Root store for the quadtree-esque data structure.
	/// </summary>
	public class CellHiearchy : MonoBehaviour
	{
		[SerializeField]
		private int _parentCellGridLevel = 3;
	    
		[SerializeField]
		private Vector2Int _rootDimension = new Vector2Int(192, 192);
	
		public ParticleCellGrid ParticleCellGrid { get; private set; }

		public CellGroupGrid[] CellGroupGridArray { get; private set; }
	    
		private void Awake()
		{
			ParticleCellGrid = new ParticleCellGrid(_rootDimension);
			CellGroupGridArray = new CellGroupGrid[_parentCellGridLevel];
			CellGroupGridArray[0] = new CellGroupGrid(ParticleCellGrid);
			for (int i = 1; i < CellGroupGridArray.Length; ++i)
			{
				CellGroupGridArray[i] = new CellGroupGrid(CellGroupGridArray[i - 1]);
			}
		}
	}
}