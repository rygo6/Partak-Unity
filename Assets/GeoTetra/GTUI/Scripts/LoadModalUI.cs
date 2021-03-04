using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace GeoTetra.GTUI
{
	public class LoadModalUI : ModalUI
	{
		[SerializeField] private Text _messageText;

		private Action _action;

		public void Init(string message)
		{
			_messageText.text = message;
		}
	}
}