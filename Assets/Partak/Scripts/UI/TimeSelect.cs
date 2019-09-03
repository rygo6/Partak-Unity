﻿using UnityEngine;
using System.Collections;
using GeoTetra.GTCommon.Variables;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Partak
{
    public class TimeSelect : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private GameState _gameState;
        [SerializeField] private Text _minutesText;

        private int _minutes = 1;

        private void Start()
        {
            _minutes = PlayerPrefs.GetInt("GameTime", 3);
            _minutesText.text = _minutes.ToString();
            _gameState.TimeLimitMinutes = _minutes;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            switch (_minutes)
            {
                case 1:
                    _minutes = 3;
                    break;
                case 3:
                    _minutes = 6;
                    break;
                case 6:
                    _minutes = 1;
                    break;
            }

            _gameState.TimeLimitMinutes = _minutes;
            _minutesText.text = _minutes.ToString();
            PlayerPrefs.SetInt("GameTime", _minutes);
        }
    }
}