using System;
using System.Collections;
using GeoTetra.GTCommon.ScriptableObjects;
using UnityEngine;

namespace Partak
{
    public class CellParticleStore : MonoBehaviour
    {
        [SerializeField] private GameState _gameState;
        [SerializeField] private ComponentContainer _componentContainer;
        public bool[] PlayerLost;
        public int[] PlayerParticleCount;
        private CursorStore _cursorStore;
        private LevelConfig _levelConfig;
        private int _startParticleCount;
        private bool _winEventFired;
        public CellParticle[] CellParticleArray { get; private set; }
        public event Action WinEvent;
        public event Action<int> LoseEvent;

        private void Awake()
        {
            _componentContainer.RegisterComponent(this);
        }

        private void Start()
        {
            _levelConfig = _componentContainer.Get<LevelConfig>();
            _cursorStore = _componentContainer.Get<CursorStore>();

            PlayerLost = new bool[_gameState.PlayerCount()];
            PlayerParticleCount = new int[_gameState.PlayerCount()];
            CellParticleArray = new CellParticle[_levelConfig.ParticleCount];
            _startParticleCount = _levelConfig.ParticleCount / _gameState.ActivePlayerCount();

            _componentContainer.Get<CellParticleSpawn>().SpawnComplete +=
                () => { StartCoroutine(CalculatePercentages()); };
        }

        private void OnDestroy()
        {
            _componentContainer.UnregisterComponent(this);
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
                    float percentage = (PlayerParticleCount[playerIndex] - _startParticleCount) /
                                       (float) (_levelConfig.ParticleCount - _startParticleCount);
                    percentage = Mathf.Clamp(percentage, 0f, 1f) * 100f;
                    _cursorStore.SetPlayerCursorMorph(playerIndex, percentage);
                    if (percentage == 100f)
                    {
                        Win();
                        yield break;
                    }

                    if (PlayerParticleCount[playerIndex] == 0f && !PlayerLost[playerIndex])
                    {
                        PlayerLost[playerIndex] = true;
                        _cursorStore.PlayerLose(playerIndex);
                        LoseEvent(playerIndex);
                    }
                }

                yield return new WaitForSeconds(.2f);
            }
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