using UnityEngine;
using UnityEngine.Purchasing;

namespace Partak {
public class Store : MonoBehaviour, IStoreListener {

	private IStoreController _controller;

	private IExtensionProvider _extensions;

	private const string FullVersionIAP = "FullVersion";

	private void Awake() {
		ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
		builder.AddProduct("FullVersion", ProductType.Consumable, new IDs {
			{ "com.technologicalmages.enrgy.fullversion1", AppleAppStore.Name }
		});
		UnityPurchasing.Initialize(this, builder);
	}
		
	public void RestorePurchases() {
		_extensions.GetExtension<IAppleExtensions>().RestoreTransactions(result => {
			Debug.Log("RestoreTransactions: " + result);
		});
	}

	public void OnInitialized(IStoreController controller, IExtensionProvider extensions) {
		_controller = controller;
		_extensions = extensions;
		Debug.Log("Partak Store Initialized : " + _controller.ToString() + " " + _extensions.ToString());
	}
		
	public void OnInitializeFailed(InitializationFailureReason error) {
	}
		
	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchase) {
		Debug.Log("ProcessPurchase: " + purchase.purchasedProduct.definition.id);
		if (purchase.purchasedProduct.definition.id == FullVersionIAP) {
			Debug.Log("FullVersion Enabled");
			PlayerPrefs.SetInt("isFullVersion", 1);
			Persistent.Get<SystemSettings>().FullVersion = true;
			GameObject.Find("PopupUI").GetComponent<PopupUI>().Show("Full Version Purchase Restore");
		}
		return PurchaseProcessingResult.Complete;
	}

	public void OnPurchaseFailed(Product i, PurchaseFailureReason p) {
	}
}
}