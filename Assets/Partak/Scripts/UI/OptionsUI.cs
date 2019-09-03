using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using GeoTetra.GTUI;

namespace Partak
{
    public class OptionsUI : StackUI
    {
        [SerializeField] private Button _toggleSoundButton;
        [SerializeField] private Button _facebookButton;
        [SerializeField] private Button _privacyPolicyButton;
        [SerializeField] private Button _restorePurchasesButton;

        protected override void Awake()
        {
            base.Awake();
            _toggleSoundButton.onClick.AddListener(Mute);
            _facebookButton.onClick.AddListener(Facebook);
            _privacyPolicyButton.onClick.AddListener(PrivacyPolicy);
            _restorePurchasesButton.onClick.AddListener(RestorePurchases);
        }

        private void RestorePurchases()
        {
//		Persistent.Get<Store>().RestorePurchases();
        }

        private void Mute()
        {
            switch (PlayerPrefs.GetInt("muted"))
            {
                case 0:
                case 1:
                    AudioListener.volume = 0f;
                    PlayerPrefs.SetInt("muted", 2);
                    DisplayModal("Sound Muted");
                    break;
                case 2:
                    AudioListener.volume = 1f;
                    PlayerPrefs.SetInt("muted", 1);
                    DisplayModal("Sound Enabled");
                    break;
            }
        }

        private void Facebook()
        {
            DisplayModal(
                "You are now being sent to the GeoTetra Facebook page. GeoTetra is the studio behind partak. Follow GeoTetra on Facebook to see upcoming features, games and provide feedback.",
                () => { Application.OpenURL("https://www.facebook.com/geotetra/"); }
            );
        }

        private void PrivacyPolicy()
        {
            DisplayModal(
                "You are now being sent to the web page which displays partak's privacy policy.",
                () => { Application.OpenURL("https://rygo6.github.io/GeoTetraSite/partakprivacypolicy.html"); }
            );
        }
    }
}