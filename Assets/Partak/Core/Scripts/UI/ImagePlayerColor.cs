using UnityEngine;
using UnityEngine.UI;

namespace GeoTetra.Partak
{
    public class ImagePlayerColor : MonoBehaviour
    {
        [SerializeField] private GameStateRef _gameStateRef;
        [SerializeField] private int _playerIndex;
        [SerializeField] private bool _constantUpdate;

        private Image _image;

        private async void Start()
        {
            await _gameStateRef.Cache();
            
            _image = GetComponent<Image>();
            _image.color = _image.color.SetRGB(_gameStateRef.Service.PlayerStates[_playerIndex].PlayerColor);
            if (_constantUpdate)
            {
                _gameStateRef.Service.PlayerStates[_playerIndex].ColorChanged += UpdateColor;
            }
        }

        private void OnDestroy()
        {
            _gameStateRef.Service.PlayerStates[_playerIndex].ColorChanged -= UpdateColor;
        }

        private void UpdateColor(Color color)
        {
            if (!_image.color.RGBEquals(color))
                _image.color = color;
        }
    }
}