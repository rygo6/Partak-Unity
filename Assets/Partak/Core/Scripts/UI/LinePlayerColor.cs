using System.Threading.Tasks;
using GeoTetra.GTCommon.Components;
using UnityEngine;

namespace GeoTetra.Partak
{
    public class LinePlayerColor : SubscribableBehaviour
    {
        [SerializeField] private PartakStateRef _partakStateRef;
        [SerializeField] private int _playerIndex;
        [SerializeField] private bool _constantUpdate;
        [SerializeField] private LineRenderer _lineRenderer;

        protected override async Task StartAsync()
        {
            await _partakStateRef.Cache(this);
            
            _lineRenderer.material.color = _lineRenderer.material.color.SetRGB(_partakStateRef.Ref.PlayerStates[_playerIndex].PlayerColor);
            if (_constantUpdate)
            {
                _partakStateRef.Ref.PlayerStates[_playerIndex].ColorChanged += UpdateColor;
            }

            await base.StartAsync();
        }

        private void OnValidate()
        {
            if (_lineRenderer == null) _lineRenderer = GetComponent<LineRenderer>();
        }

        protected override void OnDestroy()
        {
            _partakStateRef.Ref.PlayerStates[_playerIndex].ColorChanged -= UpdateColor;
            base.OnDestroy();
        }

        private void UpdateColor(Color color)
        {
            if (!_lineRenderer.material.color.RGBEquals(color))
                _lineRenderer.material.color = color;
        }
    }
}