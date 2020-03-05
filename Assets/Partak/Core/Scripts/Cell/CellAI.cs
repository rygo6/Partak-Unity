﻿using GeoTetra.GTPooling;
using GT.Threading;
using UnityEngine;
using Random = System.Random;

namespace GeoTetra.Partak
{
    public class CellAI : MonoBehaviour
    {
        [SerializeField] private ServiceReference _gameStateReference;
        [SerializeField] private CursorStore _cursorStore;
        [SerializeField] private CellParticleStore _cellParticleStore;
        [SerializeField] private CellParticleSpawn _cellParticleSpawn;
        [SerializeField] private GameClock _gameTimer;
        [SerializeField] private LevelConfig _levelConfig;
        [SerializeField] private int _randomCycleRate = 20;

        private readonly Random _random = new Random();
        private int[] _aiCellParticleIndex;
        private Vector3[] _aiCursorTarget;
        private Vector3[] _aiCursorVelocity;
        private int[] _randomPullCycle;

        private LoopThread _loopThread;
        private GameState _gameState;
        
        public bool Initialized { get; private set; }
        
        private void Awake()
        {
            _gameState = _gameStateReference.Service<GameState>();
        }

        /// <summary>
        /// Start after particles spawn
        /// </summary>
        public void Initialize()
        {
            _aiCellParticleIndex = new int[_gameState.PlayerCount()];
            _aiCursorTarget = new Vector3[_gameState.PlayerCount()];
            _aiCursorVelocity = new Vector3[_gameState.PlayerCount()];
            _randomPullCycle = new int[_gameState.PlayerCount()];

            for (int i = 0; i < _gameState.PlayerCount(); ++i)
            {
                _aiCellParticleIndex[i] = i * 10;
                _randomPullCycle[i] = i * (_randomCycleRate / _gameState.PlayerCount());
            }
            
            _loopThread = LoopThread.Create(UpdateAICursor, "CellAI", Priority.Low);
            _loopThread.Start();

            Initialized = true;
        }

        private void Update()
        {
            if (Initialized) MoveAICursorUpdate();
        }

        private void OnDestroy()
        {
            if (_loopThread != null)
                _loopThread.Stop();
        }

        public void MoveAICursorUpdate()
        {
            for (int playerIndex = 0; playerIndex < _gameState.PlayerCount(); ++playerIndex)
                if (_gameState.PlayerStates[playerIndex].PlayerMode == PlayerMode.Comp &&
                    !_cellParticleStore.PlayerLost[playerIndex])
                    _cursorStore.SetCursorPositionClamp(playerIndex,
                        Vector3.SmoothDamp(
                            _cursorStore.CursorPositions[playerIndex],
                            _aiCursorTarget[playerIndex],
                            ref _aiCursorVelocity[playerIndex],
                            .4f));
        }

        private void UpdateAICursor()
        {
            int winningPlayerIndex = _cellParticleStore.WinningPlayer();
            int losingPlayerIndex = _cellParticleStore.LosingPlayer();
            int targetPlayerIndex, playerIndex, newIndex;
            int particleLimit = _cellParticleStore.CellParticleArray.Length;
            int playerLimit = _gameState.PlayerCount();
            for (playerIndex = 0; playerIndex < playerLimit; ++playerIndex)
                if (_gameState.PlayerStates[playerIndex].PlayerMode == PlayerMode.Comp &&
                    !_cellParticleStore.PlayerLost[playerIndex])
                {
                    targetPlayerIndex = 0;
                    if (_gameTimer.GameTime < 8f)
                        targetPlayerIndex = _random.Next(0, playerLimit);
                    else if (playerIndex != winningPlayerIndex)
                        targetPlayerIndex = winningPlayerIndex;
                    else
                        targetPlayerIndex = losingPlayerIndex;

                    _randomPullCycle[playerIndex]++;
                    if (_randomPullCycle[playerIndex] == _randomCycleRate)
                    {
                        _randomPullCycle[playerIndex] = 0;
                        _aiCursorTarget[playerIndex].x = _random.Next(-10, (int) _levelConfig.LevelBounds.max.x + 10);
                        _aiCursorTarget[playerIndex].z = _random.Next(-10, (int) _levelConfig.LevelBounds.max.z + 10);
                    }
                    else if (_cellParticleStore.PlayerParticleCount[targetPlayerIndex] > 20)
                    {
                        newIndex = _aiCellParticleIndex[playerIndex] + 1;
                        if (newIndex >= particleLimit)
                            newIndex = 0;
                        while (_cellParticleStore.CellParticleArray[newIndex].PlayerIndex != targetPlayerIndex &&
                               !_cellParticleStore.PlayerLost[targetPlayerIndex])
                        {
                            ++newIndex;
                            if (newIndex >= particleLimit)
                                newIndex = 0;
                        }

                        _aiCellParticleIndex[playerIndex] = newIndex;
                        _aiCursorTarget[playerIndex] =
                            _cellParticleStore.CellParticleArray[newIndex].ParticleCell.WorldPosition;
                    }
                    else
                    {
                        _aiCursorTarget[playerIndex] = _cellParticleStore
                            .CellParticleArray[_aiCellParticleIndex[playerIndex]].ParticleCell.WorldPosition;
                    }

                    _loopThread.Wait(500);
                }
        }
    }
}