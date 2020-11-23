using System;
using System.Collections;
using System.Collections.Generic;
using Amazon.Util;
using GeoTetra.GTCommon.Attributes;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace GeoTetra.GTUI
{
    public class LoadScene : MonoBehaviour
    {
        [SerializeField] private SceneTransitRef _sceneTransit;
        [SerializeField] private bool _onStart;
        [SerializeField] private AssetReference _sceneReference;

        private void Start()
        {
            if (_onStart) Load();
        }

        public async void Load()
        {
            await _sceneTransit.Cache();
            _sceneTransit.Service.Load(null, _sceneReference);
        }
    }
}