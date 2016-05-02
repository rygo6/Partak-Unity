﻿using UnityEngine;
using UnityEngine.Advertisements;
using System.Collections;

namespace Partak {
public class AdvertisementDispatch : MonoBehaviour {

	int _gameCount;
	int _gameCountLimit = 2;
	int _gameCountLimitAdd = 2;
	int _sessionCount = 6;

	public void ShowAdvertisement() {
		if (!Persistent.Get<SystemSettings>().FullVersion && Persistent.Get<SystemSettings>().SessionCount > _sessionCount) {
			_gameCount++;
			if (_gameCount == _gameCountLimit) {
				_gameCount = 0;
				_gameCountLimit += _gameCountLimitAdd;
#if UNITY_IOS
				if (Advertisement.IsReady())
					Advertisement.Show();
#endif
			}
		}
	}
}
}