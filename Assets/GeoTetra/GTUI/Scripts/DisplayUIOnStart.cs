using System.Collections;
using System.Threading.Tasks;
using GeoTetra.GTPooling;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

namespace GeoTetra.GTUI
{
    public class DisplayUIOnStart : MonoBehaviour
    {
        [SerializeField]
        private UIRendererServiceRef _uiRendererService;
        
        [SerializeField] private bool _onlyDisplayIfEmpty;
        [SerializeField] private UIRenderer.TransitionType _transitionType = UIRenderer.TransitionType.Vertical;
        [SerializeField] private AssetReference _stackUIReference;
        [SerializeField] private float _delay = .5f;
        [SerializeField] UnityEvent _transitionFinished = new UnityEvent();

        private async void Awake()
        {
            await _uiRendererService.Cache();
            await Task.Delay((int)(_delay * 1000));
            if (_onlyDisplayIfEmpty)
            {
                if (_uiRendererService.Service.OverlayUI.CurrentStackUI == null) _uiRendererService.Service.OverlayUI.InstantiateAndDisplayStackUI(_stackUIReference, _transitionType, OnTransitionFinish);
            }
            else
            {
                _uiRendererService.Service.OverlayUI.InstantiateAndDisplayStackUI(_stackUIReference, _transitionType, OnTransitionFinish);
            }
        }

        private void OnTransitionFinish()
        {
            _transitionFinished.Invoke();
        }
    }
}