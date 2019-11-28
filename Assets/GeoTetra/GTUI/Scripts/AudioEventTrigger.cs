using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GeoTetra.GTUI
{
    public class AudioEventTrigger : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private BaseUI _parentBaseUI;
        [SerializeField] private AudioClip _audioClip;
        [SerializeField] private float _volume = 1;

        private void OnValidate()
        {
            if (_parentBaseUI == null) _parentBaseUI = GetComponentInParent<BaseUI>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _parentBaseUI.CurrentlyRenderedBy.AudioSource.PlayOneShot(_audioClip, _volume);
        }
    }
}