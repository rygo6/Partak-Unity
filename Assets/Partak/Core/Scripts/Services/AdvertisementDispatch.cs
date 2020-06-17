using System;
using System.Threading.Tasks;
using GeoTetra.GTPooling;
using UnityEngine;
using UnityEngine.Advertisements;

namespace GeoTetra.Partak
{
    [Serializable]
    public class AdvertisementDispatchReference : ServiceReferenceT<AdvertisementDispatch>
    {
        public AdvertisementDispatchReference(string guid) : base(guid)
        { }
    }
    
    public class AdvertisementDispatch : ServiceBehaviour, IUnityAdsListener 
    {
        [SerializeField] private string _appleAppStoreId = "1018927";
        [SerializeField] private string _googlePlayStoreId = "1018926";
        [SerializeField] private bool _testMode = true;
        [SerializeField] private string _rewardedVideoId = "rewardedVideo";
        
        private int _gameCount;
        private TaskCompletionSource<bool> _rewardedAdTask;

        private void Awake()
        {
            Advertisement.AddListener(this);
            Advertisement.Initialize(Application.platform == RuntimePlatform.Android ? _googlePlayStoreId : _appleAppStoreId, _testMode);
        }

        [ContextMenu("ShowRewardedAd")]
        public Task<bool> ShowRewardedAd()
        {
            _rewardedAdTask = new TaskCompletionSource<bool>();
            Advertisement.Show(_rewardedVideoId);
            return _rewardedAdTask.Task;
        }

        public void OnUnityAdsReady(string placementId)
        {
            Debug.Log("OnUnityAdsReady " + placementId);
            OnLoadComplete();
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