using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using GeoTetra.GTUI;
using UnityEngine.Purchasing;

namespace GeoTetra.Partak
{
    public class OptionsUI : StackUI
    {
        [SerializeField] private Button _restorePurchasedButton;
        [SerializeField] private Button _toggleSoundButton;
        [SerializeField] private Button _facebookButton;
        [SerializeField] private Button _privacyPolicyButton;

        protected override void Awake()
        {
            base.Awake();
            _restorePurchasedButton.onClick.AddListener(RestorePurchases);
            _toggleSoundButton.onClick.AddListener(Mute);
            _facebookButton.onClick.AddListener(Facebook);
            _privacyPolicyButton.onClick.AddListener(PrivacyPolicy);
#if UNITY_ANDROID
            _restorePurchasedButton.gameObject.SetActive(false);
#endif
        }
        
        private void RestorePurchases()
        {
            CodelessIAPStoreListener.Instance.ExtensionProvider.GetExtension<IAppleExtensions> ().RestoreTransactions (result => {
                if (result) {
                    Debug.Log("Restore Success.");
                    CurrentlyRenderedBy.DisplayMessageModal(
                        "Restore Succeeded.",
                        null);
                } else {
                    Debug.Log("Restore Failed.");
                    CurrentlyRenderedBy.DisplayMessageModal(
                        "Restore Failed.",
                        null);
                }
            });
        }
        
        private void Mute()
        {
            switch (PlayerPrefs.GetInt("muted"))
            {
                case 0:
                case 1:
                    AudioListener.volume = 0f;
                    PlayerPrefs.SetInt("muted", 2);
                    CurrentlyRenderedBy.DisplayMessageModal("Sound Muted");
                    break;
                case 2:
                    AudioListener.volume = 1f;
                    PlayerPrefs.SetInt("muted", 1);
                    CurrentlyRenderedBy.DisplayMessageModal("Sound Enabled");
                    break;
            }
        }

        private void Facebook()
        {
            CurrentlyRenderedBy.DisplayMessageModal(
                "You are now being sent to the GeoTetra Facebook page. GeoTetra is the studio behind partak. Follow GeoTetra on Facebook to see upcoming features, games and provide feedback.",
                () => { Application.OpenURL("https://www.facebook.com/geotetra/"); }
            );
        }

        private void PrivacyPolicy()
        {
            CurrentlyRenderedBy.DisplayMessageModal(
                "You are now being sent to the web page which displays partak's privacy policy.",
                () => { Application.OpenURL("https://rygo6.github.io/GeoTetraSite/partakprivacypolicy.html"); }
            );
        }
    }
}