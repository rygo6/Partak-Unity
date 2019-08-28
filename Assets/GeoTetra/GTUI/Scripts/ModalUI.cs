using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace GeoTetra.GTUI
{
	public class ModalUI : BaseUI
	{
		[SerializeField]
		private Text _messageText;

		private Action _action;

		void Awake()
		{
			Close();
		}

		public void Close()
		{
			gameObject.SetActive(false);
			if (_action != null)
			{
				_action();
				_action = null;
			}
		}

		public void Show(string message, Action action = null)
		{
			_messageText.text = message;
			gameObject.SetActive(true);
			_action = action;
		}
	}
}