using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputPad : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IInitializePotentialDragHandler
{
	[SerializeField]
	private CursorStore _cursorStore;

	[SerializeField]
	private int _playerIndex;

	private bool _visible = true;

	private Vector3 _priorPosition;

	private Vector3 _deltaPosition;

	private bool _dragging;

	private int _particleLayer = 1 << 8;

	private void LateUpdate()
	{
		Vector3 newPos = _cursorStore.CursorPositions[_playerIndex];
		newPos.x += _deltaPosition.x;
		newPos.z += _deltaPosition.z;
		_cursorStore.CursorPositions[_playerIndex] = newPos;
	
		if (_dragging)
			_deltaPosition = Vector3.zero;
		else
			_deltaPosition = Vector3.Lerp(_deltaPosition, Vector3.zero, .1f);
	}

	public void OnInitializePotentialDrag(PointerEventData eventData)
	{
		eventData.useDragThreshold = false;
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		_priorPosition = InputParticleLayerHit(eventData.position);
		_dragging = true;
	}

	public void OnDrag(PointerEventData eventData)
	{
		CalculateDeltaPosition(eventData.position);
	}

	public void OnEndDrag(PointerEventData eventData)
	{		
		CalculateDeltaPosition(eventData.position);
		_dragging = false;
	}

	private void CalculateDeltaPosition(Vector2 inputPosition)
	{
		Vector3 currentPosition = InputParticleLayerHit(inputPosition);
		_deltaPosition = currentPosition - _priorPosition;
		_priorPosition = currentPosition;
	}

	private Vector3 InputParticleLayerHit(Vector2 inputPosition)
	{
		Ray ray = Camera.main.ScreenPointToRay(inputPosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, 200f, _particleLayer))
		{
			return hit.point;
		}
		return Vector3.zero;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		_deltaPosition = Vector3.zero;
		if (_visible)
		{
			StartCoroutine(FadeOut());
		}
	}

	private IEnumerator FadeOut()
	{
		_visible = false;
		float alpha = 1f;
		while (alpha > 0f)
		{
			alpha -= Time.deltaTime * 2f;
			GetComponent<CanvasGroup>().alpha = alpha;
			yield return null;
		}
		GetComponent<CanvasGroup>().alpha = 0f;
	}
}
