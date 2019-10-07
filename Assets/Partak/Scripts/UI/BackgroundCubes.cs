using System;
using System.Collections;
using System.Collections.Generic;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTUI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Partak
{
    public class BackgroundCubes : MonoBehaviour
    {
        [SerializeField] private ComponentContainer _componentContainer;
        [SerializeField] private string _straightToPerspectiveState = "StraightToPerspective";
        [SerializeField] private string _perspectiveToStraightState = "PerspectiveToStraight";
        [SerializeField] private Animator _animator;
        private UIRenderer _uiRenderer;
        private bool _perspective = true;

        private IEnumerator Start()
        {
            _componentContainer.Populate(out _uiRenderer);
            yield return new WaitForSeconds(.5f);
            _uiRenderer.StackTransitionOccured.AddListener(ToggleCubePerspective);
        }

        private void OnDestroy()
        {
            _uiRenderer.StackTransitionOccured.RemoveListener(ToggleCubePerspective);
        }

        public void ToggleCubePerspective()
        {
            if (_animator.IsInTransition(0)) return;
            
            if (_animator.GetCurrentAnimatorStateInfo(0).IsName(_straightToPerspectiveState))
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