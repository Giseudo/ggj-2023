using System;
using System.Collections.Generic;
using UnityEngine;

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
        }

        public static void RemoveWaveSpawner(WaveSpawner waveSpawner)
        {
            _waveSpawners.Remove(waveSpawner);
            WaveSpawnerRemoved.Invoke(waveSpawner);

            waveSpawner.finished -= Instance.OnWaveFinish;
        }

        private void OnWaveFinish()
        {
            _endedWavesCount++;

            if (_endedWavesCount >= _waveSpawners.Count)
                LevelCompleted.Invoke();
        }
    }
}

// TODO
// [x] Wave system
// [ ] Build github
// [ ] Blurp
// [ ] Seed hole
// [ ] Unit selection description
// [ ] Unities upgrade
// [ ] Energy system
// [ ] Pause / play / fast forward
// [ ] Root split obstacle
// [ ] Sell unit
// [ ] Wave timer
// [ ] Absorb tree
// [ ] SFX
// [ ] Main menu
// [ ] Build & upload