using GeoTetra.GTCommon.Components;
using GeoTetra.GTPooling;
using UnityEngine;

namespace GeoTetra.Partak.UI
{
    public class InputPadGroup : SubscribableBehaviour
    {
        [SerializeField] private ComponentContainerRef _componentContainer;
        [SerializeField] private PartakStateRef _partakState;
        [SerializeField] private InputPad[] _inputPads;
        [SerializeField] private GameObject _horizontalTop;
        [SerializeField] private GameObject _horizontalBottom ;

        private CellParticleStore _cellParticleStore;
        
        public async void Initialize()
        {
            await _partakState.Cache(this);
            await _componentContainer.Cache(this);
            _cellParticleStore = await _componentContainer.AwaitRegister<CellParticleStore>();
            _cellParticleStore.LoseEvent += DisablePad;
            _cellParticleStore.WinEvent += DisableAllPads;
            
            for (int i = 0; i < _inputPads.Length; ++i)
            {
                _inputPads[i].Visibility(true);
            }
            
            for (int i = 0; i < _inputPads.Length; ++i)
            {
                if (_partakState.Ref.PlayerStates[i].PlayerMode != PlayerMode.Human)
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
        }

        public void Deinitialize()
        {
            _cellParticleStore.LoseEvent -= DisablePad;
            _cellParticleStore.WinEvent -= DisableAllPads;
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