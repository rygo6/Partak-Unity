using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace GeoTetra.GTUI
{
	public class ModalUI : BaseUI
	{
		[SerializeField] private List<Button> _closeModalButtons;

		protected override void Awake()
		{
			base.Awake();
			for (int i = 0; i < _closeModalButtons.Count; ++i)
			{
				_closeModalButtons[i].onClick.AddListener(Close);
			}
		}
		
		protected virtual void Close()
		{
			CurrentlyRenderedBy.CloseModal();
		}
	}
}