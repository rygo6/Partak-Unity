using System;
using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private bool _onStart;
        [SerializeField] private ServiceReference _sceneLoadSystem;
        [SerializeField] private AssetReference _sceneReference;

        private void Start()
        {
            if (_onStart) Load();
        }

        public void Load()
        {
            _sceneLoadSystem.Service<SceneLoadSystem>().Load(null, _sceneReference);
        }
    }
}