using System.Collections;
using UnityEngine;
using UnityEngine.Purchasing;

namespace GeoTetra.Partak
{
    public class FullVersionPurchase : MonoBehaviour
    {
        [SerializeField]
        private PartakStateRef _partakState;
        
        [SerializeField] 
        private IAPListener _iapListener;

        private void Awake()
        {
            _iapListener.onPurchaseComplete.AddListener(OnPurchasesComplete);
            _iapListener.onPurchaseFailed.AddListener(OnPurchaseFail);
        }

        private async void OnPurchasesComplete(Product product)
        {
            await _partakState.Cache();
            
            Debug.Log("Purchase Recieved " + product.definition.id);
            //May receive callback before all services init.
            StartCoroutine(EnableFullVersion());
        }

        private IEnumerator EnableFullVersion()
        {
            yield return new WaitUntil(() => _partakState.IsDone);
            _partakState.Service.EnableFullVersion();
        }

        private void OnPurchaseFail(Product product, PurchaseFailureReason reason)
        {
            Debug.Log("Full Version Purchase Fail: " + reason);
        }
    }
}