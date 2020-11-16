using System.Collections;
using System.Threading.Tasks;
using GeoTetra.GTPooling;
using UnityEngine;
using UnityEngine.Audio;

namespace GeoTetra.Partak
{
    public class GameMusic : MonoBehaviour
    {
        [SerializeField] private GameStateRef _gameState;
        [SerializeField] private AudioMixerGroup _audioMixerGroup;
        [SerializeField] private AudioSource _winAudioSource;
        [SerializeField] private LevelConfig _levelConfig;
        [SerializeField] private CellParticleStore _cellParticleStore;
        private AudioSource[,] _audioSource;
        private AudioClip[,] _music;
        private int[] _particleInterval;
        private int[] _playingClip;
        private bool _playMusic;
        
        public async Task Initialize()
        {
            await _gameState.Cache();
            
            _music = new AudioClip[_gameState.Service.PlayerCount(), 3];
            _audioSource = new AudioSource[_gameState.Service.PlayerCount(), 3];
            _playingClip = new int[_gameState.Service.PlayerCount()];
            int rand;
            for (int i = 0; i < _gameState.Service.PlayerCount(); i++)
            {
                rand = Random.Range(0, 4);
                for (int o = 0; o < 3; o++)
                {
                    _music[i, o] = (AudioClip) Resources.Load("Music/" + rand + "-" + i + "-" + o);
                    _audioSource[i, o] = gameObject.AddComponent<AudioSource>();
                    _audioSource[i, o].clip = _music[i, o];
                    _audioSource[i, o].outputAudioMixerGroup = _audioMixerGroup;
                    _audioSource[i, o].playOnAwake = false;
                    _audioSource[i, o].reverbZoneMix = 0f;
                    _audioSource[i, o].Play();
                    _audioSource[i, o].volume = 0;
                    _audioSource[i, o].loop = true;
                }

                _playingClip[i] = -1;
            }
            
            _particleInterval = new int[3];
            _particleInterval[0] = _levelConfig.Datum.ParticleCount - _levelConfig.Datum.ParticleCount / 3;
            _particleInterval[1] = _levelConfig.Datum.ParticleCount - _levelConfig.Datum.ParticleCount / 8;
            _particleInterval[2] = _levelConfig.Datum.ParticleCount;
            _playMusic = true;
            _cellParticleStore.WinEvent += () =>
            {
                StartCoroutine(MuteAll());
                _winAudioSource.Play();
            };
        }

        private void Update()
        {
            if (_playMusic)
            {
                for (int i = 0; i < _gameState.Service.PlayerCount(); i++)
                    if (_cellParticleStore.PlayerParticleCount[i] == -100)
                    {
                        _audioSource[i, 0].mute = true;
                        _audioSource[i, 1].mute = true;
                        _audioSource[i, 2].mute = true;
                    }
                    else if (_playingClip[i] != 0 &&
                             _cellParticleStore.PlayerParticleCount[i] > 0 &&
                             _cellParticleStore.PlayerParticleCount[i] < _particleInterval[0])
                    {
                        _audioSource[i, 0].volume = .5f;
                        _audioSource[i, 0].mute = false;
                        _audioSource[i, 1].mute = true;
                        _audioSource[i, 2].mute = true;
                        _playingClip[i] = 0;
                    }
                    else if (_playingClip[i] != 1 &&
                             _cellParticleStore.PlayerParticleCount[i] > _particleInterval[0] &&
                             _cellParticleStore.PlayerParticleCount[i] < _particleInterval[1])
                    {
                        _audioSource[i, 0].mute = true;
                        _audioSource[i, 1].volume = .4f;
                        _audioSource[i, 1].mute = false;
                        _audioSource[i, 2].mute = true;
                        _playingClip[i] = 1;
                    }
                    else if (_playingClip[i] != 2 &&
                             _cellParticleStore.PlayerParticleCount[i] > _particleInterval[1] &&
                             _cellParticleStore.PlayerParticleCount[i] < _particleInterval[2])
                    {
                        _audioSource[i, 0].mute = true;
                        _audioSource[i, 1].mute = true;
                        _audioSource[i, 2].volume = .4f;
                        _audioSource[i, 2].mute = false;
                        _playingClip[i] = 2;
                    }
            }
        }

        public IEnumerator MuteAll()
        {
            _playMusic = false;
            while (true)
            {
                for (int i = 0; i < _gameState.Service.PlayerCount(); i++)
                for (int o = 0; o < 3; o++)
                    _audioSource[i, o].volume -= Time.deltaTime / 4f;

                yield return null;
            }
        }
    }
}