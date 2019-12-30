using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using GeoTetra.Partak;
using UnityEngine;

namespace GeoTetra.Partak
{
    /// <summary>
    /// Controls initializing of all game systems.
    /// </summary>
    public class GameSessionSequencer : MonoBehaviour
    {
        [SerializeField] private LevelConfig _levelConfig;
        
        [SerializeField] private CellHiearchy _cellHiearchy;
        [SerializeField] private CellGradient _cellGradient;
        [SerializeField] private CellParticleDisplay _cellParticleDisplay;
        [SerializeField] private CellParticleEngine _cellParticleEngine;
        [SerializeField] private CellParticleSpawn _cellParticleSpawn;
        [SerializeField] private CellAI _cellAI;
        [SerializeField] private CellParticleStore _cellParticleStore;
        [SerializeField] private GameMusic _gameMusic;

        private void Awake()
        {
            _levelConfig.LevelDeserialized += () =>
            {
                Debug.Log("Starting GameSessionSequencer");
                StartCoroutine(Initialize());
            };
        }

        public IEnumerator Initialize()
        {
            yield return null;
            _cellHiearchy.Initialize();
            _cellGradient.Initialize();
            _cellParticleDisplay.Initialize();
            _cellParticleStore.Initialize();
            yield return StartCoroutine(_cellParticleSpawn.Initialize());
            _cellParticleEngine.Initialize();
            _cellAI.Initialize();
            _gameMusic.Initialize();
        }

        [ContextMenu("DebugInitialize")]
        public void DebugInitialize()
        {
            StartCoroutine(Initialize());
        }
    }
}