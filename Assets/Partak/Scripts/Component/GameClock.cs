using System.Collections;
using GeoTetra.GTCommon.ScriptableObjects;
using UnityEngine;

namespace Partak
{
    public class GameClock : MonoBehaviour
    {
        [SerializeField] private GameState _gameState;
        [SerializeField] private Material _surroundMaterial;
        [SerializeField] private CellParticleEngine _cellParticleMover;
        [SerializeField] private CellParticleStore _cellParticleStore;

        private float _fastKillTimeLimit = 70f;
        private Color[] _initialColors;
        private bool _limitReached;
        private float _timeLimit = 80f;
        public float GameTime { get; private set; }

        private void Start()
        {
            _initialColors = new Color[_gameState.PlayerCount()];
            for (int i = 0; i < _initialColors.Length; ++i)
            {
                _initialColors[i] = _gameState.PlayerStates[i].PlayerColor;
            }
            
            SetTimeLimit(_gameState.TimeLimitMinutes);
            _cellParticleStore.WinEvent += Win;
            Invoke("FastKillTimeOut", _fastKillTimeLimit);
            Invoke("TimeOut", _timeLimit);
        }

        private void Update()
        {
            GameTime += Time.deltaTime;
        }

        private void OnDestroy()
        {
            _surroundMaterial.SetFloat("_Blend", 0f);
            _surroundMaterial.SetTexture("_Texture2", null);
            for (int i = 0; i < _gameState.PlayerCount(); ++i)
                _gameState.PlayerStates[i].PlayerColor = _initialColors[i];
        }

        public void SetTimeLimit(int minutes)
        {
            if (minutes == 0)
                minutes = 1;
            _fastKillTimeLimit = minutes * 60;
            ;
            _timeLimit = minutes * 20 + _fastKillTimeLimit;
        }

        private void Win()
        {
            CancelInvoke();
//            _componentContainer.Get<AnalyticsRelay>().GameTime(GameTime); //TODO hook up
        }

        private void TimeOut()
        {
            _cellParticleStore.Win();
        }

        private void FastKillTimeOut()
        {
            _cellParticleMover.FastKill = true;
            StartCoroutine(FastKillTimeoutCoroutine());
        }

        private IEnumerator FastKillTimeoutCoroutine()
        {
            int winningPlayer = 0;
            while (true)
            {
                winningPlayer = _cellParticleStore.WinningPlayer();
                _gameState.PlayerStates[winningPlayer].PlayerColor += new Color(.3f, .3f, .3f, .3f);
                yield return null;
                yield return null;
                _gameState.PlayerStates[winningPlayer].PlayerColor = _initialColors[winningPlayer];
                yield return new WaitForSeconds(0.4f);
            }
        }
    }
}