﻿using System.Collections.Generic;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace GeoTetra.GTUI
{
    [CreateAssetMenu(menuName = "GeoTetra/Services/SceneLoadSystem")]
    public class SceneLoadSystem : ScriptableObject
    {
        [SerializeField] private ServiceReference _componentContainer;
        [SerializeField] private AssetReference _loadModalUI;
        
        private readonly Dictionary<string, AsyncOperationHandle<SceneInstance>> _loadedSceneInstances = new Dictionary<string,  AsyncOperationHandle<SceneInstance>>();
        private UIRenderer _uiRenderer;

        private UIRenderer UIRenderer => _uiRenderer != null ? _uiRenderer : _uiRenderer = _componentContainer.Service<ComponentContainer>().Get<UIRenderer>();

        public void Load(AssetReference unloadScene, AssetReference loadScene)
        {
            UIRenderer.InstantiateAndDisplayModalUI(_loadModalUI, () => {
                LoadCoroutine(unloadScene, loadScene);
            });
        }

        private async void LoadCoroutine(AssetReference unloadScene, AssetReference loadScene)
        {
            if (unloadScene != null && !string.IsNullOrEmpty(unloadScene.AssetGUID))
            {
                IResourceLocation unloadLocation = GetResourceLocation(unloadScene);
                if (_loadedSceneInstances.TryGetValue(unloadLocation.PrimaryKey, out  AsyncOperationHandle<SceneInstance> unloadHandle))
                {
                    Debug.Log("Removing scene "+ unloadLocation.PrimaryKey);
                    _loadedSceneInstances.Remove(unloadLocation.PrimaryKey);
                    await Addressables.UnloadSceneAsync(unloadHandle.Result, true).Task;
                }
                else
                {
                    Debug.LogWarning($"{unloadScene} Scene instance not found.");
                }
            }
            IResourceLocation loadLocation = GetResourceLocation(loadScene);
            AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(loadLocation, LoadSceneMode.Additive);    
            await handle.Task;
            if (loadLocation == null) Debug.LogWarning($"Can't find location of {handle.Result.Scene.name}'");
            else _loadedSceneInstances.Add(loadLocation.PrimaryKey, handle);
            UIRenderer.CloseModal();
        }
        
        //below should be moved to UTIL class? Or could this whole thing be put in Pooling?
        private IResourceLocation GetResourceLocation(object key)
        {
            key = EvaluateKey(key);
            IList<IResourceLocation> locs;
            foreach (var rl in Addressables.ResourceLocators)
            {
                if (rl.Locate(key, typeof(SceneInstance), out locs))
                    return locs[0];
            }

            return null;
        }
        
        private object EvaluateKey(object obj)
        {
            if (obj is IKeyEvaluator)
                return (obj as IKeyEvaluator).RuntimeKey;
            return obj;
        }
    }
}