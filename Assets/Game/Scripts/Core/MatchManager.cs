using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Combat
{
    public class MatchManager : MonoBehaviour
    {
        [SerializeField]
        private int _roundNumbers = 3;

        private int _endedWavesCount = 0;

        public static MatchManager Instance { get; private set; }
        public static int RoundNumbers { get; private set; }
        public static Action<WaveSpawner> WaveSpawnerAdded = delegate { };
        public static Action<WaveSpawner> WaveSpawnerRemoved = delegate { };
        private static List<WaveSpawner> _waveSpawners = new List<WaveSpawner>();
        public static Action LevelCompleted = delegate { };

        public void Awake()
        {
            Instance = this;
            RoundNumbers = _roundNumbers;
        }

        public static void AddWaveSpawner(WaveSpawner waveSpawner)
        {
            _waveSpawners.Add(waveSpawner);
            WaveSpawnerAdded.Invoke(waveSpawner);

            waveSpawner.finished += Instance.OnWaveFinish;
            waveSpawner.creepDied += Instance.OnCreepDeath;
        }

        public static void RemoveWaveSpawner(WaveSpawner waveSpawner)
        {
            _waveSpawners.Remove(waveSpawner);
            WaveSpawnerRemoved.Invoke(waveSpawner);

            waveSpawner.finished -= Instance.OnWaveFinish;
            waveSpawner.creepDied -= Instance.OnCreepDeath;
        }

        private void OnWaveFinish()
        {
            _endedWavesCount++;

            if (_endedWavesCount >= _waveSpawners.Count)
                LevelCompleted.Invoke();
        }

        public void OnCreepDeath(Creep creep)
        {
            GameManager.MainTree.CollectEnergy(creep.EnergyDropAmount, creep.transform.position);
        }
    }
}

// TODO
// [x] Wave system
// [x] Build github
// [x] Blurp
// [x] Seed hole
// [x] Energy system
// [x] Unit selection
// [ ] Root split obstacle & snap
// [ ] Absorb tree
// [ ] Unities upgrade
// [ ] Pause / play / fast forward
// [ ] Sell unit
// [ ] Wave timer
// [ ] Unit construction time
// [ ] SFX
// [ ] Main menu
// [ ] Build & upload