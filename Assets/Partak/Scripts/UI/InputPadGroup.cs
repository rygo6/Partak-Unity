using UnityEngine;
using System.Collections;

namespace Partak.UI
{
	public class InputPadGroup : MonoBehaviour
	{
		[SerializeField]
		private InputPad[] _inputPads;

		[SerializeField]
		private GameObject _horizontalTop;

		[SerializeField]
		private GameObject _horizontalBottom;

		private void Start()
		{
			PlayerSettings playerSettings = Persistent.Get<PlayerSettings>();
			for (int i = 0; i < _inputPads.Length; ++i)
			{
				if (playerSettings.GetPlayerMode(i) != PlayerMode.Human)
				{
					_inputPads[i].gameObject.SetActive(false);
				}
			}

			if (!_inputPads[0].gameObject.activeSelf && !_inputPads[1].gameObject.activeSelf)
			{
				_horizontalTop.SetActive(false);
			}
			if (!_inputPads[2].gameObject.activeSelf && !_inputPads[3].gameObject.activeSelf)
			{
				_horizontalBottom.SetActive(false);
			}
		}
	}
}