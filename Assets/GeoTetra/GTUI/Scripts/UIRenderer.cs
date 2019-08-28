using System;
using System.Collections;
using System.Collections.Generic;
using Boo.Lang;
using GeoTetra.Common.Assets;
using UnityEngine;
using GeoTetra.GTTween;
using UnityEngine.UI;

namespace GeoTetra.GTUI
{
    public class UIRenderer : MonoBehaviour
    {
        [SerializeField] private float _displayUIOnStartDelay = .1f;

        [SerializeField] private BaseUI _displayUIOnStart;

        [SerializeField] private Camera _uiCamera;

        [SerializeField] private CurveAsset _transitionCurve;

        [SerializeField] private float _transitionMultiplier = 2;
        
        private Stack<BaseUI> _priorUIs = new Stack<BaseUI>();
        private BaseUI _currentUI;
        private BaseUI _currentModal;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(_displayUIOnStartDelay);
            if (_displayUIOnStart != null)
            {
                InstantiateAndDisplayUI(_displayUIOnStart);
            }
        }

        public void InstantiateAndDisplayUI(BaseUI ui)
        {
            BaseUI uiInstance = Instantiate(ui);
            DisplayUI(uiInstance);
        }

        public void DisplayModalUI(BaseUI ui)
        {
            ui.CurrentlyRenderedBy = this;
            ui.Canvas.worldCamera = _uiCamera;
            ui.Canvas.renderMode = RenderMode.ScreenSpaceCamera;
            ui.Group.blocksRaycasts = false;
            ui.gameObject.SetActive(true);
            _currentUI.Group.blocksRaycasts = false;
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
            _currentModal.Group.blocksRaycasts = false;
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
                        _currentUI.Group.blocksRaycasts = true;
                    }));
        }

        public void DisplayUI(BaseUI ui)
        {
            ui.CurrentlyRenderedBy = this;
            ui.Canvas.worldCamera = _uiCamera;
            ui.Canvas.renderMode = RenderMode.ScreenSpaceCamera;
            TweenIn(ui, Direction4.Up);

            if (_currentUI != null)
            {
                BaseUI priorUI = _currentUI;
                TweenOut(priorUI, Direction4.Up, () => { priorUI.gameObject.SetActive(false); });
                _priorUIs.Push(priorUI);
            }

            _currentUI = ui;
        }

        private void TweenIn(BaseUI ui, Direction4 direction)
        {
            ui.gameObject.SetActive(true);
            ui.Group.blocksRaycasts = false;
            Vector2 newPos = ui.Root.anchoredPosition;
            if (direction == Direction4.Up)
                newPos.y = -ui.Root.rect.height;
            else if (direction == Direction4.Down)
                newPos.y = ui.Root.rect.height;
            ui.Root.anchoredPosition = newPos;
            StartCoroutine(RectTransformTweens.ToAnchoredPosition(ui.Root, Vector2.zero, _transitionMultiplier, _transitionCurve.Curve,
                () => ui.Group.blocksRaycasts = true));
        }

        private void TweenOut(BaseUI ui, Direction4 direction, Action OnFinish)
        {
            ui.Group.blocksRaycasts = false;
            Vector2 newPos = ui.Root.anchoredPosition;
            if (direction == Direction4.Up)
                newPos.y = ui.Root.rect.height;
            else if (direction == Direction4.Down)
                newPos.y = -ui.Root.rect.height;
            StartCoroutine(RectTransformTweens.ToAnchoredPosition(_currentUI.Root, newPos, _transitionMultiplier, _transitionCurve.Curve,
                OnFinish));
        }

        public void GoBack()
        {
            BaseUI priorUI = _currentUI;
            TweenOut(priorUI, Direction4.Down, () => { Destroy(priorUI.gameObject); });

            _currentUI = _priorUIs.Pop();
            TweenIn(_currentUI, Direction4.Down);
        }
    }
}