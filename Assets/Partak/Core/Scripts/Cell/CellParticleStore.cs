﻿using System;
using System.Collections;
using GeoTetra.GTCommon.Components;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using UnityEngine;

namespace GeoTetra.Partak
{
    /// <summary>
    /// Keeps track of how many particles belong to each player.
    /// </summary>
    public class CellParticleStore : SubscribableBehaviour
    {
        [SerializeField] private ServiceReference _componentContainer;
        [SerializeField] private ServiceReference _gameStateReference;
        [SerializeField] private CursorStore _cursorStore;
        [SerializeField] private LevelConfig _levelConfig;
        public event Action WinEvent;
        public event Action<int> LoseEvent;
        public bool[] PlayerLost;
        public int[] PlayerParticleCount;
        private Coroutine _calcualtePercentagesCoroutine;
        private int _startParticleCount;
        private bool _winEventFired;
        private GameState _gameState;
        
        public CellParticle[] CellParticleArray { get; private set; }

        private void Awake()
        {
            _gameState = _gameStateReference.Service<GameState>();
            _componentContainer.Service<ComponentContainer>().RegisterComponent(this);
        }

        public void Initialize()
        {
            PlayerLost = new bool[_gameState.PlayerCount()];
            PlayerParticleCount = new int[_gameState.PlayerCount()];
            CellParticleArray = new CellParticle[_levelConfig.Datum.ParticleCount];
            _startParticleCount = _levelConfig.Datum.ParticleCount / _gameState.ActivePlayerCount();
            
            if (_calcualtePercentagesCoroutine != null) {StopCoroutine(_calcualtePercentagesCoroutine);}
            _calcualtePercentagesCoroutine = StartCoroutine(CalculatePercentages());
        }

        public void IncrementPlayerParticleCount(int playerIndex)
        {
            PlayerParticleCount[playerIndex]++;
        }

        public void DecrementPlayerParticleCount(int playerIndex)
        {
            PlayerParticleCount[playerIndex]--;
        }

        private IEnumerator CalculatePercentages()
        {
            yield return new WaitForSeconds(2.0f);
            while (true)
            {
                if (Input.GetKey(KeyCode.W))
                {
                    Win();
                    yield break;
                }

                for (int playerIndex = 0; playerIndex < PlayerParticleCount.Length; ++playerIndex)
                {
                    float percentage = ParticleCountPercentage(playerIndex);
                    percentage = Mathf.Clamp(percentage, 0f, 1f) * 100f;
                    _cursorStore.SetPlayerCursorMorph(playerIndex, percentage);
                    if (percentage >= 100f)
                    {
                        Win();
                        yield break;
                    }

                    if (PlayerParticleCount[playerIndex] <= 0f && !PlayerLost[playerIndex])
                    {
                        PlayerLost[playerIndex] = true;
                        _cursorStore.PlayerLose(playerIndex);
                        LoseEvent?.Invoke(playerIndex);
                    }
                }

                yield return new WaitForSeconds(.2f);
            }
        }

        private float ParticleCountPercentage(int playerIndex)
        {
            return (PlayerParticleCount[playerIndex] - _startParticleCount) / (float) (_levelConfig.Datum.ParticleCount - _startParticleCount);
        }

        public bool AllPercentagesChanged()
        {
            bool changed = true;
            for (int playerIndex = 0; playerIndex < PlayerParticleCount.Length; ++playerIndex)
            {
                if (Mathf.Approximately(ParticleCountPercentage(playerIndex), 0)) changed = false;
            }

            return changed;
        }
        
        //should be its own object WinSequence
        public void Win()
        {
            if (!_winEventFired)
            {
                _winEventFired = true;
                _cursorStore.PlayerWin(WinningPlayer());
                WinEvent();
            }
        }

        public int WinningPlayer()
        {
            int count = 0;
            int playerIndex = 0;
            for (int i = 0; i < PlayerParticleCount.Length; ++i)
                if (PlayerParticleCount[i] > count)
                {
                    count = PlayerParticleCount[i];
                    playerIndex = i;
                }

            return playerIndex;
        }

        public int LosingPlayer()
        {
            int count = int.MaxValue;
            int playerIndex = 0;
            for (int i = 0; i < PlayerParticleCount.Length; ++i)
                if (PlayerParticleCount[i] < count && !PlayerLost[i])
                {
                    count = PlayerParticleCount[i];
                    playerIndex = i;
                }

            return playerIndex;
        }
    }
}