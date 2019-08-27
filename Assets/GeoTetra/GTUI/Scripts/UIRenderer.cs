using System;
using System.Collections;
using System.Collections.Generic;
using Boo.Lang;
using GeoTetra.Common.Assets;
using UnityEngine;
using GeoTetra.GTTween;
using UnityEngine.UI;

public class UIRenderer : MonoBehaviour
{
    [SerializeField] private float _displayUIOnStartDelay = .1f;
    
    [SerializeField] private UI _displayUIOnStart;

    [SerializeField] private Camera _uiCamera;

    [SerializeField] private CurveAsset _transitionCurve;

    private Stack<UI> _priorUIs = new Stack<UI>();
    private UI _currentUI;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(_displayUIOnStartDelay);
        if (_displayUIOnStart != null)
        {
            InstantiateAndDisplayUI(_displayUIOnStart);
        }
    }

    public void InstantiateAndDisplayUI(UI ui)
    {
        UI uiInstance = Instantiate(ui);
        DisplayUI(uiInstance);
    }
    
    public void DisplayUI(UI ui)
    {
        ui.CurrentlyRenderedBy = this;
        ui.Canvas.worldCamera = _uiCamera;
        ui.Canvas.renderMode = RenderMode.ScreenSpaceCamera;
        TweenIn(ui, Direction4.Up);
        
        if (_currentUI != null)
        {
            UI priorUI = _currentUI;
            TweenOut(priorUI, Direction4.Up,() => { priorUI.gameObject.SetActive(false); });
            _priorUIs.Push(priorUI);
        }

        _currentUI = ui;
    }

    private void TweenIn(UI ui, Direction4 direction)
    {
        ui.gameObject.SetActive(true);
        ui.Group.blocksRaycasts = false;
        Vector2 newPos = ui.Root.anchoredPosition;
        if (direction == Direction4.Up)
            newPos.y = -ui.Root.rect.height;
        else if (direction == Direction4.Down)
            newPos.y = ui.Root.rect.height;
        ui.Root.anchoredPosition = newPos;
        StartCoroutine(RectTransformTweens.ToAnchoredPosition(ui.Root, Vector2.zero, 1, _transitionCurve.Curve,  () => ui.Group.blocksRaycasts = true));
    }
    
    private void TweenOut(UI ui, Direction4 direction, Action OnFinish)
    {
        ui.Group.blocksRaycasts = false;
        Vector2 newPos = ui.Root.anchoredPosition;
        if (direction == Direction4.Up)
            newPos.y = ui.Root.rect.height;
        else if (direction == Direction4.Down)
            newPos.y = -ui.Root.rect.height;
        StartCoroutine(RectTransformTweens.ToAnchoredPosition(_currentUI.Root, newPos, 1, _transitionCurve.Curve, OnFinish));
    }
    
    public void GoBack()
    {
        UI priorUI = _currentUI;
        TweenOut(priorUI, Direction4.Down, () => { Destroy(priorUI.gameObject); });

        _currentUI = _priorUIs.Pop();
        TweenIn(_currentUI, Direction4.Down);
    }
}
