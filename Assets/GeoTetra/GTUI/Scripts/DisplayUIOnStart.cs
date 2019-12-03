using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace GeoTetra.GTUI
{
    public class DisplayUIOnStart : MonoBehaviour
    {
        [SerializeField] private bool _onlyDisplayIfEmpty;
        [SerializeField] private UIRenderer.TransitionType _transitionType = UIRenderer.TransitionType.Vertical;
        [SerializeField] private ServiceReference _componentContainer;
        [SerializeField] private AssetReference _stackUIReference;
        [SerializeField] private float _delay = .5f;
        [SerializeField] UnityEvent TransitionFinished = new UnityEvent();

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(_delay);
            UIRenderer uiRenderer = _componentContainer.Service<ComponentContainer>().Get<UIRenderer>();
            if (_onlyDisplayIfEmpty)
            {
                if (uiRenderer.CurrentStackUI == null) uiRenderer.InstantiateAndDisplayStackUI(_stackUIReference, _transitionType, OnTransitionFinish);
            }
            else
            {
                uiRenderer.InstantiateAndDisplayStackUI(_stackUIReference, _transitionType, OnTransitionFinish);
            }
        }

        private void OnTransitionFinish()
        {
            TransitionFinished.Invoke();
        }
    }
}