using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Partak.UI
{
	public class InputPad : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IInitializePotentialDragHandler
	{
		[SerializeField]
		private CursorStore _cursorStore;

		private Vector3 _hitPosition;

		[SerializeField]
		private int _playerIndex;

		private bool _visible = true;

		private Vector3 _priorPosition;

		private Vector3 _deltaPosition;

		private bool _dragging;

		private int _particleLayer = 1 << 8;

		private LineRenderer _lineRenderer;

		private void Start()
		{
			PlayerSettings playerSettings = Persistent.Get<PlayerSettings>();
			_lineRenderer = GetComponentInChildren<LineRenderer>();
			_lineRenderer.SetColors(playerSettings.PlayerColors[_playerIndex], playerSettings.PlayerColors[_playerIndex]);
			_lineRenderer.SetVertexCount(2);
			_lineRenderer.enabled = false;
		}

		private void LateUpdate()
		{
			_cursorStore.SetCursorPositionClamp(_playerIndex, _deltaPosition);

			if (_dragging)
				_deltaPosition = Vector3.zero;
			else
				_deltaPosition = Vector3.Lerp(_deltaPosition, Vector3.zero, .1f);

			_lineRenderer.SetPosition(0, _cursorStore.CursorPositions[_playerIndex]);
			_lineRenderer.SetPosition(1, _hitPosition);
		}

		public void OnInitializePotentialDrag(PointerEventData eventData)
		{
			eventData.useDragThreshold = false;
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
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
			_hitPosition = InputParticleLayerHit(inputPosition);
			_deltaPosition = _hitPosition - _priorPosition;
			_priorPosition = _hitPosition;
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
			_lineRenderer.enabled = true;
			_priorPosition = InputParticleLayerHit(eventData.position);
			_hitPosition = _priorPosition;
			_deltaPosition = Vector3.zero;
			if (_visible)
			{
				StartCoroutine(FadeOut());
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			_lineRenderer.enabled = false;
		}

		private IEnumerator FadeOut()
		{
			CanvasGroup canvasGroup = GetComponentInChildren<CanvasGroup>();
			_visible = false;
			float alpha = 1f;
			while (alpha > 0f)
			{
				alpha -= Time.deltaTime * 2f;
				canvasGroup.alpha = alpha;
				yield return null;
			}
			canvasGroup.alpha = 0f;
		}
	}
}