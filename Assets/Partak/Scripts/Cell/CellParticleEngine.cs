using System;
using GeoTetra.GTCommon.Components;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using GT.Threading;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Partak
{
    public class CellParticleEngine : SubscribableBehaviour
    {
        [SerializeField] private ServiceReference _componentContainer;
        [SerializeField] private int _attackMultiplier = 3;
        [SerializeField] private CellParticleStore _cellParticleStore;
        [SerializeField] private CellParticleSpawn _cellParticleSpawn;
        private readonly int[] RotateDirectionMove = {0, -1, 1, -2, 2, -3, 3, -4, 4};
        private LoopThread _loopThread;
        private int[] _randomRotate;
        private int _randomRotateIndex;
        public bool Pause { get; set; }
        public bool FastKill { get; set; }

        private void Awake()
        {
            _componentContainer.Service<ComponentContainer>().RegisterComponent(this);
            
            _randomRotate = new int[128];
            for (int i = 0; i < _randomRotate.Length; ++i)
                if (i % 4 == 0)
                    _randomRotate[i] = Random.Range(-2, 2);
                else
                    _randomRotate[i] = Random.Range(-1, 1);

            _cellParticleSpawn.SpawnComplete += () =>
            {
                _loopThread = LoopThread.Create(MoveParticles, "CellParticleEngine", Priority.High,
                    FindObjectOfType<LevelConfig>().MoveCycleTime);
                _loopThread.Start();
            };
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_loopThread != null)
                _loopThread.Stop();
        }

        private void MoveParticles()
        {
            CellParticle[] cellParticleArray = _cellParticleStore.CellParticleArray;
            CellParticle currentCellParticle;
            ParticleCell currentParticleCell, nextParticleCell;
            int particleLimit = cellParticleArray.Length;
            int directionLimit = RotateDirectionMove.Length;
            int winningPlayer = _cellParticleStore.WinningPlayer();
            int checkDirection, d, p, life;
            for (p = 0; p < particleLimit; ++p)
            for (d = 0; d < directionLimit; ++d)
            {
                currentCellParticle = cellParticleArray[p];
                currentParticleCell = currentCellParticle.ParticleCell;
                checkDirection = CellUtility.RotateDirection(
                    currentCellParticle.ParticleCell.PrimaryDirectionArray[currentCellParticle.PlayerIndex],
                    RotateDirectionMove[d] + _randomRotate[_randomRotateIndex]);
                ++_randomRotateIndex;
                if (_randomRotateIndex == _randomRotate.Length)
                    _randomRotateIndex = 0;
                nextParticleCell = currentParticleCell.DirectionalParticleCellArray[checkDirection];

                if (nextParticleCell == null || nextParticleCell.InhabitedBy == 255)
                    //edge or wall
                    break;

                if (nextParticleCell.InhabitedBy == -1)
                {
                    //is empty move
                    currentCellParticle.ParticleCell = nextParticleCell;
                    break;
                }

                if (currentParticleCell.InhabitedBy != nextParticleCell.InhabitedBy)
                {
                    //if other player, take life
                    if (FastKill && currentParticleCell.InhabitedBy == winningPlayer)
                        life = nextParticleCell._cellParticle.Life - _attackMultiplier * 2;
                    else
                        life = nextParticleCell._cellParticle.Life - _attackMultiplier;
                    if (life <= 0)
                        nextParticleCell._cellParticle.ChangePlayer(currentCellParticle.PlayerIndex);
                    else
                        nextParticleCell._cellParticle.Life = life;
                    if (d > 2)
                        break;
                }
                else if (currentParticleCell.InhabitedBy == nextParticleCell.InhabitedBy)
                {
                    //if other cell is same player, give it additional life boost
                    nextParticleCell._cellParticle.Life++;
                    if (d > 2)
                        break;
                }
            }
        }
    }
}