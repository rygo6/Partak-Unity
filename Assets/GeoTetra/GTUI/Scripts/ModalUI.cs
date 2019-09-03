using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace GeoTetra.GTUI
{
	public class ModalUI : BaseUI
	{
		[SerializeField] private Button _closeModalButton;

		protected override void Awake()
		{
			base.Awake();
			_closeModalButton?.onClick.AddListener(Close);
		}
		
		protected virtual void Close()
		{
			CurrentlyRenderedBy.CloseModal();
		}
	}
}