using UnityEngine;
using System.Collections;

namespace Partak
{
	/// <summary>
	/// Particle cell.
	/// 
	/// Cell which contains particle.
	/// 
	/// </summary>
	public class ParticleCell
	{
		/*
		Directions:
		0 - NNE
		1 - NE
		2 - NEE
		3 - SEE
		4 - SE
		5 - SSE
		6 - SSW
		7 - SW
		8 - SWW
		9 - NWW
		10 - NW
		11 - NNW		
		*/

		public readonly Vector3 WorldPosition;

		public readonly ParticleCellGrid ParticleCellGrid;

		public readonly ParticleCell[] DirectionalParticleCellArray;

		//stores the primary direction for each players particles
		public readonly int[] PrimaryDirectionArray;

		/// <summary>
		/// 0 1 2 3 player ID, -1 empty
		/// </summary>
		public int InhabitedBy { get; set; }

		/// <summary>
		/// Lowest level CellGroup this belongs to.
		/// Is possible for it to be the same as Top Cell Group.
		/// </summary>
		public CellGroup BottomCellGroup { get; set; }

		/// <summary>
		/// Highest level CellGroup this belongs to.
		/// </summary>
		public CellGroup TopCellGroup { get; set; }

		/// <summary>
		/// Store the index of the particle currently in this cell. -1 if no particle.
		/// </summary>
		public int ParticleIndex { get; set; }

		public ParticleCell(ParticleCellGrid particleCellGrid, Vector3 worldPosition)
		{
			DirectionalParticleCellArray = new ParticleCell[Direction12.Count]; //this could be 8 and just the groups run on 12
			PrimaryDirectionArray = new int[4];
			InhabitedBy = -1;
			this.ParticleCellGrid = particleCellGrid;
			this.WorldPosition = worldPosition;
		}
	}
}