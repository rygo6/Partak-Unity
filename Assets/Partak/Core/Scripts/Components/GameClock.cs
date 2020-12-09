using System.Collections;
using System.Threading.Tasks;
using GeoTetra.GTCommon.Components;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using UnityEngine;

namespace GeoTetra.Partak
{
    public class GameClock : SubscribableBehaviour
    {
        [SerializeField] private AnalyticsRelayReference _analyticsRelay;
        [SerializeField] private PartakStateRef _partakState;
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

        protected override async Task StartAsync()
        {
            await _partakState.Cache(this);
            
            _initialColors = new Color[_partakState.Ref.PlayerCount()];
            for (int i = 0; i < _initialColors.Length; ++i)
            {
                _initialColors[i] = _partakState.Ref.PlayerStates[i].PlayerColor;
            }
            
            SetTimeLimit(_partakState.Ref.TimeLimitMinutes);
            _cellParticleStore.WinEvent += Win;
            Invoke(nameof(FastKillTimeOut), _fastKillTimeLimit);
            Invoke(nameof(TimeOut), _timeLimit);

            await base.StartAsync();
        }

        private void Update()
        {
            GameTime += Time.deltaTime;
        }

        protected override void OnDestroy()
        {
            if (_cellParticleStore != null) _cellParticleStore.WinEvent -= Win;
            _surroundMaterial.SetFloat(BlendProperty, 0f);
            _surroundMaterial.SetTexture(Texture2Property, null);
            for (int i = 0; i < _partakState.Ref.PlayerCount(); ++i)
            {
                _partakState.Ref.PlayerStates[i].PlayerColor = _initialColors[i];
            }
            base.OnDestroy();
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
                _partakState.Ref.PlayerStates[winningPlayer].PlayerColor += new Color(.3f, .3f, .3f, .3f);
                yield return null;
                yield return null;
                _partakState.Ref.PlayerStates[winningPlayer].PlayerColor = _initialColors[winningPlayer];
                yield return new WaitForSeconds(0.4f);
            }
        }
    }
}