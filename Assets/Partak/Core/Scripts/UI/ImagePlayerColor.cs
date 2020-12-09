using System;
using System.Threading.Tasks;
using GeoTetra.GTCommon.Components;
using UnityEngine;
using UnityEngine.UI;

namespace GeoTetra.Partak
{
    public class ImagePlayerColor : SubscribableBehaviour
    {
        [SerializeField] private PartakStateRef _partakStateRef;
        [SerializeField] private int _playerIndex;
        [SerializeField] private bool _constantUpdate;
        [SerializeField] private Image _image;

        protected override async Task StartAsync()
        {
            await _partakStateRef.Cache(this);
            
            _image.color = _image.color.SetRGB(_partakStateRef.Ref.PlayerStates[_playerIndex].PlayerColor);
            if (_constantUpdate)
            {
                _partakStateRef.Ref.PlayerStates[_playerIndex].ColorChanged += UpdateColor;
            }

            await base.StartAsync();
        }

        private void OnValidate()
        {
            if (_image == null) _image = GetComponent<Image>();
        }

        protected override void OnDestroy()
        {
            _partakStateRef.Ref.PlayerStates[_playerIndex].ColorChanged -= UpdateColor;
            base.OnDestroy();
        }

        private void UpdateColor(Color color)
        {
            if (!_image.color.RGBEquals(color))
                _image.color = color;
        }
    }
}