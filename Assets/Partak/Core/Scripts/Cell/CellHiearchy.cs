﻿using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Amazon.Auth.AccessControlPolicy.ActionIdentifiers;
using GeoTetra.GTCommon.Components;
using GeoTetra.GTPooling;
using UnityEngine;

namespace GeoTetra.Partak
{
    /// <summary>
    /// Entry point of quadtree data structure.
    /// </summary>
    public class CellHiearchy : SubscribableBehaviour
    {
        [SerializeField] private PartakStateRef _partakState;
        [SerializeField] private LevelConfig _levelConfig;

        public int ParentCellGridLevel { get; private set; }
        public ParticleCellGrid ParticleCellGrid { get; private set; }
        public CellGroupGrid[] CellGroupGrids { get; private set; }
        public CellGroup[] CombinedFlatCellGroups { get; private set; }

        public async Task Initialize()
        {
            Debug.Log("Initialize Cell Hierarchy");
            await _partakState.Cache(this);
            
            int x = _levelConfig.Datum.LevelSize.x;
            int y = _levelConfig.Datum.LevelSize.y;
            if (x == 0 || y == 0)
            {
                Debug.LogError("Dimensions are " + x + "  " + y);
                return;
            }

            ParentCellGridLevel = 0;
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
            CellGroupGrids[0] = new CellGroupGrid(ParticleCellGrid, _partakState.Ref.PlayerCount());
            for (int i = 1; i < CellGroupGrids.Length; ++i)
                CellGroupGrids[i] = new CellGroupGrid(CellGroupGrids[i - 1], _partakState.Ref.PlayerCount());

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