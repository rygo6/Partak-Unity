using System.Threading.Tasks;
using GeoTetra.GTCommon.Components;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

namespace GeoTetra.GTUI
{
    public class LoadScene : SubscribableBehaviour
    {
        [SerializeField] private SceneTransitRef _sceneTransit;
        [SerializeField] private bool _onStart;
        [SerializeField] private AssetReference _sceneReference;
        [SerializeField] private UnityEvent _onLoadComplete;

        protected override Task StartAsync()
        {
            if (_onStart) Load();
            return base.StartAsync();
        }

        public async void Load()
        {
            await _sceneTransit.Cache(this);
            await _sceneTransit.Ref.Load(null, _sceneReference);
            _onLoadComplete.Invoke();
        }
    }
}