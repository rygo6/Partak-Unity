using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace Partak
{
	public class PopupUI : MonoBehaviour
	{
		[SerializeField]
		private Text _messageText;

		private Canvas _canvas;

		private Camera _camera;

		private Action _action;

		void Awake()
		{
			_canvas = GetComponentInChildren<Canvas>();
			_camera = GetComponentInChildren<Camera>();
			Close();
		}

		public void Close()
		{
			_canvas.enabled = false;
			_camera.gameObject.SetActive(false);
			if (_action != null)
			{
				_action();
				_action = null;
			}
		}

		public void Show(string message, Action action = null)
		{
			_messageText.text = message;
			_canvas.enabled = true;
			_camera.gameObject.SetActive(true);
			_action = action;
		}
	}
}