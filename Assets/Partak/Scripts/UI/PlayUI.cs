using UnityEngine;
using UnityEngine.UI;
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