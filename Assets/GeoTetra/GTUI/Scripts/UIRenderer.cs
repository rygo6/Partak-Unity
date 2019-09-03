using System;
using System.Collections;
using System.Collections.Generic;
using Boo.Lang;
using GeoTetra.GTCommon.Assets;
using UnityEngine;
using GeoTetra.GTTween;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GeoTetra.GTUI
{
    public class UIRenderer : MonoBehaviour
    {
        [FormerlySerializedAs("_displayUIOnStartDelay")] [SerializeField] private float _displayStackUIOnStartDelay = .1f;
        [FormerlySerializedAs("_displayUIOnStart")] [SerializeField] private StackUI _displayStackUIOnStart;
        [SerializeField] private Camera _uiCamera;
        [SerializeField] private CurveAsset _transitionCurve;
        [SerializeField] private float _transitionMultiplier = 2;
        [SerializeField] private UnityEvent _stackTransitionOccured;
        
        private readonly Stack<StackUI> _priorStackUIs = new Stack<StackUI>();
        private StackUI _currentStackUI;
        private ModalUI _currentModal;

        public Camera UICamera => _uiCamera;

        public UnityEvent StackTransitionOccured => _stackTransitionOccured;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(_displayStackUIOnStartDelay);
            if (_displayStackUIOnStart != null)
            {
                InstantiateAndDisplayStackUI(_displayStackUIOnStart);
            }
        }

        public void InstantiateAndDisplayStackUI(StackUI ui)
        {
            StackUI uiInstance = Instantiate(ui);
            DisplayStackUI(uiInstance);
        }

        public void DisplayModalUI(ModalUI ui)
        {
            ui.OnTransitionInStart(this);
            _currentStackUI.Group.blocksRaycasts = false;
            _currentModal = ui;
            StartCoroutine(
                Tweens.ToFloat(0,
                    1,
                    _transitionMultiplier,
                    _transitionCurve.Curve,
                    f => ui.Group.alpha = f,
                    () =>
                    {
                        ui.Group.blocksRaycasts = true;
                    }));
        }

        public void CloseModal()
        {
            _currentModal.OnTransitionOutStart();
            StartCoroutine(
                Tweens.ToFloat(
                    1,
                    0,
                    _transitionMultiplier, 
                    _transitionCurve.Curve,
                    f => _currentModal.Group.alpha = f,
                    () =>
                    {
                        Destroy(_currentModal.gameObject);
                        _currentStackUI.Group.blocksRaycasts = true;
                    }));
        }

        public void DisplayStackUI(StackUI ui)
        {
            TweenIn(ui, Direction4.Up);

            if (_currentStackUI != null)
            {
                StackUI priorUI = _currentStackUI;
                TweenOut(priorUI, Direction4.Up,  priorUI.OnTransitionOutFinish);
                _priorStackUIs.Push(priorUI);
            }

            _currentStackUI = ui;
            
            _stackTransitionOccured.Invoke();
        }

        private void TweenIn(StackUI ui, Direction4 direction)
        {
            ui.OnTransitionInStart(this);
            Vector2 newPos = ui.Root.anchoredPosition;
            if (direction == Direction4.Up)
                newPos.y = -ui.Root.rect.height;
            else if (direction == Direction4.Down)
                newPos.y = ui.Root.rect.height;
            ui.Root.anchoredPosition = newPos;
            StartCoroutine(RectTransformTweens.ToAnchoredPosition(
                ui.Root, 
                Vector2.zero, 
                _transitionMultiplier, 
                _transitionCurve.Curve,
                ui.OnTransitionInFinish));
        }

        private void TweenOut(StackUI ui, Direction4 direction, Action OnFinish)
        {
            ui.OnTransitionOutStart();
            Vector2 newPos = ui.Root.anchoredPosition;
            if (direction == Direction4.Up)
                newPos.y = ui.Root.rect.height;
            else if (direction == Direction4.Down)
                newPos.y = -ui.Root.rect.height;
            StartCoroutine(RectTransformTweens.ToAnchoredPosition(_currentStackUI.Root, newPos, _transitionMultiplier, _transitionCurve.Curve,
                OnFinish));
        }

        public void GoBack()
        {
            StackUI priorUI = _currentStackUI;
            TweenOut(priorUI, Direction4.Down, () => { Destroy(priorUI.gameObject); });

            _currentStackUI = _priorStackUIs.Pop();
            TweenIn(_currentStackUI, Direction4.Down);
            
            _stackTransitionOccured.Invoke();
        }
    }
}