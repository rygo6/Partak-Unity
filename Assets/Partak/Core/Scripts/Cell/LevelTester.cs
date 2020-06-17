using System.Collections;
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
        [SerializeField] private ComponentContainerReference _componentContainer;
        [SerializeField] private GameStateReference _gameSate;
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

        private const float MaxTestWaitTime = 5;
        private const int TestPlayerCount = 4;
        private GameState.PlayerState[] _playerStates;

        public enum TestResult
        {
            CursorsBlocked,
            SpawnBlocked,
            ParticlesBlocked,
            Success
        }
        
        public TestResult Result { get; private set; }
        
        private void Awake()
        {
            _playerStates = new GameState.PlayerState[TestPlayerCount];
            for (int i = 0; i < _playerStates.Length; ++i)
            {
                _playerStates[i] = new GameState.PlayerState
                {
                    PlayerColor = _gameSate.Service.PlayerStates[i].PlayerColor
                };
            }
            
            _componentContainer.Service.RegisterComponent(this);
            _levelConfig.LevelDeserialized += Initialize;
            _levelConfig.SizeChanged += Initialize;
        }

        private void Initialize()
        {
            _cellHiearchy.Initialize();
            _cellGradient.Initialize(false, _playerStates);
            _cellParticleDisplay.Initialize();
            _cellParticleDisplay.gameObject.SetActive(false);
            _cellParticleEngine.Initialize(false);
        }

        public IEnumerator RunTest()
        {
            //disable floor collider catcher because right now "no-wall" is discerned by no racyast hit
            _itemDrop.gameObject.SetActive(false);
            _inputCatcher.gameObject.SetActive(false);
            _cellParticleDisplay.gameObject.SetActive(true);
            _cursorStore.SetCursorsToStartPosition();
            
            //reconstruct hiearchy
            _cellHiearchy.Initialize();
            if (AnyCursorPositionBlocked())
            {
                _itemDrop.gameObject.SetActive(true);
                _inputCatcher.gameObject.SetActive(true);
                Result = TestResult.CursorsBlocked;
                yield break;
            }

            _cellGradient.StartThread();
            _cellParticleStore.Initialize();
            yield return StartCoroutine(_cellParticleSpawn.Initialize(_levelConfig.Datum.ParticleCount, _playerStates));
            if (!_cellParticleSpawn.SpawnSuccessful)
            {
                ClearTest();
                Result = TestResult.SpawnBlocked;
                yield break;
            }
            
            _cursorStore.SetCursorsTo(_cursorStore.CursorPositions[0]);
            _cellParticleEngine.StartThread(0);

            float counter = 0;
            while (!_cellParticleStore.AllPercentagesChanged() )
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

        private void ClearTest()
        {
             _cellParticleEngine.Thread?.Stop();
            _cellGradient.Thread?.Stop();
            _cellHiearchy.Initialize();
            _itemDrop.gameObject.SetActive(true);
            _inputCatcher.gameObject.SetActive(true);
            _cursorStore.SetCursorsToStartPosition();
            _cellParticleDisplay.gameObject.SetActive(false);
        }
    }
}