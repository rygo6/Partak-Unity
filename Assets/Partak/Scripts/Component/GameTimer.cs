using UnityEngine;
using System.Collections;
using UnityEngine.Analytics;
using System.Collections.Generic;

namespace Partak
{
	public class GameTimer : MonoBehaviour
	{
		[SerializeField]
		private Material _surroundMaterial;

		[SerializeField]
		private float _timeLimit = 60f;

		public float GameTime { get; private set; }

		private bool _limitReached;

		private CellParticleMover _cellParticleMover;

		private CellParticleStore _cellParticleStore;

		[SerializeField]
		private Texture[] _playerColorTextures;

		private void Start()
		{
			_cellParticleMover = FindObjectOfType<CellParticleMover>();
			_cellParticleStore = FindObjectOfType<CellParticleStore>();

			FindObjectOfType<CellParticleStore>().WinEvent += LogGameTime;
		}

		private void Update()
		{
			GameTime += Time.deltaTime;
			if (GameTime > _timeLimit && !_limitReached)
			{
				_limitReached = true;
				_cellParticleMover.Timeout = true;
				StartCoroutine(TimeoutCoroutine());
			}
		}

		private void OnDestroy()
		{
			_surroundMaterial.SetFloat("_Blend", 0f);
			_surroundMaterial.SetTexture("_Texture2", null);
		}

		private void LogGameTime()
		{
			string levelName = "Level" + (Persistent.Get<PlayerSettings>().LevelIndex + 1);
			Debug.Log("Time spent on " + levelName + " " + GameTime);
			Analytics.CustomEvent("LevelTime", new Dictionary<string, object>
			{
				{"LevelName", levelName},
				{"Time", GameTime},
			});
		}

		private IEnumerator TimeoutCoroutine()
		{
			float blend = 0f;
			int winningPlayer = 0;
			int newWinningPlayer = 0;
			_surroundMaterial.SetTexture("_Texture2", _playerColorTextures[winningPlayer]);
			while (true)
			{
				blend += Time.deltaTime;
				_surroundMaterial.SetFloat("_Blend", Mathf.PingPong(blend, .5f));
				newWinningPlayer = _cellParticleStore.WinningPlayer();
				if (winningPlayer != newWinningPlayer)
				{
					winningPlayer = newWinningPlayer;
					_surroundMaterial.SetTexture("_Texture2", _playerColorTextures[winningPlayer]);
				}
				yield return null;
			}
		}
	}
}