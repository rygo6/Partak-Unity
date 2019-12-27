using System;
using System.Collections;
using System.Collections.Generic;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using GeoTetra.GTUI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Partak
{
    public class BackgroundCubes : MonoBehaviour
    {
        [SerializeField] private ServiceReference _componentContainer;
        [SerializeField] private string _fadeInState = "FadeIn";
        [SerializeField] private string _straightToPerspectiveState = "StraightToPerspective";
        [SerializeField] private string _perspectiveToStraightState = "PerspectiveToStraight";
        [SerializeField] private Animator _animator;
        private UIRenderer _uiRenderer;

        private void Start()
        {
            _componentContainer.Service<ComponentContainer>().Populate(out _uiRenderer);
        }

        public void AddTransitionListener()
        {
            _uiRenderer.StackTransitionOccured.AddListener(ToggleCubePerspective);
        }

        private void OnDestroy()
        {
            _uiRenderer.StackTransitionOccured.RemoveListener(ToggleCubePerspective);
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