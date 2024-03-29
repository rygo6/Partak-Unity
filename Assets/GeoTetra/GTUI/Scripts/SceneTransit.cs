﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GeoTetra.GTPooling;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace GeoTetra.GTUI
{
    [System.Serializable]
    public class SceneTransitRef : ServiceObjectReferenceT<SceneTransit>
    {
        public SceneTransitRef(string guid) : base(guid)
        { }
    }
    
    [CreateAssetMenu(menuName = "GeoTetra/Services/SceneTransit", fileName = "SceneTransit.asset")]
    public class SceneTransit : ServiceObject
    {
        [SerializeField]
        private UIRendererServiceRef _uiRendererService;
        
        [SerializeField] 
        private AssetReference _loadModalUI;
        
        private readonly Dictionary<string, AsyncOperationHandle<SceneInstance>> _loadedSceneInstances = new Dictionary<string,  AsyncOperationHandle<SceneInstance>>();
        
        protected override async Task OnServiceStart()
        {
            await _uiRendererService.Cache(this);
            await base.OnServiceStart();
        }

        public async Task Load(AssetReference unloadScene, AssetReference loadScene)
        {
            await Starting;
            await _uiRendererService.Ref.OverlayUI.DisplayLoadModal("Loading Scene...");
            await LoadCoroutine(unloadScene, loadScene);
        }

        private async Task LoadCoroutine(AssetReference unloadScene, AssetReference loadScene)
        {
            if (unloadScene != null && !string.IsNullOrEmpty(unloadScene.AssetGUID))
            {
                IResourceLocation unloadLocation = GetResourceLocation(unloadScene);
                if (_loadedSceneInstances.TryGetValue(unloadLocation.PrimaryKey, out  AsyncOperationHandle<SceneInstance> unloadHandle))
                {
                    Debug.Log("Removing scene "+ unloadLocation.PrimaryKey);
                    _loadedSceneInstances.Remove(unloadLocation.PrimaryKey);
                    try
                    {
                        await Addressables.UnloadSceneAsync(unloadHandle.Result, true).Task;
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(e);
                    }
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
            _uiRendererService.Ref.OverlayUI.CloseModal();
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