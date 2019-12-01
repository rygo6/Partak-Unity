using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Markup;
using GeoTetra.GTCommon.Components;
using GeoTetra.GTCommon.ScriptableObjects;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace GeoTetra.GTUI
{
    [CreateAssetMenu(menuName = "GeoTetra/UI/SceneLoadSystem")]
    public class SceneLoadSystem : ScriptableObject
    {
        [SerializeField] private ComponentContainer _componentContainer;
        [SerializeField] private AssetReference _loadModalUI;
        
        private readonly Dictionary<string, SceneInstance> _loadedSceneInstances = new Dictionary<string, SceneInstance>();
        private UIRenderer _uiRenderer;

        private UIRenderer UIRenderer => _uiRenderer != null ? _uiRenderer : _uiRenderer = _componentContainer.Get<UIRenderer>();

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
                if (_loadedSceneInstances.TryGetValue(unloadLocation.PrimaryKey, out SceneInstance sceneInstance))
                {
                    _loadedSceneInstances.Remove(unloadLocation.PrimaryKey);
                    await Addressables.UnloadSceneAsync(sceneInstance).Task;                   
                }
                else
                {
                    Debug.LogWarning($"{unloadScene} Scene instance not found.");
                }
            }
            SceneInstance loaded = await loadScene.LoadSceneAsync(LoadSceneMode.Additive).Task;
            IResourceLocation loadLocation = GetResourceLocation(loadScene);
            if (loadLocation == null) Debug.LogWarning($"Can't find location of {loaded.Scene.name}'");
            else _loadedSceneInstances.Add(loadLocation.PrimaryKey, loaded);
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