using UnityEngine;
using UnityEngine.UI;

namespace Partak
{
	public class MainUI : MonoBehaviour
	{
		[SerializeField]
		private Button _focusButton;

		public void FocusButton()
		{
			_focusButton.Select();
		}
	}
}