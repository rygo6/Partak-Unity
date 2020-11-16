using System.Collections;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using UnityEngine;

namespace GeoTetra.Partak
{
    public class GameClock : MonoBehaviour
    {
        [SerializeField] private AnalyticsRelayReference _analyticsRelay;
        [SerializeField] private GameStateRef _gameState;
        [SerializeField] private Material _surroundMaterial;
        [SerializeField] private CellParticleEngine _cellParticleMover;
        [SerializeField] private CellParticleStore _cellParticleStore;

        private static readonly int BlendProperty = Shader.PropertyToID("_Blend");
        private static readonly int Texture2Property = Shader.PropertyToID("_Texture2");
        private float _fastKillTimeLimit = 70f;
        private Color[] _initialColors;
        private bool _limitReached;
        private float _timeLimit = 80f;
        
        public float GameTime { get; private set; }

        private async void Start()
        {
            await _gameState.Cache();
            
            _initialColors = new Color[_gameState.Service.PlayerCount()];
            for (int i = 0; i < _initialColors.Length; ++i)
            {
                _initialColors[i] = _gameState.Service.PlayerStates[i].PlayerColor;
            }
            
            SetTimeLimit(_gameState.Service.TimeLimitMinutes);
            _cellParticleStore.WinEvent += Win;
            Invoke(nameof(FastKillTimeOut), _fastKillTimeLimit);
            Invoke(nameof(TimeOut), _timeLimit);
        }

        private void Update()
        {
            GameTime += Time.deltaTime;
        }

        private void OnDestroy()
        {
            if (_cellParticleStore != null) _cellParticleStore.WinEvent -= Win;
            _surroundMaterial.SetFloat(BlendProperty, 0f);
            _surroundMaterial.SetTexture(Texture2Property, null);
            for (int i = 0; i < _gameState.Service.PlayerCount(); ++i)
            {
                _gameState.Service.PlayerStates[i].PlayerColor = _initialColors[i];
            }
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
             _analyticsRelay.Service.GameTime(GameTime);
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

        // After a certain amount of time, the winning player becomes stronger to kill faster.
        private IEnumerator FastKillTimeoutCoroutine()
        {
            int winningPlayer = 0;
            while (true)
            {
                winningPlayer = _cellParticleStore.WinningPlayer();
                _gameState.Service.PlayerStates[winningPlayer].PlayerColor += new Color(.3f, .3f, .3f, .3f);
                yield return null;
                yield return null;
                _gameState.Service.PlayerStates[winningPlayer].PlayerColor = _initialColors[winningPlayer];
                yield return new WaitForSeconds(0.4f);
            }
        }
    }
}