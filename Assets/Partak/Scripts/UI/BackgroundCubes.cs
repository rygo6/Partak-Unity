using System;
using System.Collections;
using System.Collections.Generic;
using GeoTetra.GTUI;
using UnityEngine;

namespace Partak
{
    public class BackgroundCubes : MonoBehaviour
    {
        [SerializeField] private string _straightToPerspectiveState = "StraightToPerspective";
        [SerializeField] private string _perspectiveToStraightState = "PerspectiveToStraight";
        [SerializeField] private Animator _animator;
        [SerializeField] private UIRenderer _uiRenderer;
        private bool _perspective = true;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(.2f);
            _uiRenderer.StackTransitionOccured.AddListener(ToggleCubePerspective);
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