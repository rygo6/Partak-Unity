using UnityEngine;
using System.Collections;
using UnityEngine.Analytics;
using System.Collections.Generic;

namespace Partak {
public class GameClock : MonoBehaviour {

	public float GameTime { get; private set; }
	[SerializeField] Texture[] _playerColorTextures;
	[SerializeField] Material _surroundMaterial;
	float _fastKillTimeLimit = 70f;
	float _timeLimit = 80f;
	bool _limitReached;
	CellParticleEngine _cellParticleMover;
	CellParticleStore _cellParticleStore;
	Color[] _initialColors = new Color[4];
	MenuConfig _menuConfig;

	void Start() {
		_cellParticleMover = FindObjectOfType<CellParticleEngine>();
		_cellParticleStore = FindObjectOfType<CellParticleStore>();
		_menuConfig = Persistent.Get<MenuConfig>();
		_menuConfig.PlayerColors.CopyTo(_initialColors, 0);
		SetTimeLimit(Persistent.Get<MenuConfig>().TimeLimitMinutes);
		FindObjectOfType<CellParticleStore>().WinEvent += Win;
		Invoke("FastKillTimeOut", _fastKillTimeLimit);
		Invoke("TimeOut", _timeLimit);
	}

	void Update() {
		GameTime += Time.deltaTime;
	}

	void OnDestroy() {
		_surroundMaterial.SetFloat("_Blend", 0f);
		_surroundMaterial.SetTexture("_Texture2", null);
		for (int i = 0; i < _menuConfig.PlayerColors.Length; ++i)
			_menuConfig.PlayerColors[i] = _initialColors[i];
	}

	public void SetTimeLimit(int minutes) {
		if (minutes == 0)
			minutes = 1;
		_fastKillTimeLimit = minutes * 60;
		;
		_timeLimit = (minutes * 20) + _fastKillTimeLimit;
	}

	void Win() {
		Persistent.Get<AnalyticsRelay>().GameTime(GameTime);
	}

	void TimeOut() {
		_cellParticleStore.Win();
	}

	void FastKillTimeOut() {
		_cellParticleMover.FastKill = true;
		StartCoroutine(FastKillTimeoutCoroutine());
	}

	IEnumerator FastKillTimeoutCoroutine() {
		int winningPlayer = 0;
		while (true) {
			winningPlayer = _cellParticleStore.WinningPlayer();
			_menuConfig.PlayerColors[winningPlayer] += new Color(.3f, .3f, .3f, .3f);
			yield return null;
			yield return null;
			_menuConfig.PlayerColors[winningPlayer] = _initialColors[winningPlayer];
			yield return new WaitForSeconds(0.4f);
		}
	}
}
}