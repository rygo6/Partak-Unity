using UnityEngine;
using UnityEngine.UI;

namespace GeoTetra.Partak
{
    public class LinePlayerColor : MonoBehaviour
    {
        [SerializeField] private PartakStateRef _partakStateRef;
        [SerializeField] private int _playerIndex;
        [SerializeField] private bool _constantUpdate;
        [SerializeField] private LineRenderer _lineRenderer;

        private async void Start()
        {
            await _partakStateRef.Cache();
            
            _lineRenderer.material.color = _lineRenderer.material.color.SetRGB(_partakStateRef.Service.PlayerStates[_playerIndex].PlayerColor);
            if (_constantUpdate)
            {
                _partakStateRef.Service.PlayerStates[_playerIndex].ColorChanged += UpdateColor;
            }
        }

        private void OnValidate()
        {
            if (_lineRenderer == null) _lineRenderer = GetComponent<LineRenderer>();
        }

        private void OnDestroy()
        {
            _partakStateRef.Service.PlayerStates[_playerIndex].ColorChanged -= UpdateColor;
        }

        private void UpdateColor(Color color)
        {
            if (!_lineRenderer.material.color.RGBEquals(color))
                _lineRenderer.material.color = color;
        }
    }
}