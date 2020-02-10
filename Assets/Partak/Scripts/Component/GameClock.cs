using System.Collections;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using UnityEngine;

namespace GeoTetra.Partak
{
    public class GameClock : MonoBehaviour
    {
        [SerializeField] private ServiceReference _analyticsRelay;
        [SerializeField] private ServiceReference _gameStateReference;
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
            _initialColors = new Color[_gameStateReference.Service<GameState>().PlayerCount()];
            for (int i = 0; i < _initialColors.Length; ++i)
            {
                _initialColors[i] = _gameStateReference.Service<GameState>().PlayerStates[i].PlayerColor;
            }
            
            SetTimeLimit(_gameStateReference.Service<GameState>().TimeLimitMinutes);
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
            if (_cellParticleStore != null) _cellParticleStore.WinEvent -= Win;
            _surroundMaterial.SetFloat("_Blend", 0f);
            _surroundMaterial.SetTexture("_Texture2", null);
            for (int i = 0; i < _gameStateReference.Service<GameState>().PlayerCount(); ++i)
                _gameStateReference.Service<GameState>().PlayerStates[i].PlayerColor = _initialColors[i];
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
             _analyticsRelay.Service<AnalyticsRelay>().GameTime(GameTime);
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
                _gameStateReference.Service<GameState>().PlayerStates[winningPlayer].PlayerColor += new Color(.3f, .3f, .3f, .3f);
                yield return null;
                yield return null;
                _gameStateReference.Service<GameState>().PlayerStates[winningPlayer].PlayerColor = _initialColors[winningPlayer];
                yield return new WaitForSeconds(0.4f);
            }
        }
    }
}