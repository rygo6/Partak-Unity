using System.Threading.Tasks;
using GeoTetra.GTCommon.Components;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

namespace GeoTetra.GTUI
{
    public class DisplayUIOnStart : SubscribableBehaviour
    {
        [SerializeField]
        private UIRendererServiceRef _uiRendererService;
        
        [SerializeField] private bool _onlyDisplayIfEmpty;
        [SerializeField] private UIRenderer.TransitionType _transitionType = UIRenderer.TransitionType.Vertical;
        [SerializeField] private AssetReference _stackUIReference;
        [SerializeField] private float _delay = .5f;
        [SerializeField] UnityEvent _transitionFinished = new UnityEvent();

        protected override async Task StartAsync()
        {
            await _uiRendererService.Cache(this);
            await Task.Delay((int)(_delay * 1000));
            if (_onlyDisplayIfEmpty)
            {
                if (_uiRendererService.Ref.OverlayUI.CurrentStackUI == null) _uiRendererService.Ref.OverlayUI.InstantiateAndDisplayStackUI(_stackUIReference, _transitionType, OnTransitionFinish);
            }
            else
            {
                _uiRendererService.Ref.OverlayUI.InstantiateAndDisplayStackUI(_stackUIReference, _transitionType, OnTransitionFinish);
            }
            await base.StartAsync();
        }

        private void OnTransitionFinish()
        {
            _transitionFinished.Invoke();
        }
    }
}