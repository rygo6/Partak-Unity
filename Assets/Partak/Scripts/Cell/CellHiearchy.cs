using UnityEngine;

namespace Partak {
public class CellHiearchy : MonoBehaviour {

	int _parentCellGridLevel;

	public int ParentCellGridLevel { get { return _parentCellGridLevel; } }

	public ParticleCellGrid ParticleCellGrid { get; private set; }

	public CellGroupGrid[] CellGroupGrids { get; private set; }

	public CellGroup[] CombinedFlatCellGroups { get; private set; }
		    	    	    
	void Awake() {
		LevelConfig levelConfig = GameObject.FindObjectOfType<LevelConfig>();
		int x = levelConfig.RootDimension.X;
		int y = levelConfig.RootDimension.Y;
		while (x % 2 == 0 && y % 2 == 0) {
			x /= 2;
			y /= 2;
			_parentCellGridLevel++;
		}
		_parentCellGridLevel++;

		Vector2Int rootDimensin = FindObjectOfType<LevelConfig>().RootDimension;
		ParticleCellGrid = new ParticleCellGrid(rootDimensin);
		CellGroupGrids = new CellGroupGrid[_parentCellGridLevel];
		CellGroupGrids[0] = new CellGroupGrid(ParticleCellGrid);
		for (int i = 1; i < CellGroupGrids.Length; ++i) {
			CellGroupGrids[i] = new CellGroupGrid(CellGroupGrids[i - 1]);
		}

		int combinedFlatCellGroupCount = 0;
		for (int i = 0; i < CellGroupGrids.Length; ++i) {
			CellGroupGrids[i].FlattenGrid();
			combinedFlatCellGroupCount += CellGroupGrids[i].FlatGrid.Length;
		}

		CombinedFlatCellGroups = new CellGroup[combinedFlatCellGroupCount];
		int combinedFlatCellGroupIndex = 0;
		for (int i = 0; i < CellGroupGrids.Length; ++i) {
			for (int o = 0; o < CellGroupGrids[i].FlatGrid.Length; ++o) {
				CombinedFlatCellGroups[combinedFlatCellGroupIndex] = CellGroupGrids[i].FlatGrid[o];
				combinedFlatCellGroupIndex++;
			}
		}
				
		for (int i = 0; i < CellGroupGrids.Length; ++i) {
			for (int o = 0; o < CellGroupGrids[i].Grid.Length; ++o) {
				if (CellGroupGrids[i].Grid[o] != null)
					CellGroupGrids[i].Grid[o].FillParentCellGroups();
			}
		}
	}
}
}