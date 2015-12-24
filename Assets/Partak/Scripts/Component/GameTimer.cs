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

		private float _fastKillTimeLimit = 60f;

		private float _timeLimit = 75f;

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

			FindObjectOfType<CellParticleStore>().WinEvent += Win;

			Invoke("FastKillTimeOut", _fastKillTimeLimit);
			Invoke("TimeOut", _timeLimit);
		}

		private void Update()
		{
			GameTime += Time.deltaTime;
		}

		private void OnDestroy()
		{
			_surroundMaterial.SetFloat("_Blend", 0f);
			_surroundMaterial.SetTexture("_Texture2", null);
		}

		private void Win()
		{
			Persistent.Get<AnalyticsRelay>().GameTime(GameTime);
		}

		private void TimeOut()
		{
			_cellParticleStore.Win();
		}

		private void FastKillTimeOut()
		{
			_cellParticleMover.FastKill = true;
			StartCoroutine(FastKillTimeoutCoroutine());
		}

		//this should be in it's own object
		private IEnumerator FastKillTimeoutCoroutine()
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