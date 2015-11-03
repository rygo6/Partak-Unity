using UnityEngine;
using System.Collections;

namespace Partak
{
	public class CellParticleMove : MonoBehaviour
	{
		[SerializeField]
		private CellParticleStore _cellParticleStore;

		static private readonly int[] RotateDirectionMove = new int[9]{ 0, -1, 1, -2, 2, -3, 3, -4, 4 };

		[SerializeField]
		public int _attackMultiplier = 1;

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
			int directionLimit = RotateDirectionMove.Length;
			int checkDirection;
			int d;
			int p;
			int life;

			for (p = 0; p < limit; ++p)
			{
				for (d = 0; d < directionLimit; ++d)
				{
					currentCellParticle = cellParticleArray[p];
					currentParticleCell = currentCellParticle.ParticleCell;

					checkDirection = CellUtility.RotateDirection(
						currentCellParticle.ParticleCell.PrimaryDirectionArray[currentCellParticle.PlayerIndex], 
						RotateDirectionMove[d]);

					nextParticleCell = currentParticleCell.DirectionalParticleCellArray[checkDirection];
						
					if (nextParticleCell != null)
					{
						//move
						if (nextParticleCell.InhabitedBy == -1)
						{
							currentCellParticle.ParticleCell = nextParticleCell;
							d = directionLimit;
						}
						//if other player, take life
						else if (currentParticleCell.InhabitedBy != nextParticleCell.InhabitedBy)
						{	

//								life = ( particle[nextParticleIndex].life - ((5-rotateDirectionMove[iDirection])*(attackMultiplier*2)) );
							life = nextParticleCell.CellParticle.Life - ((5 - Mathf.Abs(RotateDirectionMove[d])) * _attackMultiplier);								

							if (life <= 0)
							{
								//tick particle counts
//								PlayerData.player[playerID].particleCount++;
//								PlayerData.player[nparticlePlayer].particleCount--;

								nextParticleCell.CellParticle.ChangePlayer(currentCellParticle.PlayerIndex);
							}
							else
							{
								nextParticleCell.CellParticle.Life = life;
							}

							if (d > 2)
								d = directionLimit;
						}
						//if other cell is same player, give it additional life boost
						else if (currentParticleCell.InhabitedBy == nextParticleCell.InhabitedBy)
						{	
							nextParticleCell.CellParticle.Life++;
							if (d > 2)
								d = directionLimit;
						}


					}


				}
			}


		}

	}
}