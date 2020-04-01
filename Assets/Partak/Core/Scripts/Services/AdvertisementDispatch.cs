using System;
using System.Diagnostics;
using System.Threading.Tasks;
using GeoTetra.GTCommon.ScriptableObjects;
using GeoTetra.GTPooling;
using UnityEngine;
using UnityEngine.Advertisements;
using Debug = UnityEngine.Debug;

namespace GeoTetra.Partak
{
    [Serializable]
    public class AdvertisementDispatchReference : ServiceReferenceT<AdvertisementDispatch>
    {
        public AdvertisementDispatchReference(string guid) : base(guid)
        { }
    }
    
    [CreateAssetMenu(menuName = "GeoTetra/Partak/Services/AdvertisementDispatch")]
    public class AdvertisementDispatch : ScriptableObject, IUnityAdsListener 
    {
        [SerializeField] private string _appleAppStoreId = "1018927";
        [SerializeField] private string _googlePlayStoreId = "1018926";
        [SerializeField] private bool _testMode = true;
        [SerializeField] private string _rewardedVideoId = "rewardedVideo";
        
        private int _gameCount;
        private bool _showOnReady;
        private TaskCompletionSource<bool> _rewardedAdTask;
        
        private void OnDisable()
        {
            Advertisement.RemoveListener(this);
            Debug.Log("AdvertisementDispatch OnDisable");
        }

        [ContextMenu("ShowRewardedAd")]
        public Task<bool> ShowRewardedAd()
        {
            _showOnReady = false;
            _rewardedAdTask = new TaskCompletionSource<bool>();

            if (!Advertisement.isInitialized)
            {
                Advertisement.AddListener(this);
                Advertisement.Initialize(
                    Application.platform == RuntimePlatform.Android ? _googlePlayStoreId : _appleAppStoreId, _testMode);
            }

            if (Advertisement.IsReady())
            {
                Advertisement.Show(_rewardedVideoId);
            }
            else
            {
                _showOnReady = true;
            }

            return _rewardedAdTask.Task;
        }

        public void OnUnityAdsReady(string placementId)
        {
            Debug.Log("OnUnityAdsReady " + placementId);
            if (_showOnReady && _rewardedVideoId == placementId)
            {
                _showOnReady = false;
                Advertisement.Show(_rewardedVideoId);
            }
        }

        public void OnUnityAdsDidError(string message)
        {
            Debug.Log("OnUnityAdsDidError " + message);
        }

        public void OnUnityAdsDidStart(string placementId)
        {
            Debug.Log("OnUnityAdsDidStart " + placementId);
        }

        public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
        {
            Debug.Log("OnUnityAdsDidFinish " + placementId + " " + showResult);
            if (placementId == _rewardedVideoId)
            {
                _rewardedAdTask.SetResult(showResult == ShowResult.Finished);   
            }
        }
    }
}