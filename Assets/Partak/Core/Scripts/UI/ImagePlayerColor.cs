﻿using System;
using UnityEngine;
using UnityEngine.UI;

namespace GeoTetra.Partak
{
    public class ImagePlayerColor : MonoBehaviour
    {
        [SerializeField] private PartakStateRef _partakStateRef;
        [SerializeField] private int _playerIndex;
        [SerializeField] private bool _constantUpdate;
        [SerializeField] private Image _image;

        private async void Start()
        {
            await _partakStateRef.Cache();
            
            _image.color = _image.color.SetRGB(_partakStateRef.Service.PlayerStates[_playerIndex].PlayerColor);
            if (_constantUpdate)
            {
                _partakStateRef.Service.PlayerStates[_playerIndex].ColorChanged += UpdateColor;
            }
        }

        private void OnValidate()
        {
            if (_image == null) _image = GetComponent<Image>();
        }

        private void OnDestroy()
        {
            _partakStateRef.Service.PlayerStates[_playerIndex].ColorChanged -= UpdateColor;
        }

        private void UpdateColor(Color color)
        {
            if (!_image.color.RGBEquals(color))
                _image.color = color;
        }
    }
}