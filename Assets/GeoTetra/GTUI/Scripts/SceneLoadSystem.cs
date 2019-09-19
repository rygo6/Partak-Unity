using System;
using System.Collections;
using System.Collections.Generic;
using GeoTetra.GTCommon.Components;
using GeoTetra.GTCommon.ScriptableObjects;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GeoTetra.GTUI
{
    public class SceneLoadSystem : SubscribableBehaviour
    {
        [SerializeField] private ComponentContainer _componentContainer;
        [SerializeField] private ModalUI _loadModalUI;

        private UIRenderer _uiRenderer;

        private void Awake()
        {
            _componentContainer.RegisterComponent(this);
        }

        private void Start()
        {
            _uiRenderer = _componentContainer.Get<UIRenderer>();
        }

        public void Load(string unloadScene, string loadScene)
        {
            ModalUI loadModalUI = Instantiate(_loadModalUI);
            _uiRenderer.DisplayModalUI(loadModalUI, () => { StartCoroutine(LoadCoroutine(unloadScene, loadScene)); });
        }

        private IEnumerator LoadCoroutine(string unloadScene, string loadScene)
        {
            AsyncOperation unload = SceneManager.UnloadSceneAsync(unloadScene);
            yield return unload;
            AsyncOperation load = SceneManager.LoadSceneAsync(loadScene, LoadSceneMode.Additive);
            yield return load;
            _uiRenderer.CloseModal();
        }
    }
}