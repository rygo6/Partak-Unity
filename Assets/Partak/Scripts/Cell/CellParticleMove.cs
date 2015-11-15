using UnityEngine;
using System.Collections;
using System.Threading;
using System.Diagnostics;

namespace Partak
{
	public class CellParticleMove : MonoBehaviour
	{
		[SerializeField]
		private CellParticleStore _cellParticleStore;

		static private readonly int[] RotateDirectionMove = new int[9]{ 0, -1, 1, -2, 2, -3, 3, -4, 4 };

		private bool _processCycle;

		private bool _run;

		private Thread _thread;

		[SerializeField]
		public int _attackMultiplier = 1;

		private long _time;

		private void FixedUpdate()
		{
			_processCycle = true;
		}

		private void OnDestroy()
		{
			StopThread();
		}

		public void StartThread()
		{
			_thread = new Thread(RunThread);
			_thread.Priority = System.Threading.ThreadPriority.Highest;
			_run = true;
			_thread.Start();
		}

		private void StopThread()
		{
			_run = false;
			_thread.Abort();
			while (_thread.IsAlive)
			{
			}
		}

		private void RunThread()
		{
			while (_run) {
				while (!_processCycle) {
				}
				_processCycle = false;
				MoveParticles ();
			}
		}

		private void RunTimedThread()
		{
			Stopwatch stopWatch = new Stopwatch();
			while (_run)
			{
				stopWatch.Reset ();
				while (!_processCycle)
				{
				}
				stopWatch.Start ();
				_processCycle = false;
				MoveParticles();
				_time = stopWatch.ElapsedMilliseconds;
				stopWatch.Stop ();
			}
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
							life = nextParticleCell.CellParticle.Life - ((5 - Mathf.Abs(RotateDirectionMove[d])) * _attackMultiplier);	

							if (life <= 0)
								nextParticleCell.CellParticle.ChangePlayer(currentCellParticle.PlayerIndex);
							else
								nextParticleCell.CellParticle.Life = life;

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