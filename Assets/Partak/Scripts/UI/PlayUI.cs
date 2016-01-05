using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;
using System.Collections;

namespace Partak
{
	public class PlayUI : MonoBehaviour
	{
		[SerializeField]
		private Button _focusButton;

		public void FocusButton()
		{
			_focusButton.Select();
		}

	}
}