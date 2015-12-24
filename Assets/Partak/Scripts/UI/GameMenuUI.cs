using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Advertisements;

namespace Partak.UI
{
	public class GameMenuUI : MonoBehaviour
	{
		[SerializeField]
		public Button _replayButton;

		[SerializeField]
		public Button _mainButton;

		[SerializeField]
		public Button _nextButton;

		[SerializeField]
		public Button _skipButton;

		[SerializeField]
		public Button _resumeButton;

		[SerializeField]
		public Button[] _pauseButtons;

		private void Awake()
		{
			_pauseButtons[0].onClick.AddListener(ShowPauseMenu);
			_pauseButtons[1].onClick.AddListener(ShowPauseMenu);

			_resumeButton.onClick.AddListener(Resume);
			_replayButton.onClick.AddListener(Replay);
			_nextButton.onClick.AddListener(Next);
			_mainButton.onClick.AddListener(MainMenu);
			_skipButton.onClick.AddListener(Skip);

			FindObjectOfType<CellParticleStore>().WinEvent += ShowWinMenu;
		}

		private void ShowPauseMenu()
		{
			GetComponent<Animator>().Play("SlideIn");
			_pauseButtons[0].interactable = false;
			_pauseButtons[1].interactable = false;
			_replayButton.gameObject.SetActive(false);
			_mainButton.gameObject.SetActive(true);
			_nextButton.gameObject.SetActive(false);
			_skipButton.gameObject.SetActive(true);
			_resumeButton.gameObject.SetActive(true);
			FindObjectOfType<CellParticleMover>().Pause = true;
		}

		public void ShowWinMenu()
		{
			GetComponent<Animator>().Play("SlideIn");
			_pauseButtons[0].gameObject.SetActive(false);
			_pauseButtons[1].gameObject.SetActive(false);
			_replayButton.gameObject.SetActive(true);
			_mainButton.gameObject.SetActive(true);
			_nextButton.gameObject.SetActive(true);
			_skipButton.gameObject.SetActive(false);
			_resumeButton.gameObject.SetActive(false);
		}

		private void Resume()
		{
			_pauseButtons[0].interactable = true;
			_pauseButtons[1].interactable = true;
			GetComponent<Animator>().Play("SlideOut");
			FindObjectOfType<CellParticleMover>().Pause = false;
		}

		private void MainMenu()
		{
			StartCoroutine(LoadCoroutine("OpenMenu"));
		}

		private void Replay()
		{
			string levelName = "Level" + (Persistent.Get<PlayerSettings>().LevelIndex + 1);
			Debug.Log("Replaying " + levelName);
			Persistent.Get<AnalyticsRelay>().ReplayLevel();
			StartCoroutine(LoadCoroutine("Level" + (Persistent.Get<PlayerSettings>().LevelIndex + 1)));
		}

		private void Skip()
		{
			Persistent.Get<PlayerSettings>().LevelIndex++;
			string levelName = "Level" + (Persistent.Get<PlayerSettings>().LevelIndex + 1);
			Debug.Log("Skipping " + levelName);
			Persistent.Get<AnalyticsRelay>().SkipLevel();
			StartCoroutine(LoadCoroutine(levelName));
		}

		private void Next()
		{
			Persistent.Get<AdvertisementDispatch>().ShowAdvertisement();
			Persistent.Get<PlayerSettings>().LevelIndex++;
			StartCoroutine(LoadCoroutine("Level" + (Persistent.Get<PlayerSettings>().LevelIndex + 1)));
		}

		private IEnumerator LoadCoroutine(string levelName)
		{
			GetComponent<Animator>().Play("SlideOut");
			Prime31.EtceteraBinding.showActivityView();
			//done so sound can play
			yield return new WaitForSeconds(.5f);
			SceneManager.LoadScene(levelName);
		}
	}
}