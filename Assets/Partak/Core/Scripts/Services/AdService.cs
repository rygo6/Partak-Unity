using System;
using System.Threading.Tasks;
using GeoTetra.GTPooling;
using UnityEngine;
using UnityEngine.Advertisements;

namespace GeoTetra.Partak
{
    [Serializable]
    public class AdServiceRef : ServiceObjectReferenceT<AdService>
    {
        public AdServiceRef(string guid) : base(guid)
        { }
    }
    
    [CreateAssetMenu(menuName = "GeoTetra/Services/UnityAdService", fileName = "UnityAdService.asset")]
    public class AdService : ServiceObject, IUnityAdsListener 
    {
        [SerializeField] private string _appleAppStoreId = "1018927";
        [SerializeField] private string _googlePlayStoreId = "1018926";
        [SerializeField] private bool _testMode = true;
        [SerializeField] private string _rewardedVideoId = "rewardedVideo";
        
        private int _gameCount;
        private TaskCompletionSource<bool> _initializeTask;
        private TaskCompletionSource<bool> _rewardedAdTask;

        protected override void OnServiceEnd()
        {
            _initializeTask = null;
            _rewardedAdTask = null;
            base.OnServiceEnd();
        }

        private Task Initialize()
        {
            if (!Advertisement.isInitialized)
            {
                _initializeTask = new TaskCompletionSource<bool>();
                Advertisement.AddListener(this);
                Advertisement.Initialize(Application.platform == RuntimePlatform.Android ? _googlePlayStoreId : _appleAppStoreId, _testMode);
                return _initializeTask.Task;
            }

            return Task.CompletedTask;
        }

        [ContextMenu("ShowRewardedAd")]
        public async Task ShowRewardedAd()
        {
            await Initialize();
            _rewardedAdTask = new TaskCompletionSource<bool>();
            Advertisement.Show(_rewardedVideoId);
            await _rewardedAdTask.Task;
        }

        public void OnUnityAdsReady(string placementId)
        {
            Debug.Log("OnUnityAdsReady " + placementId);
            if (placementId == _rewardedVideoId && !_initializeTask.Task.IsCompleted) _initializeTask.SetResult(true);
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