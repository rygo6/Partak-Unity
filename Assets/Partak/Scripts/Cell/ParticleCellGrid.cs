using UnityEngine;

namespace GeoTetra.Partak
{
    public class ParticleCellGrid
    {
        private const int ObstacleLayer = 1 << 9;

        public readonly Vector2Int Dimension;

        public readonly ParticleCell[] Grid;

        public ParticleCellGrid(int x, int y)
        {
            Dimension = new Vector2Int(x, y);
            Grid = BuildParticleCellGrid(Dimension, this);
        }

        public ParticleCellGrid(Vector2Int dimension)
        {
            Dimension = dimension;
            Grid = BuildParticleCellGrid(dimension, this);
        }

        /// <summary>
        ///     Constructs a grid of ParticleCell
        ///     Particle cells are null if there is an obstacle blocking them.
        /// </summary>
        /// <returns>The ParticleCell grid.</returns>
        private ParticleCell[] BuildParticleCellGrid(Vector2Int dimension, ParticleCellGrid parentParticleCellGrid)
        {
            ParticleCell[] particleCellArray = new ParticleCell[dimension.x * dimension.y];
            
            //fill all slots in list
            int index = 0;
            for (int y = 0; y < dimension.y; ++y)
            {
                for (int x = 0; x < dimension.x; ++x)
                {
                    Vector3 position = new Vector2(x / 10f, y / 10f);
                    particleCellArray[index] = new ParticleCell(parentParticleCellGrid,new Vector3(position.x, 0f, position.y));
                    Ray ray = new Ray(new Vector3(position.x, 10f, position.y), Vector3.down);
                    if (Physics.Raycast(ray, 20f, ObstacleLayer))
                    {
                        particleCellArray[index].InhabitedBy = 255;
                    }
                    index++;
                }
            }

            //connect list together
            for (int i = 0; i < particleCellArray.Length; ++i)
            {
                ParticleCell cell = particleCellArray[i];
                for (int d = 0; d < Direction12.Count; ++d)
                {
                    Vector2Int coordinate =
                        CellUtility.GridIndexToCoordinate(i, dimension) + CellUtility.DirectionToXY(d);
                    int coordinateIndex = CellUtility.CoordinateToGridIndex(coordinate, dimension);
                    if (coordinateIndex != -1)
                        cell.DirectionalParticleCellArray[d] = particleCellArray[coordinateIndex];
                }
            }

            return particleCellArray;
        }

        public void DrawDebugRay()
        {
            for (int y = 0; y < Dimension.y; ++y)
            {
                for (int x = 0; x < Dimension.x; ++x)
                {
                    Vector3 position = new Vector2(x / 10f, y / 10f);
                    Ray ray = new Ray(new Vector3(position.x, 10f, position.y), Vector3.down);
                    if (Physics.Raycast(ray, 20f, ObstacleLayer))
                    {
                        Debug.DrawRay(ray.origin, ray.direction, Color.red);
                    }
                    else
                    {
                        Debug.DrawRay(ray.origin, ray.direction, Color.green);
                    }
                }
            }
        }
    }
}