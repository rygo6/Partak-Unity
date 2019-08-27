using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIRendererEventTrigger : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private bool _goBackOnClick;
    [SerializeField] private UI _displayOnClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_displayOnClick != null)
        {
            transform.root.GetComponent<UI>().CurrentlyRenderedBy.InstantiateAndDisplayUI(_displayOnClick);
        }
        else if (_goBackOnClick)
        {
            transform.root.GetComponent<UI>().CurrentlyRenderedBy.GoBack();
        }
    }
}