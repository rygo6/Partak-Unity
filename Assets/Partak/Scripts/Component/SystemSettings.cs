using GeoTetra.GTCommon.ScriptableObjects;
using UnityEngine;

namespace Partak
{
    public class SystemSettings : MonoBehaviour
    {
        private const string Version = "2.0.5";
        public bool FullVersion { get; set; }

        public int SessionCount { get; private set; }

        private void Awake()
        {
            if (PlayerPrefs.HasKey("isFullVersion"))
            {
                Debug.Log("isFullVersion");
                FullVersion = true;
            }

            SessionCount = PlayerPrefs.GetInt("SessionCount");
            Debug.Log("SessionCount: " + SessionCount);
            PlayerPrefs.SetInt("SessionCount", ++SessionCount);

            switch (PlayerPrefs.GetInt("muted"))
            {
                case 1:
                    AudioListener.volume = 1f;
                    break;
                case 2:
                    AudioListener.volume = 0f;
                    break;
            }

//		CrashReporting.Init("ff1d2528-adf9-4ba4-bf2d-d34f2ccfe587", Version);
        }
    }
}