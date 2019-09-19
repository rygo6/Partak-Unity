using System;
using System.Collections;
using System.Collections.Generic;
using GeoTetra.GTCommon.ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;

namespace GeoTetra.GTUI
{
    public class DisplayUIOnStart : MonoBehaviour
    {
        [SerializeField] private ComponentContainer _componentContainer;
        [SerializeField] private StackUI _stackUI;
        [SerializeField] private float _delay = .5f;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(_delay);
            Debug.Log($"Displaying {_stackUI}");
            UIRenderer uiRenderer = _componentContainer.Get<UIRenderer>();
            uiRenderer.InstantiateAndDisplayStackUI(_stackUI);
        }
    }
}