using System.Collections;
using GeoTetra.GTPooling;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

namespace GeoTetra.GTUI
{
    public class DisplayUIOnStart : MonoBehaviour
    {
        [SerializeField] 
        [AssetReferenceComponentRestriction(typeof(UIRenderer))]
        private UIRendererReference _uiRenderer;
        
        [SerializeField] private bool _onlyDisplayIfEmpty;
        [SerializeField] private UIRenderer.TransitionType _transitionType = UIRenderer.TransitionType.Vertical;
        [SerializeField] private AssetReference _stackUIReference;
        [SerializeField] private float _delay = .5f;
        [SerializeField] UnityEvent _transitionFinished = new UnityEvent();

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(_delay);
            if (_onlyDisplayIfEmpty)
            {
                if (_uiRenderer.Service.CurrentStackUI == null) _uiRenderer.Service.InstantiateAndDisplayStackUI(_stackUIReference, _transitionType, OnTransitionFinish);
            }
            else
            {
                _uiRenderer.Service.InstantiateAndDisplayStackUI(_stackUIReference, _transitionType, OnTransitionFinish);
            }
        }

        private void OnTransitionFinish()
        {
            _transitionFinished.Invoke();
        }
    }
}