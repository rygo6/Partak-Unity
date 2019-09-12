using GeoTetra.GTCommon.ScriptableObjects;
using UnityEngine;

namespace Partak.UI
{
    public class InputPadGroup : MonoBehaviour
    {
        [SerializeField] private GameState _gameState;
        [SerializeField] private InputPad[] _inputPads;
        [SerializeField] private GameObject _horizontalTop;
        [SerializeField] private GameObject _horizontalBottom;

        private void Start()
        {
            for (int i = 0; i < _inputPads.Length; ++i)
            {
                if (_gameState.PlayerStates[i].PlayerMode != PlayerMode.Human)
                {
                    _inputPads[i].gameObject.SetActive(false);
                }
            }

            if (!_inputPads[0].gameObject.activeSelf && !_inputPads[1].gameObject.activeSelf)
            {
                _horizontalTop.SetActive(false);
            }

            if (!_inputPads[2].gameObject.activeSelf && !_inputPads[3].gameObject.activeSelf)
            {
                _horizontalBottom.SetActive(false);
            }

            FindObjectOfType<CellParticleStore>().LoseEvent += DisablePad;
            FindObjectOfType<CellParticleStore>().WinEvent += DisableAllPads;
        }

        private void DisableAllPads()
        {
            for (int i = 0; i < _inputPads.Length; ++i)
            {
                DisablePad(i);
            }
        }

        private void DisablePad(int playerIndex)
        {
            _inputPads[playerIndex].Disable();
        }
    }
}