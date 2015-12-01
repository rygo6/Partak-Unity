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

		private readonly int[] RotateDirectionMove = new int[]{ 0, -1, 1, -2, 2, -3, 3};

		public bool Pause { get; set; }

		private bool _runThread;

		private Thread _thread;

		[SerializeField]
		public int _attackMultiplier = 3;

		private int _cycleTime;

		private Color[] _playerColors;

		private void Awake()
		{
			_playerColors = Persistent.Get<PlayerSettings>().PlayerColors;
			_cycleTime = FindObjectOfType<LevelConfig>().MoveCycleTime;
			FindObjectOfType<CellParticleSpawn>().SpawnComplete += StartThread;
			_cellParticleStore.WinEvent += () =>
			{
				_runThread = false;
			};
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
				_runThread = false;
				_thread.Join();
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
			Thread.BeginThreadAffinity();
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
			Thread.EndThreadAffinity();
		}
			
		private void MoveParticles()
		{
			CellParticle[] cellParticleArray = _cellParticleStore.CellParticleArray;
			CellParticle currentCellParticle;
			ParticleCell currentParticleCell, nextParticleCell;
			ParticleCell particleCell;
			int particleLimit = cellParticleArray.Length;
			int directionLimit = RotateDirectionMove.Length;
			int winningPlayer = _cellParticleStore.WinningPlayer();
			int checkDirection, d, p, i, life, limit, rotateDirection;
			CellGroup bottomCellGroup;

			for (p = 0; p < particleLimit; ++p)
			{
				for (d = 0; d < directionLimit; ++d)
				{
					currentCellParticle = cellParticleArray[p];
					currentParticleCell = currentCellParticle.ParticleCell;

					//inlined for performance
//					checkDirection = CellUtility.RotateDirection(
//						currentCellParticle.ParticleCell.PrimaryDirectionArray[currentCellParticle.PlayerIndex], 
//						RotateDirectionMove[d]);

					checkDirection = currentParticleCell.PrimaryDirectionArray[currentCellParticle.PlayerIndex] + RotateDirectionMove[d];
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
							//commented and manually inlined. This is such intricate nasty inlining, generally you should undo it
							//before trying to make future changes
//							currentCellParticle.ParticleCell = nextParticleCell;

							currentCellParticle._particleCell.BottomCellGroup.RemovePlayerParticle(currentCellParticle.PlayerIndex);
							currentCellParticle._particleCell.InhabitedBy = -1;
							currentCellParticle._particleCell._cellParticle = null;

							currentCellParticle._particleCell = nextParticleCell;
							currentCellParticle._particleCell._cellParticle = currentCellParticle;
							currentCellParticle._particleCell.InhabitedBy = currentCellParticle.PlayerIndex;
							currentCellParticle._particleCell.BottomCellGroup.AddPlayerParticle(currentCellParticle.PlayerIndex);

							break;
						}
						//if other player, take life
						else if (currentParticleCell.InhabitedBy != nextParticleCell.InhabitedBy)
						{	
							if (Timeout && currentParticleCell.InhabitedBy == winningPlayer)
								life = nextParticleCell._cellParticle.Life - (_attackMultiplier * 2);	
							else
								life = nextParticleCell._cellParticle.Life - _attackMultiplier;	

							if (life <= 0)
							{
								//commented and manually inlined. This is such intricate nasty inlining, generally you should undo it
								//before trying to make future changes
//								nextParticleCell._cellParticle.ChangePlayer(currentCellParticle.PlayerIndex);

								_cellParticleStore.PlayerParticleCount[nextParticleCell._cellParticle.PlayerIndex]--;

								//inlined below
//								nextParticleCell.BottomCellGroup.RemovePlayerParticle(nextParticleCell._cellParticle.PlayerIndex);
								bottomCellGroup = nextParticleCell.BottomCellGroup;
								bottomCellGroup.PlayerParticleCount[nextParticleCell._cellParticle.PlayerIndex]--;
								bottomCellGroup.PlayerParticleCount[currentCellParticle.PlayerIndex]++;
								limit = bottomCellGroup.ParentCellGroups.Length;
								for (i = 0; i < limit; ++i)
								{
									bottomCellGroup.ParentCellGroups[i].PlayerParticleCount[nextParticleCell._cellParticle.PlayerIndex]--;
									bottomCellGroup.ParentCellGroups[i].PlayerParticleCount[currentCellParticle.PlayerIndex]++;
								}

								nextParticleCell._cellParticle.PlayerIndex = currentCellParticle.PlayerIndex;
								nextParticleCell._cellParticle.Life = 255;
								nextParticleCell._cellParticle.PlayerColor = _playerColors[currentCellParticle.PlayerIndex];
								nextParticleCell.InhabitedBy = currentCellParticle.PlayerIndex;

								//inlined above
//								nextParticleCell.BottomCellGroup.AddPlayerParticle(currentCellParticle.PlayerIndex);

								_cellParticleStore.PlayerParticleCount[currentCellParticle.PlayerIndex]++;
							}
							else
								nextParticleCell._cellParticle.Life = life;

							if (d > 2)
								break;
						}
						//if other cell is same player, give it additional life boost
						else if (currentParticleCell.InhabitedBy == nextParticleCell.InhabitedBy)
						{	
							nextParticleCell._cellParticle.Life++;

							if (d > 2)
								break;
						}
					}
				}
			}
		}
	}
}