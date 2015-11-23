using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Partak.UI
{
	public class SetPlayerMode : MonoBehaviour
	{
		[SerializeField]
		private int _playerIndex;

		[SerializeField]
		private PlayerMode _playerMode;

		private void Start()
		{
			Toggle toggle = GetComponent<Toggle>();
			if (Persistent.Get<PlayerSettings>().GetPlayerMode(_playerIndex) == _playerMode)
				toggle.isOn = true;
			else
				toggle.isOn = false;

			toggle.onValueChanged.AddListener(SetPersistent);
		}

		public void SetPersistent(bool state)
		{
			if (state)
			{
				Persistent.Get<PlayerSettings>().SetPlayerMode(_playerIndex, _playerMode);
			}
		}
	}
}