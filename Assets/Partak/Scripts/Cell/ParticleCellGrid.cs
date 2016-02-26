using UnityEngine;

namespace Partak {
public class ParticleCellGrid {

	public readonly ParticleCell[] Grid;

	public readonly Vector2Int Dimension;

	const int ObstacleLayer = 1 << 9;

	public ParticleCellGrid(int x, int y) {
		Dimension = new Vector2Int(x, y);
		this.Grid = BuildParticleCellGrid(Dimension, this);
	}

	public ParticleCellGrid(Vector2Int dimension) {
		this.Dimension = dimension;
		this.Grid = BuildParticleCellGrid(dimension, this);
	}

	/// <summary>
	/// Constructs a grid of ParticleCell
	/// Particle cells are null if there is an obstacle blocking them.
	/// </summary>
	/// <returns>The ParticleCell grid.</returns>
	private ParticleCell[] BuildParticleCellGrid(Vector2Int dimension, ParticleCellGrid parentParticleCellGrid) {
		ParticleCell[] particleCellArray = new ParticleCell[dimension.X * dimension.Y]; 

		//fill all slots in list
		int index = 0;
		for (int y = 0; y < dimension.Y; ++y) {
			for (int x = 0; x < dimension.X; ++x) {
				Vector3 position = new Vector2((float)x / 10f, (float)y / 10f);
				particleCellArray[index] = new ParticleCell(
						parentParticleCellGrid, 
						new Vector3(position.x, 0f, position.y));
				Ray ray = new Ray(new Vector3(position.x, 10f, position.y), Vector3.down);
				if (Physics.Raycast(ray, 20f, ObstacleLayer)) {
					particleCellArray[index].InhabitedBy = 255;
				}
				index++;
			}
		}

		//connect list together
		for (int i = 0; i < particleCellArray.Length; ++i) {
			ParticleCell cell = particleCellArray[i];
			for (int d = 0; d < Direction12.Count; ++d) {
				Vector2Int coordinate = CellUtility.GridIndexToCoordinate(i, dimension) + CellUtility.DirectionToXY(d);
				int coordinateIndex = CellUtility.CoordinateToGridIndex(coordinate, dimension);
				if (coordinateIndex != -1) {
					cell.DirectionalParticleCellArray[d] = particleCellArray[coordinateIndex];
				}
			}
		}

		return particleCellArray;
	}
}
}