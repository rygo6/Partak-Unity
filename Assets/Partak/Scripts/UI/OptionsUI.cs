using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using GeoTetra.GTUI;

namespace Partak
{
    public class OptionsUI : MonoBehaviour
    {
        [SerializeField] private Button _focusButton;

        public void RestorePurchases()
        {
//		Persistent.Get<Store>().RestorePurchases();
        }

        public void Mute()
        {
            switch (PlayerPrefs.GetInt("muted"))
            {
                case 0:
                case 1:
                    AudioListener.volume = 0f;
                    PlayerPrefs.SetInt("muted", 2);
                    GameObject.Find("PopupUI").GetComponent<ModalUI>().Show("sound muted");
                    break;
                case 2:
                    AudioListener.volume = 1f;
                    PlayerPrefs.SetInt("muted", 1);
                    GameObject.Find("PopupUI").GetComponent<ModalUI>().Show("sound enabled");
                    break;
            }
        }

        public void Facebook()
        {
            GameObject.Find("PopupUI").GetComponent<ModalUI>().Show(
                "You are now being sent to the GeoTetra Facebook page. GeoTetra is the studio behind partak. Follow GeoTetra on Facebook to see upcoming featues, games and provide feedback.",
                () => { Application.OpenURL("https://www.facebook.com/geotetra/"); }
            );
        }

        public void PrivacyPolicy()
        {
            GameObject.Find("PopupUI").GetComponent<ModalUI>().Show(
                "You are now being sent to the web page which displays partak's privacy policy.",
                () => { Application.OpenURL("https://rygo6.github.io/GeoTetraSite/partakprivacypolicy.html"); }
            );
        }

        public void FocusButton()
        {
            _focusButton.Select();
        }
    }
}