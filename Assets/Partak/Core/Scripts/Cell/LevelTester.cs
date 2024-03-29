﻿using System.Collections;
using System.Threading.Tasks;
using GeoTetra.GTCommon.Components;
using GeoTetra.GTPooling;
using GeoTetra.GTSnapper;
using UnityEngine;

namespace GeoTetra.Partak
{
    /// <summary>
    /// Controls initializing of all game systems.
    /// </summary>
    public class LevelTester : SubscribableBehaviour
    {
        [SerializeField]
        private ComponentContainerRef _componentContainer;
        
        [SerializeField]
        private PartakStateRef _partakSate;
        
        [SerializeField] private LevelConfig _levelConfig;
        [SerializeField] private CellHiearchy _cellHiearchy;
        [SerializeField] private CellGradient _cellGradient;
        [SerializeField] private CellParticleDisplay _cellParticleDisplay;
        [SerializeField] private CellParticleEngine _cellParticleEngine;
        [SerializeField] private CellParticleSpawn _cellParticleSpawn;
        [SerializeField] private CellParticleStore _cellParticleStore;
        [SerializeField] private CursorStore _cursorStore;
        [SerializeField] private ItemDrop _itemDrop;
        [SerializeField] private InputCatcher _inputCatcher;
        [SerializeField] private ItemRoot _itemRoot;
        
        private const float MaxTestWaitTime = 10;
        private const int TestPlayerCount = 4;
        private PartakState.PlayerState[] _playerStates;

        public enum TestResult
        {
            CursorsBlocked,
            SpawnBlocked,
            ParticlesBlocked,
            Success,
            TooFewObjects,
            TooManyObjects
        }
        
        public TestResult Result { get; private set; }

        private void Awake()
        {
            _levelConfig.LevelDeserialized += Initialize;
            _levelConfig.SizeChanged += Initialize;
        }

        protected override async Task StartAsync()
        {
            await Task.WhenAll(
                _partakSate.Cache(this), 
                _componentContainer.CacheAndRegister(this)
            );
            await base.StartAsync();
        }

        private async void Initialize()
        {
            await Starting;

            _playerStates = new PartakState.PlayerState[TestPlayerCount];
            for (int i = 0; i < _playerStates.Length; ++i)
            {
                _playerStates[i] = new PartakState.PlayerState
                {
                    PlayerColor = _partakSate.Ref.PlayerStates[i].PlayerColor
                };
            }
            
            await _cellHiearchy.Initialize();
            await _cellGradient.Initialize(false, _playerStates);
            await _cellParticleDisplay.Initialize();
            _cellParticleDisplay.gameObject.SetActive(false);
            await _cellParticleEngine.Initialize(false);
        }

        public IEnumerator RunTest(int testCursorIndex)
        {
            Debug.Log("ItemCount " + _itemRoot.ItemCount);
            if (_itemRoot.ItemCount < 4)
            {
                Result = TestResult.TooFewObjects;
                yield break;
            }
            
            if (_itemRoot.ItemCount > 40)
            {
                Result = TestResult.TooManyObjects;
                yield break;
            }
            
            //disable floor collider catcher because right now "no-wall" is discerned by no racyast hit
            _itemDrop.gameObject.SetActive(false);
            _inputCatcher.gameObject.SetActive(false);
            _cellParticleDisplay.gameObject.SetActive(true);
            _cursorStore.SetCursorsToStartPosition();
            _itemRoot.UnHighlightAll();
            
            //reconstruct hiearchy
            Task hiearchyTask = _cellHiearchy.Initialize();
            yield return new WaitUntil(() => hiearchyTask.IsCompleted);
            if (AnyCursorPositionBlocked())
            {
                _itemDrop.gameObject.SetActive(true);
                _inputCatcher.gameObject.SetActive(true);
                Result = TestResult.CursorsBlocked;
                yield break;
            }

            _cellGradient.StartThread();
            Task storeTask = _cellParticleStore.Initialize(_playerStates);
            yield return new WaitUntil(() => storeTask.IsCompleted);
            Task spawnTask = _cellParticleSpawn.Initialize(_levelConfig.Datum.ParticleCount, _playerStates);
            yield return new WaitUntil(() => spawnTask.IsCompleted);
            if (!_cellParticleSpawn.SpawnSuccessful)
            {
                ClearTest();
                Result = TestResult.SpawnBlocked;
                yield break;
            }

            Vector3[] cursorPositions = (Vector3[]) _cursorStore.CursorPositions.Clone();
            _cellParticleEngine.StartThread(0);
            
            for (int cursorFocus = 0; cursorFocus < 4; ++cursorFocus)
            {
                if (cursorFocus == testCursorIndex) continue;
                
                _cursorStore.CursorPositions[testCursorIndex] = cursorPositions[cursorFocus];
                
                float counter = 0;
                // float moveCounter = 0;
                while (!_cellParticleStore.PercentageChanged(cursorFocus) )
                {
                    if (counter > MaxTestWaitTime)
                    {
                        ClearTest();
                        Result = TestResult.ParticlesBlocked;
                        yield break;
                    }
                    
                    counter += Time.deltaTime;
                    yield return null;
                }
            }

            ClearTest();
            Result = TestResult.Success;
        }

        private bool AnyCursorPositionBlocked()
        {
            for (int i = 0; i < _cursorStore.CursorPositions.Length; ++i)
            {
                int particleIndex = CellUtility.WorldPositionToGridIndex(_cursorStore.CursorPositions[i].x,_cursorStore.CursorPositions[i].z, _cellHiearchy.ParticleCellGrid.Dimension);
                ParticleCell startParticleCell = _cellHiearchy.ParticleCellGrid.Grid[particleIndex];
                if (startParticleCell.InhabitedBy == 255)
                {
                    return true;
                }
            }

            return false;
        }

        private async void ClearTest()
        {
             _cellParticleEngine.Thread?.Stop();
            _cellGradient.Thread?.Stop();
            await _cellHiearchy.Initialize();
            _itemDrop.gameObject.SetActive(true);
            _inputCatcher.gameObject.SetActive(true);
            _cursorStore.SetCursorsToStartPosition();
            _cellParticleDisplay.gameObject.SetActive(false);
        }
    }
}