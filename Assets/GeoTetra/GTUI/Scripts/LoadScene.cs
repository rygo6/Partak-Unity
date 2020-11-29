using System.Threading.Tasks;
using GeoTetra.GTCommon.Components;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GeoTetra.GTUI
{
    public class LoadScene : SubscribableBehaviour
    {
        [SerializeField] private SceneTransitRef _sceneTransit;
        [SerializeField] private bool _onStart;
        [SerializeField] private AssetReference _sceneReference;

        protected override Task StartAsync()
        {
            if (_onStart) Load();
            return base.StartAsync();
        }

        public async void Load()
        {
            await _sceneTransit.Cache(this);
            _sceneTransit.Service.Load(null, _sceneReference);
        }
    }
}