using GeoTetra.GTPooling;
using UnityEngine;
using UnityEngine.UI;

namespace GeoTetra.Partak
{
    public class ImagePlayerColor : MonoBehaviour
    {
        [SerializeField] private GameStateReference _gameState;
        [SerializeField] private int _playerIndex;
        [SerializeField] private bool _constantUpdate;

        private Image _image;

        private void Start()
        {
            _image = GetComponent<Image>();
            _image.color = _image.color.SetRGB(_gameState.Service.PlayerStates[_playerIndex].PlayerColor);
            if (_constantUpdate)
            {
                _gameState.Service.PlayerStates[_playerIndex].ColorChanged += UpdateColor;
            }
        }

        private void OnDestroy()
        {
            _gameState.Service.PlayerStates[_playerIndex].ColorChanged -= UpdateColor;
        }

        private void UpdateColor(Color color)
        {
            if (!_image.color.RGBEquals(color))
                _image.color = color;
        }
    }
}