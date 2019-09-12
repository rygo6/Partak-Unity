using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GeoTetra.GTUI
{
    public class AudioEventTrigger : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private AudioClip _audioClip;
        [SerializeField] private float _volume = 1;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            transform.root.GetComponent<BaseUI>().CurrentlyRenderedBy.AudioSource.PlayOneShot(_audioClip, _volume);
        }
    }
}