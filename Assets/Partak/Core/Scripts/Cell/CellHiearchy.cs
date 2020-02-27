using System;
using Amazon.Auth.AccessControlPolicy.ActionIdentifiers;
using GeoTetra.GTPooling;
using UnityEngine;

namespace GeoTetra.Partak
{
    /// <summary>
    /// Entry point of quadtree data structure.
    /// </summary>
    public class CellHiearchy : MonoBehaviour
    {
        [SerializeField] private ServiceReference _gameStateReference;
        [SerializeField] private LevelConfig _levelConfig;
        private GameState _gameState;

        public int ParentCellGridLevel { get; private set; }
        public ParticleCellGrid ParticleCellGrid { get; private set; }
        public CellGroupGrid[] CellGroupGrids { get; private set; }
        public CellGroup[] CombinedFlatCellGroups { get; private set; }

        private void Awake()
        {
            _gameState = _gameStateReference.Service<GameState>();
        }

        public void Initialize()
        {
            Debug.Log("Initialize Cell Hierarchy");
            
            int x = _levelConfig.Datum.LevelSize.x;
            int y = _levelConfig.Datum.LevelSize.y;
            if (x == 0 || y == 0)
            {
                Debug.LogError("Dimensions are " + x + "  " + y);
                return;
            }
            
            while (x % 2 == 0 && y % 2 == 0)
            {
                x /= 2;
                y /= 2;
                ParentCellGridLevel++;
            }

            ParentCellGridLevel++;

            Vector2Int rootDimension = _levelConfig.Datum.LevelSize;
            ParticleCellGrid = new ParticleCellGrid(rootDimension);
            CellGroupGrids = new CellGroupGrid[ParentCellGridLevel];
            CellGroupGrids[0] = new CellGroupGrid(ParticleCellGrid, _gameState.PlayerCount());
            for (int i = 1; i < CellGroupGrids.Length; ++i)
                CellGroupGrids[i] = new CellGroupGrid(CellGroupGrids[i - 1], _gameState.PlayerCount());

            int combinedFlatCellGroupCount = 0;
            for (int i = 0; i < CellGroupGrids.Length; ++i)
            {
                CellGroupGrids[i].FlattenGrid();
                combinedFlatCellGroupCount += CellGroupGrids[i].FlatGrid.Length;
            }

            CombinedFlatCellGroups = new CellGroup[combinedFlatCellGroupCount];
            int combinedFlatCellGroupIndex = 0;
            for (int i = 0; i < CellGroupGrids.Length; ++i)
            for (int o = 0; o < CellGroupGrids[i].FlatGrid.Length; ++o)
            {
                CombinedFlatCellGroups[combinedFlatCellGroupIndex] = CellGroupGrids[i].FlatGrid[o];
                combinedFlatCellGroupIndex++;
            }

            for (int i = 0; i < CellGroupGrids.Length; ++i)
            for (int o = 0; o < CellGroupGrids[i].Grid.Length; ++o)
                if (CellGroupGrids[i].Grid[o] != null)
                    CellGroupGrids[i].Grid[o].FillParentCellGroups();
        }

        private void OnDrawGizmosSelected()
        {
            ParticleCellGrid?.DrawDebugRay();
        }
    }
}