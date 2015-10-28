﻿using UnityEngine;
using System.Collections;

namespace Partak
{
	public class CellParticleMove : MonoBehaviour
	{
		[SerializeField]
		private CellParticleStore _cellParticleStore;

		static private readonly int[] RotateDirectionMove = new int[9]{ 0, -1, 1, -2, 2, -3, 3, -4, 4 };

		private void Update()
		{
			MoveParticles();
		}

		private void MoveParticles()
		{
			CellParticle[] cellParticleArray = _cellParticleStore.CellParticleArray;
			CellParticle currentCellParticle;
			ParticleCell currentParticleCell;
			ParticleCell nextParticleCell;
			int limit = cellParticleArray.Length;
			int directionLimit = cellParticleArray.Length;
			int checkDirection;

			for (int p = 0; p < limit; ++p)
			{
				for (int d = 0; d < directionLimit; ++d)
				{
					currentCellParticle = cellParticleArray[p];
					currentParticleCell = currentCellParticle.ParticleCell;

					checkDirection = CellUtility.RotateDirection(
						currentCellParticle.ParticleCell.PrimaryDirectionArray[currentCellParticle.PlayerIndex], 
						RotateDirectionMove[d]);

					nextParticleCell = currentParticleCell.DirectionalParticleCellArray[checkDirection];
						
					if (nextParticleCell != null)
					{
						if (nextParticleCell.InhabitedBy == -1)
						{
							nextParticleCell.CellParticle = currentCellParticle;
//							nextParticleCell.InhabitedBy = currentCellParticle.PlayerIndex;
//							nextParticleCell.a

						}
					}

				}
			}

		}

	}
}