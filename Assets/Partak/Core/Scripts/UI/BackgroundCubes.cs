using GeoTetra.GTPooling;
using GeoTetra.GTUI;
using UnityEngine;

namespace GeoTetra.Partak
{
    public class BackgroundCubes : MonoBehaviour
    {
        [SerializeField]
        private UIRendererServiceRef _uiRendererService;
        
        [SerializeField] private string _fadeInState = "FadeIn";
        [SerializeField] private string _straightToPerspectiveState = "StraightToPerspective";
        [SerializeField] private string _perspectiveToStraightState = "PerspectiveToStraight";
        [SerializeField] private Animator _animator;
        
        public async void AddTransitionListener()
        {
            await _uiRendererService.Cache();
            _uiRendererService.Service.OverlayUI.StackTransitionOccured.AddListener(ToggleCubePerspective);
        }

        private void OnDestroy()
        {
            _uiRendererService.Service.OverlayUI.StackTransitionOccured.RemoveListener(ToggleCubePerspective);
        }

        public void ToggleCubePerspective()
        {
            if (_animator.IsInTransition(0)) return;
            
            if (_animator.GetCurrentAnimatorStateInfo(0).IsName(_straightToPerspectiveState) || _animator.GetCurrentAnimatorStateInfo(0).IsName(_fadeInState))
            {
                _animator.Play(_perspectiveToStraightState);
            }
            else
            {
                _animator.Play(_straightToPerspectiveState);
            }
        }
    }
}