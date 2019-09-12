using System;
using System.Collections;
using System.Collections.Generic;
using GeoTetra.GTCommon.ScriptableObjects;
using UnityEngine;

namespace GeoTetra.GTUI
{
    public class DisplayUIOnStart : MonoBehaviour
    {
        [SerializeField] private ComponentContainer _componentContainer;
        [SerializeField] private StackUI _stackUI;

        private void Start()
        {
            Debug.Log($"Displaying {_stackUI}");
            UIRenderer uiRenderer = _componentContainer.Get<UIRenderer>();
            uiRenderer.InstantiateAndDisplayStackUI(_stackUI);
        }
    }
}