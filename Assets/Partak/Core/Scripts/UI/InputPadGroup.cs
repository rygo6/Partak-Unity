using GeoTetra.GTPooling;
using UnityEngine;

namespace GeoTetra.Partak.UI
{
    public class InputPadGroup : MonoBehaviour
    {
        [SerializeField] private ComponentContainerReference _componentContainer;
        [SerializeField] private GameStateRef _gameState;
        [SerializeField] private InputPad[] _inputPads;
        [SerializeField] private GameObject _horizontalTop;
        [SerializeField] private GameObject _horizontalBottom ;
        
        public async void Initialize()
        {
            await _gameState.Cache();
            
            for (int i = 0; i < _inputPads.Length; ++i)
            {
                _inputPads[i].Visibility(true);
            }
            
            for (int i = 0; i < _inputPads.Length; ++i)
            {
                if (_gameState.Service.PlayerStates[i].PlayerMode != PlayerMode.Human)
                {
                    _inputPads[i].gameObject.SetActive(false);
                }
                else
                {
                    _inputPads[i].gameObject.SetActive(true);
                    _inputPads[i].Initialize();
                }
            }

            if (!_inputPads[0].gameObject.activeSelf && !_inputPads[1].gameObject.activeSelf)
            {
                _horizontalTop.SetActive(false);
            }
            else
            {
                _horizontalTop.SetActive(true);
            }

            if (!_inputPads[2].gameObject.activeSelf && !_inputPads[3].gameObject.activeSelf)
            {
                _horizontalBottom.SetActive(false);
            }
            else
            {
                _horizontalBottom.SetActive(true);
            }

            _componentContainer.Service.Get<CellParticleStore>().LoseEvent += DisablePad;
            _componentContainer.Service.Get<CellParticleStore>().WinEvent += DisableAllPads;
        }

        public void Deinitialize()
        {
            _componentContainer.Service.Get<CellParticleStore>().LoseEvent -= DisablePad;
            _componentContainer.Service.Get<CellParticleStore>().WinEvent -= DisableAllPads;
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
            _inputPads[playerIndex].Visibility(false);
        }
    }
}