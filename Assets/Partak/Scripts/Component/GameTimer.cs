using UnityEngine;
using System.Collections;

namespace Partak
{
	public class GameTimer : MonoBehaviour
	{
		[SerializeField]
		private Material _surroundMaterial;

		[SerializeField]
		private float _timeLimit = 60f;

		private float _time;

		private bool _limitReached;

		private CellParticleMover _cellParticleMover;

		private CellParticleStore _cellParticleStore;

		[SerializeField]
		private Texture[] _playerColorTextures;

		private void Start()
		{
			_cellParticleMover = FindObjectOfType<CellParticleMover>();
			_cellParticleStore = FindObjectOfType<CellParticleStore>();
		}

		private void Update()
		{
			_time += Time.deltaTime;
			if (_time > _timeLimit && !_limitReached)
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

		private IEnumerator TimeoutCoroutine()
		{
			float blend = 0f;
			while (true)
			{
				blend += Time.deltaTime;
				_surroundMaterial.SetFloat("_Blend", Mathf.PingPong(blend, .5f));
				_surroundMaterial.SetTexture("_Texture2", _playerColorTextures[_cellParticleStore.WinningPlayer()]);
				yield return null;
			}
		}
	}
}