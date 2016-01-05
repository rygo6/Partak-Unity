﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;
using System.Collections;

namespace Partak
{
	public class OptionsUI : MonoBehaviour
	{
		[SerializeField]
		private Button _focusButton;

		public void RestorePurchases()
		{
			Persistent.Get<Store>().RestorePurchases();
		}

		public void FocusButton()
		{
			_focusButton.Select();
		}

	}
}