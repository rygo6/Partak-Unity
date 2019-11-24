using System;
using System.Collections;
using System.Collections.Generic;
using GeoTetra.GTCommon.ScriptableObjects;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace GeoTetra.GTUI
{
    public class DisplayUIOnStart : MonoBehaviour
    {
        [SerializeField] private ComponentContainer _componentContainer;
        [SerializeField] private AssetReference _stackUIReference;
        [SerializeField] private float _delay = .5f;
        [SerializeField] UnityEvent TransitionFinished = new UnityEvent();
        
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(_delay);
            UIRenderer uiRenderer = _componentContainer.Get<UIRenderer>();
            uiRenderer.InstantiateAndDisplayStackUI(_stackUIReference, OnTransitionFinish);
        }

        private void OnTransitionFinish()
        {
            TransitionFinished.Invoke();
        }
    }
}