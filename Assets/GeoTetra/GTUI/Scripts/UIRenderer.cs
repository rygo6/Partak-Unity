 using System;
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
        [SerializeField] private ServiceReference _componentContainer;
        [SerializeField] private AddressablesPool _addressablesPool;
        [SerializeField] private Camera _uiCamera;
        [SerializeField] private AnimationCurveReference _transitionCurve;
        [SerializeField] private float _transitionMultiplier = 2;
        [SerializeField] private float _fadeMultiplier = 3;
        [SerializeField] private UnityEvent _stackTransitionOccured;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AssetReference _loadModalUI;
        [SerializeField] private AssetReference _messageModalUiReference;
        [SerializeField] private AssetReference _selectionModalUiReference;

        public enum TransitionType
        {
            Fade,
            Vertical,
            Horizontal
        }
        
        private readonly Stack<StackUI> _priorStackUIs = new Stack<StackUI>();
        private StackUI _currentStackUI;
        private ModalUI _currentModal;

        public Camera UICamera => _uiCamera;
        public UnityEvent StackTransitionOccured => _stackTransitionOccured;
        public AudioSource AudioSource => _audioSource;
        public AddressablesPool Pool => _addressablesPool;
        public StackUI CurrentStackUI => _currentStackUI;
        
        private void Awake()
        {
            _componentContainer.Service<ComponentContainer>().RegisterComponent(this);
        }

        private void Reset()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        public async void InstantiateAndDisplayStackUI(AssetReference ui, TransitionType transitionType = TransitionType.Vertical, Action onFinish = null)
        {
            StackUI uiInstance = await _addressablesPool.PoolInstantiateAsync<StackUI>(ui);
            DisplayStackUI(uiInstance, transitionType, onFinish);
        }
        
        public async void InstantiateAndDisplayModalUI(AssetReference ui, Action onFinish = null)
        {
            ModalUI uiInstance = await _addressablesPool.PoolInstantiateAsync<ModalUI>(ui);
            DisplayModalUI(uiInstance, onFinish);
        }
        
        public void DisplayLoadModal()
        {
            InstantiateAndDisplayModalUI(_loadModalUI);
        }

        public async void DisplaySelectionModal(string mainMessage, string[] messages, Action[] actions, int focusIndex)
        {
            SelectionModalUI messageModalUi = await Pool.PoolInstantiateAsync<SelectionModalUI>(_selectionModalUiReference);
            messageModalUi.Init(mainMessage, messages, actions, focusIndex);
            DisplayModalUI(messageModalUi);
        }

        public async void DisplayMessageModal(string message, Action action = null)
        {
            MessageModalUI messageModalUi = await Pool.PoolInstantiateAsync<MessageModalUI>(_messageModalUiReference);
            messageModalUi.Init(message, action);
            DisplayModalUI(messageModalUi);
        }
        
        public void DisplayModalUI(ModalUI ui, Action onFinish = null)
        {
            _currentModal = ui;
            if (_currentStackUI != null) _currentStackUI.Group.blocksRaycasts = false;
            FadeIn(ui, () =>
            {
                if (_currentStackUI != null) _currentStackUI.Group.blocksRaycasts = true;
                onFinish?.Invoke();
            });
        }

        public void CloseModal(Action onFinish = null)
        {
            ModalUI ui = _currentModal;
            _currentModal = null;
            if (_currentStackUI != null) _currentStackUI.Group.blocksRaycasts = false;
            FadeOut(ui, () =>
            {
                if (_currentStackUI != null) _currentStackUI.Group.blocksRaycasts = true;
                _addressablesPool.ReleaseToPool(ui.gameObject);
                 onFinish?.Invoke();
            });
        }

        public void DisplayStackUI(StackUI ui, TransitionType transitionType,  Action onFinish = null)
        {
            if (transitionType == TransitionType.Fade) FadeIn(ui, onFinish);
            else if (transitionType == TransitionType.Vertical) TweenIn(ui, Direction4.Up, onFinish);

            if (_currentStackUI != null)
            {
                StackUI priorUI = _currentStackUI;
                TweenOut(priorUI, Direction4.Up, null);
                _priorStackUIs.Push(priorUI);
            }

            _currentStackUI = ui;
            _stackTransitionOccured.Invoke();
        }

        public void Flush(Action onFinish = null, TransitionType transitionType = TransitionType.Vertical)
        {
            StackUI currentUI = _currentStackUI;
            _currentStackUI = null;
            if (transitionType == TransitionType.Vertical)
            {
                TweenOut(currentUI, Direction4.Down, () =>
                {
                    _addressablesPool.ReleaseToPool(currentUI.gameObject);
                    onFinish?.Invoke();
                });
            }
            else if (transitionType == TransitionType.Fade)
            {
                FadeOut(currentUI, () =>
                {
                    _addressablesPool.ReleaseToPool(currentUI.gameObject);
                    onFinish?.Invoke();
                });
            }


            while (_priorStackUIs.Count > 0)
            {
                StackUI stackUI = _priorStackUIs.Pop();
                _addressablesPool.ReleaseToPool(stackUI.gameObject);
            }
        }

        private void FadeIn(BaseUI ui, Action onFinish)
        {
            ui.TransitionRoot.anchoredPosition = Vector2.zero; 
            ui.OnTransitionInStart(this);
            StartCoroutine(
                Tweens.ToFloat(0,
                    1,
                    _fadeMultiplier,
                    _transitionCurve.Value,
                    f => ui.Group.alpha = f,
                    () =>
                    {
                        ui.OnTransitionInFinish();
                        onFinish?.Invoke();
                    }));
        }
        
        private void FadeOut(BaseUI ui, Action onFinish)
        {
            ui.TransitionRoot.anchoredPosition = Vector2.zero; 
            ui.OnTransitionOutStart();
            StartCoroutine(
                Tweens.ToFloat(
                    1,
                    0,
                    _fadeMultiplier,
                    _transitionCurve.Value,
                    f => ui.Group.alpha = f,
                    () =>
                    {
                        ui.OnTransitionOutFinish();
                        onFinish?.Invoke();
                    }));
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
                }));
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
            TweenOut(currentUI, Direction4.Down, () => { _addressablesPool.ReleaseToPool(currentUI.gameObject); });

            _currentStackUI = _priorStackUIs.Pop();
            TweenIn(_currentStackUI, Direction4.Down, null);

            _stackTransitionOccured.Invoke();
        }
    }
}