using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
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
            _levelConfig.LevelDeserialized += async () =>
            {
                Debug.Log("Starting GameSessionSequencer");
                await Initialize();
            };
        }

        private async Task Initialize()
        {
            await _cellHiearchy.Initialize();
            await _cellGradient.Initialize(true);
            await _cellParticleDisplay.Initialize();
            await _cellParticleStore.Initialize();
            await _cellParticleSpawn.Initialize();
            await _cellParticleEngine.Initialize(true);
            await _cellAI.Initialize();
            await _gameMusic.Initialize();
        }

        [ContextMenu("DebugInitialize")]
        public void DebugInitialize()
        {
            // StartCoroutine(Initialize());
        }
    }
}