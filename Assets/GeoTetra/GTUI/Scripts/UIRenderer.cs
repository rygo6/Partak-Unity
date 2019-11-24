﻿using System;
using System.Collections;
using System.Collections.Generic;
using GeoTetra.GTCommon.Components;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using UnityEngine;
using GeoTetra.GTTween;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GeoTetra.GTUI
{
    [RequireComponent(typeof(AudioSource))]
    public class UIRenderer : SubscribableBehaviour
    {
        [SerializeField] private ComponentContainer _componentContainer;
        [SerializeField] private Camera _uiCamera;
        [SerializeField] private AnimationCurveReference _transitionCurve;
        [SerializeField] private float _transitionMultiplier = 2;
        [SerializeField] private UnityEvent _stackTransitionOccured;
        [SerializeField] private AudioSource _audioSource;

        private readonly Stack<StackUI> _priorStackUIs = new Stack<StackUI>();
        private StackUI _currentStackUI;
        private ModalUI _currentModal;

        public Camera UICamera => _uiCamera;

        public UnityEvent StackTransitionOccured => _stackTransitionOccured;

        public AudioSource AudioSource
        {
            get => _audioSource;
        }

        private void Awake()
        {
            _componentContainer.RegisterComponent(this);
        }

        private void Reset()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        protected override void OnDestroy()
        {
            _componentContainer.UnregisterComponent(this);
        }

        public async void InstantiateAndDisplayStackUI(AssetReference ui, Action onFinish = null)
        {
            StackUI uiInstance = await ui.PoolInstantiateAsync<StackUI>();
            DisplayStackUI(uiInstance, onFinish);
        }

        public void DisplayModalUI(ModalUI ui, Action onFinish = null)
        {
            ui.OnTransitionInStart(this);
            _currentModal = ui;
            StartCoroutine(
                Tweens.ToFloat(0,
                    1,
                    _transitionMultiplier,
                    _transitionCurve.Value,
                    f => ui.Group.alpha = f,
                    () =>
                    {
                        ui.Group.blocksRaycasts = true;
                        onFinish?.Invoke();
                    }));
        }

        public void CloseModal(Action onFinish = null)
        {
            ModalUI ui = _currentModal;
            _currentModal = null;
            ui.OnTransitionOutStart();
            StartCoroutine(
                Tweens.ToFloat(
                    1,
                    0,
                    _transitionMultiplier,
                    _transitionCurve.Value,
                    f => ui.Group.alpha = f,
                    () =>
                    {
                        Addressables.ReleaseInstance(ui.gameObject);
                        if (_currentStackUI != null) _currentStackUI.Group.blocksRaycasts = true;
                        onFinish?.Invoke();
                    }));
        }

        public void DisplayStackUI(StackUI ui, Action onFinish = null)
        {
            TweenIn(ui, Direction4.Up, onFinish);

            if (_currentStackUI != null)
            {
                StackUI priorUI = _currentStackUI;
                TweenOut(priorUI, Direction4.Up, null);
                _priorStackUIs.Push(priorUI);
            }

            _currentStackUI = ui;
            _stackTransitionOccured.Invoke();
        }

        public void Flush(Action onFinish = null)
        {
            StackUI currentUI = _currentStackUI;
            _currentStackUI = null;
            TweenOut(currentUI, Direction4.Down, () =>
            {
                Addressables.ReleaseInstance(currentUI.gameObject);
                onFinish?.Invoke();
            });

            while (_priorStackUIs.Count > 0)
            {
                StackUI stackUI = _priorStackUIs.Pop();
                Addressables.ReleaseInstance(stackUI.gameObject);
            }
        }

        private void TweenIn(StackUI ui, Direction4 direction, Action OnFinish)
        {
            ui.OnTransitionInStart(this);
            Vector2 newPos = ui.TransitionRoot.anchoredPosition;
            if (direction == Direction4.Up) newPos.y = -ui.TransitionRoot.rect.height;
            else if (direction == Direction4.Down) newPos.y = ui.TransitionRoot.rect.height;
            ui.TransitionRoot.anchoredPosition = newPos;
            StartCoroutine(RectTransformTweens.ToAnchoredPosition(
                ui.TransitionRoot,
                Vector2.zero,
                _transitionMultiplier,
                _transitionCurve.Value,
                () =>
                {
                    ui.OnTransitionInFinish();
                    OnFinish?.Invoke();
                } 
                ));
        }

        private void TweenOut(StackUI ui, Direction4 direction, Action OnFinish)
        {
            ui.OnTransitionOutStart();
            Vector2 newPos = ui.TransitionRoot.anchoredPosition;
            if (direction == Direction4.Up) newPos.y = ui.TransitionRoot.rect.height;
            else if (direction == Direction4.Down) newPos.y = -ui.TransitionRoot.rect.height;
            StartCoroutine(RectTransformTweens.ToAnchoredPosition(
                ui.TransitionRoot,
                newPos,
                _transitionMultiplier,
                _transitionCurve.Value,
                () =>
                {
                    ui.OnTransitionOutFinish();
                    OnFinish?.Invoke();
                }));
        }

        public void GoBack()
        {
            StackUI currentUI = _currentStackUI;
            TweenOut(currentUI, Direction4.Down, () => { Addressables.ReleaseInstance(currentUI.gameObject); });

            _currentStackUI = _priorStackUIs.Pop();
            TweenIn(_currentStackUI, Direction4.Down, null);

            _stackTransitionOccured.Invoke();
        }
    }
}