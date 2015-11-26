using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

			FindObjectOfType<CellParticleStore>().WinEvent += ShowWinMenu;
		}

		private void ShowPauseMenu()
		{
			GetComponent<Animator>().Play("SlideIn");
			_pauseButtons[0].gameObject.SetActive(false);
			_pauseButtons[1].gameObject.SetActive(false);
			_replayButton.gameObject.SetActive(false);
			_mainButton.gameObject.SetActive(true);
			_nextButton.gameObject.SetActive(true);
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
			_resumeButton.gameObject.SetActive(false);
		}

		private void Resume()
		{
			_pauseButtons[0].gameObject.SetActive(true);
			_pauseButtons[1].gameObject.SetActive(true);
			GetComponent<Animator>().Play("SlideOut");
			FindObjectOfType<CellParticleMover>().Pause = false;
		}

		private void MainMenu()
		{
			Prime31.EtceteraBinding.showActivityView();
			Application.LoadLevel("OpenMenu");
		}

		private void Replay()
		{
			Prime31.EtceteraBinding.showActivityView();
			Application.LoadLevel("Level" + (Persistent.Get<PlayerSettings>().LevelIndex + 1));
		}

		private void Next()
		{
			Prime31.EtceteraBinding.showActivityView();
			Persistent.Get<PlayerSettings>().LevelIndex++;
			Application.LoadLevel("Level" + (Persistent.Get<PlayerSettings>().LevelIndex + 1));
		}
	}
}