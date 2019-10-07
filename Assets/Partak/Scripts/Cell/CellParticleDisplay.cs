using GeoTetra.GTCommon.ScriptableObjects;
using UnityEngine;

namespace Partak
{
    public class CellParticleDisplay : MonoBehaviour
    {
        [SerializeField] private GameState _gameState;
        [SerializeField] private CellHiearchy _cellHiearchy;
        [SerializeField] private CellParticleSystem _cellParticleSystemPrefab;
        
        private CellParticleSystem[] _cellParticleSystems;
        private int[] _levelCount;

        private void Start()
        {
            int systemCount = _cellHiearchy.CellGroupGrids.Length;
            LevelConfig levelConfig = FindObjectOfType<LevelConfig>();

            //you add additional positions onto the particle count as a buffer in case when it is reading in
            //particle positions one of the particles is updated from another thread and ends up registering twice
            int particleCount = levelConfig.ParticleCount + levelConfig.ParticleCount / 4;

            _cellParticleSystems = new CellParticleSystem[systemCount];
            for (int i = 0; i < systemCount; ++i)
            {
                _cellParticleSystems[i] = Instantiate(_cellParticleSystemPrefab, transform).GetComponent<CellParticleSystem>();
//			_cellParticleSystems[i].transform.position = levelConfig.LevelBounds.center;
//			_cellParticleSystems[i].transform.parent = Camera.main.transform;
//			_cellParticleSystems[i].transform.localPosition = new Vector3(0f, 0f, 4f);
//			_cellParticleSystems[i].transform.localRotation = Quaternion.identity;
//			_cellParticleSystems[i].transform.localScale = Vector3.one;
                float particleSize = Mathf.Pow(2, i);
                particleSize /= 10f;
                _cellParticleSystems[i].Initialize(particleCount, particleSize);
                particleCount /= 2;
            }

            _levelCount = new int[_cellHiearchy.ParentCellGridLevel];
            for (int i = 0; i < _levelCount.Length; ++i) _levelCount[i] = (int) Mathf.Pow(4, i) - (i / 2 + 1);
        }

        private void Update()
        {
            DrawParticleSystems();
        }

        private void DrawCellGroup(CellGroup cellGroup)
        {
            int parentLevel = cellGroup.CellGroupGrid.ParentLevel;
            int levelCount = _levelCount[parentLevel];
            int playerIndex;
            int playerLimit = _gameState.PlayerCount();
            for (playerIndex = 0; playerIndex < playerLimit; ++playerIndex)
            {
                if (cellGroup.PlayerParticleCount[playerIndex] > levelCount)
                {
                    _cellParticleSystems[parentLevel].SetNextParticle(
                        cellGroup.WorldPosition,
                        _gameState.PlayerStates[playerIndex].PlayerColor);
                    return;
                }
            }

            if (cellGroup.ChildCellGroupArray != null)
            {
                int cellIndex;
                int cellLimit = cellGroup.ChildCellGroupArray.Length;
                for (cellIndex = 0; cellIndex < cellLimit; ++cellIndex)
                    DrawCellGroup(cellGroup.ChildCellGroupArray[cellIndex]);
            }
        }

        private void ResetParticleSystemsCount()
        {
            for (int i = 0; i < _cellParticleSystems.Length; ++i) _cellParticleSystems[i].ResetCount();
        }

        private void UpdateParticleSystems()
        {
            for (int i = 0; i < _cellParticleSystems.Length; ++i) _cellParticleSystems[i].UpdateParticleSystem();
        }

        private void DrawParticleSystems()
        {
            ResetParticleSystemsCount();

            CellGroupGrid cellGroupGrid;
            int cellIndex;
            int cellLimit;
            for (int levelIndex = 0; levelIndex < _cellHiearchy.CellGroupGrids.Length; ++levelIndex)
            {
                cellGroupGrid = _cellHiearchy.CellGroupGrids[levelIndex];
                cellLimit = cellGroupGrid.FlatGrid.Length;
                for (cellIndex = 0; cellIndex < cellLimit; ++cellIndex)
                    DrawCellGroup(cellGroupGrid.FlatGrid[cellIndex]);
            }

            UpdateParticleSystems();
        }
    }
}