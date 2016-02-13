//#define DISABLE_PLAYERPREF
using UnityEngine;

namespace Partak {
public class MenuConfig : MonoBehaviour {
	public const int MaxPlayers = 4;

	public readonly Color[] PlayerColors = new Color[MaxPlayers];

	public readonly PlayerMode[] PlayerModes = new PlayerMode[MaxPlayers];

	public readonly IPlayerInput[] PlayerInputs = new IPlayerInput[MaxPlayers];

	public int TimeLimitMinutes { get; set; }

	public int LevelIndex { get; set; }

	public bool PlayerActive(int playerIndex) {
		if (PlayerModes[playerIndex] != PlayerMode.None) {
			return true;
		} else {
			return false;
		}
	}

	public int ActivePlayerCount() {
		int count = 0;
		for (int i = 0; i < PlayerModes.Length; ++i) {
			if (PlayerModes[i] != PlayerMode.None) {
				count++;
			}
		}
		return count;
	}
        
	public int ActiveHumanPlayerCount() {
		int count = 0;
		for (int i = 0; i < PlayerModes.Length; ++i) {
			if (PlayerModes[i] == PlayerMode.Human) {
				count++;
			}
		}
		return count;
	}
}
}