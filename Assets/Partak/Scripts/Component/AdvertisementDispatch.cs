using UnityEngine;
using UnityEngine.Advertisements;
using System.Collections;

namespace Partak
{
	public class AdvertisementDispatch : MonoBehaviour
	{
		private const string AdvertisementCountKey = "AdveristementCount";

		private int _gameCount;

		private int _gameCountLimit = 3;

		public void ShowAdvertisement()
		{
			Debug.Log("ShowAdvertisement " + Persistent.Get<SystemSettings>().FullVersion + " " + _gameCount + " " + _gameCountLimit + " " + Persistent.Get<SystemSettings>().SessionCount);
			if (!Persistent.Get<SystemSettings>().FullVersion && Persistent.Get<SystemSettings>().SessionCount > 3)
			{
				_gameCount++;
				if (_gameCount == _gameCountLimit)
				{
					_gameCount = 0;
					_gameCountLimit++;
					if (Advertisement.IsReady())
						Advertisement.Show();
				}
			}
		}
	}
}