using UnityEngine;
using System.Collections;
using System.Threading;
using System.Diagnostics;

namespace Partak
{
	public class CellParticleMover : MonoBehaviour
	{
		[SerializeField]
		private CellParticleStore _cellParticleStore;

		public bool Timeout { get; set; }

		private readonly int[] RotateDirectionMove = new int[9]{ 0, -1, 1, -2, 2, -3, 3, -4, 4 };

		public bool Pause { get; set; }

		private bool _runThread;

		private Thread _thread;

		[SerializeField]
		public int _attackMultiplier = 3;

		private int _cycleTime;

		private void Awake()
		{
			_cycleTime = FindObjectOfType<LevelConfig>().MoveCycleTime;
			FindObjectOfType<CellParticleSpawn>().SpawnComplete += StartThread;
			_cellParticleStore.WinEvent += StopThread;
		}

		private void OnDestroy()
		{
			StopThread();
		}

		public void StartThread()
		{
			_runThread = true;
			_thread = new Thread(RunThread);
			_thread.IsBackground = true;
			_thread.Priority = System.Threading.ThreadPriority.Highest;
			_thread.Name = "CellParticleMove";
			_thread.Start();
//			StartCoroutine(RunCoroutine());
		}

		private void StopThread()
		{
			if (_thread != null)
			{
#if UNITY_EDITOR
				_thread.Abort();	
#endif				
				_runThread = false;
				while (_thread.IsAlive)
				{
				}
			}
		}

		private IEnumerator RunCoroutine()
		{
			while (_runThread)
			{
				MoveParticles();
				yield return null;
			}
		}

		private void RunThread()
		{
			Stopwatch stopWatch = new Stopwatch();
			stopWatch.Start();
			int startTime, deltaTime;
			while (_runThread)
			{
				startTime = (int)stopWatch.ElapsedMilliseconds;
				MoveParticles();
				deltaTime = (int)stopWatch.ElapsedMilliseconds - startTime;
				if (deltaTime < _cycleTime)
					Thread.Sleep(_cycleTime - deltaTime);
				stopWatch.Reset();
			}
		}

		private void MoveParticles()
		{
			CellParticle[] cellParticleArray = _cellParticleStore.CellParticleArray;
			CellParticle currentCellParticle;
			ParticleCell currentParticleCell, nextParticleCell;
			int limit = cellParticleArray.Length;
			int directionLimit = RotateDirectionMove.Length;
			int winningPlayer = _cellParticleStore.WinningPlayer();
			int checkDirection, d, p, life;

			for (p = 0; p < limit; ++p)
			{
				for (d = 0; d < directionLimit; ++d)
				{
					currentCellParticle = cellParticleArray[p];
					currentParticleCell = currentCellParticle.ParticleCell;

					//inlined for performance
//					checkDirection = CellUtility.RotateDirection(
//						currentCellParticle.ParticleCell.PrimaryDirectionArray[currentCellParticle.PlayerIndex], 
//						RotateDirectionMove[d]);

					checkDirection = currentCellParticle.ParticleCell.PrimaryDirectionArray[currentCellParticle.PlayerIndex] + RotateDirectionMove[d];
					if (checkDirection > 11)
						checkDirection = checkDirection - 12;
					else if (checkDirection < 0)
						checkDirection = 12 + checkDirection;	

					nextParticleCell = currentParticleCell.DirectionalParticleCellArray[checkDirection];
						
					if (nextParticleCell != null)
					{
						//move
						if (nextParticleCell.InhabitedBy == -1)
						{
							currentCellParticle.ParticleCell = nextParticleCell;
							break;
						}
						//if other player, take life
						else if (currentParticleCell.InhabitedBy != nextParticleCell.InhabitedBy)
						{	
							if (Timeout && currentParticleCell.InhabitedBy == winningPlayer)
							{
								life = nextParticleCell.CellParticle.Life - ((5 - Mathf.Abs(RotateDirectionMove[d])) * _attackMultiplier * 2);	
							}
							else
							{
								life = nextParticleCell.CellParticle.Life - ((5 - Mathf.Abs(RotateDirectionMove[d])) * _attackMultiplier);	
							}

							if (life <= 0)
								nextParticleCell.CellParticle.ChangePlayer(currentCellParticle.PlayerIndex);
							else
								nextParticleCell.CellParticle.Life = life;

							if (d > 2)
								break;
						}
						//if other cell is same player, give it additional life boost
						else if (currentParticleCell.InhabitedBy == nextParticleCell.InhabitedBy)
						{	
							nextParticleCell.CellParticle.Life++;

							if (d > 2)
								break;
						}
					}
				}
			}
		}
	}
}