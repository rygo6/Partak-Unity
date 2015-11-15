using UnityEngine;
using System.Collections;

namespace Partak
{
	public class CellGroupGrid
	{
		public CellGroup[] Grid { get; private set; }
		
		public CellGroup[] FlatGrid { get; private set; }

		public readonly Vector2Int Dimension;

		public readonly int ParentLevel;

		public CellGroupGrid(CellGroupGrid cellGroupGrid)
		{
			ParentLevel = cellGroupGrid.ParentLevel + 1;
			Dimension = cellGroupGrid.Dimension / 2;
			Grid = BuildParentCellGroupLayer(cellGroupGrid, this);
			FlatGrid = FlattenGrid(Grid);
		}

		public CellGroupGrid(ParticleCellGrid particleCellGrid)
		{
			Dimension = particleCellGrid.Dimension;
			Grid = BuildCellGroupLayerFromParticleCellGrid(particleCellGrid, this);
			FlatGrid = FlattenGrid(Grid);
		}
			
		/// <summary>
		/// Flattens the grid.
		/// Generates a cellGroupArray with no null entries
		/// and no entries which have a parent. Flattened grid
		/// representing the minimum cells one must iterate through
		/// to touch every ParticleCell in the CellHiearchy.
		/// </summary>
		/// <returns>The flattened grid.</returns>
		/// <param name="cellGroupArray">Cell group array to flatten.</param>
		public CellGroup[] FlattenGrid(CellGroup[] cellGroupArray)
		{
			int nonNullCount = 0;
			for (int i = 0; i < Grid.Length; ++i)
			{
				if (Grid[i] != null && Grid[i].ParentCellGroup == null)
				{
					nonNullCount++;
				}
			}

			CellGroup[] flattenedGrid = new CellGroup[nonNullCount];

			int flattenedIndex = 0;
			for (int i = 0; i < Grid.Length; ++i)
			{
				if (Grid[i] != null && Grid[i].ParentCellGroup == null)
				{
					flattenedGrid[flattenedIndex] = Grid[i];
					flattenedIndex++;
				}
			}

			return flattenedGrid;
		}

		/// <summary>
		/// Builds the first CellGroup layer from the ParticleCellGrid
		/// </summary>
		/// <returns>The CellGroup Layer.</returns>
		/// <param name="particleCellArray">Particle cell Grid.</param>
		private CellGroup[] BuildCellGroupLayerFromParticleCellGrid(ParticleCellGrid particleCellGrid, CellGroupGrid parentCellGroupGrid)
		{
			ParticleCell[] particleCellGridArray = particleCellGrid.Grid;
			CellGroup[] cellGroupArray = new CellGroup[particleCellGridArray.Length];

			//copy particleCellGrid to cellGroupList
			for (int i = 0; i < particleCellGridArray.Length; ++i)
			{
				if (particleCellGridArray[i] != null)
				{
					cellGroupArray[i] = new CellGroup(
						parentCellGroupGrid, 
						null,
						new ParticleCell[1]{ particleCellGridArray[i] });
					particleCellGridArray[i].BottomCellGroup = cellGroupArray[i];
				}
			}

			//connect cellGroupList together
			for (int i = 0; i < cellGroupArray.Length; ++i)
			{
				if (cellGroupArray[i] != null)
				{
					CellGroup cellGroup = cellGroupArray[i];
					for (int d = 0; d < Direction12.Count; ++d)
					{
						Vector2Int coordinate = CellUtility.GridIndexToCoordinate(i, particleCellGrid.Dimension) + CellUtility.DirectionToXY(d);
						int coordinateIndex = CellUtility.CoordinateToGridIndex(coordinate, particleCellGrid.Dimension);
						if (coordinateIndex != -1)
						{
							cellGroup.DirectionalCellGroupArray[d] = cellGroupArray[coordinateIndex];
						}
					}
				}
			}

			return cellGroupArray;
		}

		/// <summary>
		/// Builds subsequent CellGroup Layers. Function can be run as many times
		/// as desired to build as many parent CellGroup layers as desired.
		/// </summary>
		/// <returns>The parent cell group layer.</returns>
		/// <param name="baseCellGroupArray">Base CellGroup layer array.</param>
		/// <param name="cellGroupDimension">Dimension of baseCellGroup.</param>
		private CellGroup[] BuildParentCellGroupLayer(CellGroupGrid baseCellGroupGrid, CellGroupGrid parentCellGroupGrid)
		{
			CellGroup[] baseCellGroupArray = baseCellGroupGrid.Grid;
			Vector2Int baseCellGroupDimension = baseCellGroupGrid.Dimension;
			int baseParentLevel = baseCellGroupGrid.ParentLevel;
			Vector2Int parentCellGroupDimension = baseCellGroupDimension / 2;
			CellGroup[] parentCellGroupGridArray = new CellGroup[parentCellGroupDimension.x * parentCellGroupDimension.y];

			int parentIndex = 0;
			for (int y = 0; y < baseCellGroupDimension.y; y += 2)
			{
				for (int x = 0; x < baseCellGroupDimension.x; x += 2)
				{
					//Gather 4 cellGroups that would compose parent cellGroup
					CellGroup[] childCellGroupArray = {
						baseCellGroupArray[CellUtility.CoordinateToGridIndex(x, y, baseCellGroupDimension)],
						baseCellGroupArray[CellUtility.CoordinateToGridIndex(x + 1, y, baseCellGroupDimension)],
						baseCellGroupArray[CellUtility.CoordinateToGridIndex(x, y + 1, baseCellGroupDimension)],
						baseCellGroupArray[CellUtility.CoordinateToGridIndex(x + 1, y + 1, baseCellGroupDimension)]
					};

					//check if any are null
					bool nullQuad = false;
					for (int i = 0; i < childCellGroupArray.Length; ++i)
					{
						if (childCellGroupArray[i] == null)
						{
							nullQuad = true;
							break;
						}
					}

					//if not null check if any neighboring cellGroups are more than one parentLevel difference
					bool neighborParentLevelTooSmall = false;
					if (!nullQuad)
					{
						CellGroup[] neigborCellGroupArray = {
							childCellGroupArray[3].DirectionalCellGroupArray[Direction12.NNW],
							childCellGroupArray[3].DirectionalCellGroupArray[Direction12.NNE],//0
							childCellGroupArray[3].DirectionalCellGroupArray[Direction12.NE],//1
							childCellGroupArray[3].DirectionalCellGroupArray[Direction12.NEE],//2
							childCellGroupArray[3].DirectionalCellGroupArray[Direction12.SEE],

							childCellGroupArray[1].DirectionalCellGroupArray[Direction12.NEE],
							childCellGroupArray[1].DirectionalCellGroupArray[Direction12.SEE],//3
							childCellGroupArray[1].DirectionalCellGroupArray[Direction12.SE],//4
							childCellGroupArray[1].DirectionalCellGroupArray[Direction12.SSE],//5
							childCellGroupArray[1].DirectionalCellGroupArray[Direction12.SSW],

							childCellGroupArray[0].DirectionalCellGroupArray[Direction12.SSE],
							childCellGroupArray[0].DirectionalCellGroupArray[Direction12.SSW],//6
							childCellGroupArray[0].DirectionalCellGroupArray[Direction12.SW],//7
							childCellGroupArray[0].DirectionalCellGroupArray[Direction12.SWW],//8
							childCellGroupArray[0].DirectionalCellGroupArray[Direction12.NWW],

							childCellGroupArray[2].DirectionalCellGroupArray[Direction12.SWW],
							childCellGroupArray[2].DirectionalCellGroupArray[Direction12.NWW],//9
							childCellGroupArray[2].DirectionalCellGroupArray[Direction12.NW],//10
							childCellGroupArray[2].DirectionalCellGroupArray[Direction12.NNW],//11
							childCellGroupArray[2].DirectionalCellGroupArray[Direction12.NNE],
						};

						for (int i = 0; i < neigborCellGroupArray.Length; ++i)
						{
							if (neigborCellGroupArray[i] == null)
							{

							}
							//parent level is 1 less to the cellGroup that will be created
							else if (neigborCellGroupArray[i].CellGroupGrid.ParentLevel == baseParentLevel)
							{

							}
							//parent level is equal to the cellGroup that will be created
							else if (neigborCellGroupArray[i].CellGroupGrid.ParentLevel == baseParentLevel + 1)
							{

							}
							//parent level is 2 less to the cellGroup that will be created
							else if (neigborCellGroupArray[i].CellGroupGrid.ParentLevel == baseParentLevel - 1)
							{
								neighborParentLevelTooSmall = true;
								break;
							}
							else
							{
								Debug.LogError("Neigbor Cell " + i + " has greater than 1 one parent level difference. neigborParentLevel: " +
								neigborCellGroupArray[i].CellGroupGrid.ParentLevel + " baseParentLevel: " + baseParentLevel);
							}
						}
					}

					//If all is valid, construct parent cell
					if (!nullQuad && !neighborParentLevelTooSmall)
					{
						//Get child particleCells of Quad
						ParticleCell[] childParticleCellArray = new ParticleCell[childCellGroupArray[0].ChildParticleCellArray.Length * 4];
						int particleIndex = 0;
						for (int i = 0; i < childCellGroupArray.Length; ++i)
						{
							for (int o = 0; o < childCellGroupArray[i].ChildParticleCellArray.Length; ++o)
							{
								childParticleCellArray[particleIndex] = childCellGroupArray[i].ChildParticleCellArray[o];
								particleIndex++;
							}
						}

						CellGroup parentCellGroup = new CellGroup(
							                            parentCellGroupGrid,
							                            childCellGroupArray,
							                            childParticleCellArray);

						ConnectStraightNeighborCellGroup(
							parentCellGroup, 
							childCellGroupArray[0],
							Direction12.SWW, 
							Direction12.SEE, 
							Direction12.NEE, 
							Direction12.NE);

						ConnectDiagonalNeighborCellGroup(
							parentCellGroup, 
							childCellGroupArray[0],
							Direction12.SW);

						ConnectStraightNeighborCellGroup(
							parentCellGroup, 
							childCellGroupArray[0],
							Direction12.SSW, 
							Direction12.NNW, 
							Direction12.NNE, 
							Direction12.NE);

						ConnectStraightNeighborCellGroup(
							parentCellGroup, 
							childCellGroupArray[1],
							Direction12.SSE, 
							Direction12.NNE, 
							Direction12.NNW, 
							Direction12.NW);

						ConnectDiagonalNeighborCellGroup(
							parentCellGroup, 
							childCellGroupArray[1],
							Direction12.SE);

						ConnectStraightNeighborCellGroup(
							parentCellGroup,
							childCellGroupArray[1],
							Direction12.SEE, 
							Direction12.SWW, 
							Direction12.NWW, 
							Direction12.NW);

						ConnectStraightNeighborCellGroup(
							parentCellGroup,
							childCellGroupArray[3],
							Direction12.NEE, 
							Direction12.NWW, 
							Direction12.SWW, 
							Direction12.SW);

						ConnectDiagonalNeighborCellGroup(
							parentCellGroup,
							childCellGroupArray[3],
							Direction12.NE);

						ConnectStraightNeighborCellGroup(
							parentCellGroup, 
							childCellGroupArray[3],
							Direction12.NNE, 
							Direction12.SSE, 
							Direction12.SSW, 
							Direction12.SW);

						ConnectStraightNeighborCellGroup(
							parentCellGroup, 
							childCellGroupArray[2],
							Direction12.NNW, 
							Direction12.SSW, 
							Direction12.SSE, 
							Direction12.SE);

						ConnectDiagonalNeighborCellGroup(
							parentCellGroup, 
							childCellGroupArray[2],
							Direction12.NW);

						ConnectStraightNeighborCellGroup(
							parentCellGroup, 
							childCellGroupArray[2],
							Direction12.NWW, 
							Direction12.NEE, 
							Direction12.SEE, 
							Direction12.SE);

						parentCellGroupGridArray[parentIndex] = parentCellGroup;
					}
					parentIndex++;
				}
			}	

			return parentCellGroupGridArray;
		}

		private void ConnectStraightNeighborCellGroup(
			CellGroup parentCellGroup, 
			CellGroup baseCellGroup,
			int neighborDirection,
			int oppositeDirectionA,
			int oppositeDirectionB,
			int diagonalDirection)
		{
			CellGroup neighborCellGroup = baseCellGroup.DirectionalCellGroupArray[neighborDirection];
			parentCellGroup.DirectionalCellGroupArray[neighborDirection] = neighborCellGroup;
			if (neighborCellGroup != null)
			{
				if (neighborCellGroup.CellGroupGrid.ParentLevel == parentCellGroup.CellGroupGrid.ParentLevel)
				{
					neighborCellGroup.DirectionalCellGroupArray[oppositeDirectionA] = parentCellGroup;
				}
				else if (neighborCellGroup.CellGroupGrid.ParentLevel == parentCellGroup.CellGroupGrid.ParentLevel - 1)
				{					
					neighborCellGroup.DirectionalCellGroupArray[oppositeDirectionA] = parentCellGroup;
					neighborCellGroup.DirectionalCellGroupArray[oppositeDirectionB] = parentCellGroup;
					neighborCellGroup.DirectionalCellGroupArray[diagonalDirection] = parentCellGroup;
				}
				else if (neighborCellGroup.CellGroupGrid.ParentLevel == parentCellGroup.CellGroupGrid.ParentLevel + 1)
				{
					Debug.LogError("neighborCellGroup.parentLevel == parentCellGroup.parentLevel + 1");
				}
				else
				{
					Debug.LogError("Neigbor Cell has greater than 1 one parent level difference");
				}
			}
		}

		private void ConnectDiagonalNeighborCellGroup(CellGroup parentCellGroup, CellGroup baseCellGroup, int neighborDirection)
		{
			CellGroup neighborCellGroup = baseCellGroup.DirectionalCellGroupArray[neighborDirection];				
			parentCellGroup.DirectionalCellGroupArray[neighborDirection] = neighborCellGroup;
			if (neighborCellGroup != null)
			{
				neighborCellGroup.DirectionalCellGroupArray[CellUtility.InvertDirection(neighborDirection)] = parentCellGroup;
			}
		}
	}
}