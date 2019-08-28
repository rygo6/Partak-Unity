using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using GeoTetra.GTUI;

namespace Partak.UI
{
    public class StartGame : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            MenuConfig playerSettings = Persistent.Get<MenuConfig>();
            int activeCount = 0;
            for (int i = 0; i < MenuConfig.MaxPlayers; ++i)
            {
                if (playerSettings.PlayerModes[i] != PlayerMode.None)
                    activeCount++;
            }

            if (activeCount >= 2)
            {
                StartCoroutine(LoadCoroutine());
            }
            else
            {
                GameObject.Find("PopupUI").GetComponent<ModalUI>().Show("enable atleast two players");
            }
        }

        private IEnumerator LoadCoroutine()
        {
            Persistent.Get<AnalyticsRelay>().MenuLevelLoad();
            //done so sound can play
            yield return new WaitForSeconds(.5f);
            Persistent.Get<AdvertisementDispatch>().ShowAdvertisement();
            string levelName = "Level" + (Persistent.Get<MenuConfig>().LevelIndex + 1);
            SceneManager.LoadScene(levelName);
        }
    }
}