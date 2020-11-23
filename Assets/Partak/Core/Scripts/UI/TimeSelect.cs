using UnityEngine;
using System.Collections;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GeoTetra.Partak
{
    public class TimeSelect : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] 
        private PartakStateRef _partakStateRef;
        
        [SerializeField] private Text _minutesText;

        private int _minutes = 1;

        private async void Awake()
        {
            await _partakStateRef.Cache();
            _minutes = PlayerPrefs.GetInt("GameTime", 3);
            _minutesText.text = _minutes.ToString();
            _partakStateRef.Service.TimeLimitMinutes = _minutes;
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

            _partakStateRef.Service.TimeLimitMinutes = _minutes;
            _minutesText.text = _minutes.ToString();
            PlayerPrefs.SetInt("GameTime", _minutes);
        }
    }
}