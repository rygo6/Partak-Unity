using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace GeoTetra.GTUI
{
	public class MessageModalUI : ModalUI
	{
		[SerializeField] private Text _messageText;
		[SerializeField] private Button _okButton;

		private Action _action;
		
		protected override void Awake()
		{
			base.Awake();
			_okButton.onClick.AddListener(OkClicked);
		}

		private void OkClicked()
		{
			_action?.Invoke();
			base.Close();
		}

		public void Init(string message, Action action = null)
		{
			_messageText.text = message;
			_action = action;
		}
	}
}