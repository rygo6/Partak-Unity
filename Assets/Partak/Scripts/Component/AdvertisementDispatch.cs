using GeoTetra.GTCommon.ScriptableObjects;
using UnityEngine;
using UnityEngine.Advertisements;

namespace Partak
{
    public class AdvertisementDispatch : MonoBehaviour
    {
        private int _gameCount;
//        private int _gameCountLimit = 2;
//        private readonly int _gameCountLimitAdd = 2;
//        private readonly int _sessionCount = 6;

        public void ShowAdvertisement()
        {
//            if (!_componentContainer.Get<SystemSettings>().FullVersion &&
//                _componentContainer.Get<SystemSettings>().SessionCount > _sessionCount)
//            {
//                _gameCount++;
//                if (_gameCount == _gameCountLimit)
//                {
//                    _gameCount = 0;
//                    _gameCountLimit += _gameCountLimitAdd;
//#if UNITY_IOS
//                    if (Advertisement.IsReady())
//                        Advertisement.Show();
//#endif
//                }
//            }
        }
    }
}