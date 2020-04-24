using UnityEngine;
using UnityEngine.Purchasing;

namespace GeoTetra.Partak
{
    public class FullVersionPurchase : MonoBehaviour
    {
        [SerializeField] private IAPListener _iapListener;
        [SerializeField] private GameStateReference _gameState;

        private void Awake()
        {
            _iapListener.onPurchaseComplete.AddListener(OnPurchasesComplete);
            _iapListener.onPurchaseFailed.AddListener(OnPurchaseFail);
        }

        private void OnPurchasesComplete(Product product)
        {
            _gameState.Service.EnableFullVersion();
        }
        
        private void OnPurchaseFail(Product product, PurchaseFailureReason reason)
        {
            Debug.Log(reason);
        }
    }
}