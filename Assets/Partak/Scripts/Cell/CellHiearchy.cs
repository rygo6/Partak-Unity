using UnityEngine;

namespace Partak
{
    /// <summary>
    /// Cell hiearchy.
    /// Root store for the quadtree-esque data structure.
    /// </summary>
    public class CellHiearchy : MonoBehaviour
	{
		public int ParentCellGridLevel { get { return _parentCellGridLevel; } }

		[SerializeField]
		private int _parentCellGridLevel = 3;
	
		public ParticleCellGrid ParticleCellGrid { get; private set; }

		public CellGroupGrid[] CellGroupGrids { get; private set; }

		public CellGroup[] CombinedFlatCellGroups { get; private set; }
	    
		private void Awake()
		{
			Vector2Int rootDimensin = FindObjectOfType<LevelConfig>().RootDimension;
			ParticleCellGrid = new ParticleCellGrid(rootDimensin);
			CellGroupGrids = new CellGroupGrid[_parentCellGridLevel];
			CellGroupGrids[0] = new CellGroupGrid(ParticleCellGrid);
			for (int i = 1; i < CellGroupGrids.Length; ++i)
			{
				CellGroupGrids[i] = new CellGroupGrid(CellGroupGrids[i - 1]);
			}

			int combinedFlatCellGroupCount = 0;
			for (int i = 0; i < CellGroupGrids.Length; ++i)
			{
				CellGroupGrids[i].FlattenGrid();
				combinedFlatCellGroupCount += CellGroupGrids[i].FlatGrid.Length;
			}

			CombinedFlatCellGroups = new CellGroup[combinedFlatCellGroupCount];
			int combinedFlatCellGroupIndex = 0;
			for (int i = 0; i < CellGroupGrids.Length; ++i)
			{
				for (int o = 0; o < CellGroupGrids[i].FlatGrid.Length; ++o)
				{
					CombinedFlatCellGroups[combinedFlatCellGroupIndex] = CellGroupGrids[i].FlatGrid[o];
					combinedFlatCellGroupIndex++;
				}
			}
				
			for (int i = 0; i < CellGroupGrids.Length; ++i)
			{
				for (int o = 0; o < CellGroupGrids[i].Grid.Length; ++o)
				{
					if (CellGroupGrids[i].Grid[o] != null)
						CellGroupGrids[i].Grid[o].FillParentCellGroups();
				}
			}
		}
	}
}