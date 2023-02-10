using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Core;
using DG.Tweening;

namespace Game.Combat
{
    public class MatchManager : MonoBehaviour
    {
        [SerializeField]
        private int _roundNumbers = 3;

        private int _endedWavesCount = 0;

        private static List<WaveSpawner> _waveSpawners;

        public static MatchManager Instance { get; private set; }
        public static int RoundNumbers { get; private set; }
        public static List<WaveSpawner> WaveSpawners => _waveSpawners;

        public static Action<WaveSpawner> WaveSpawnerAdded;
        public static Action<WaveSpawner> WaveSpawnerRemoved;
        public static Action<int, Vector3> DroppedEnergy;
        public static Action LevelCompleted;

        public void Awake()
        {
            Instance = this;
            RoundNumbers = _roundNumbers;

            WaveSpawnerAdded = delegate { };
            WaveSpawnerRemoved = delegate { };
            DroppedEnergy = delegate { };
            LevelCompleted = delegate { };

            _waveSpawners = new List<WaveSpawner>();
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
            InternalDropEnergy(creep.EnergyDropAmount, creep.transform.position);
        }

        private void InternalDropEnergy(int amount, Vector3 position)
        {
            DroppedEnergy.Invoke(amount, position);

            StartCoroutine(CollectEnergy(amount));
        }

        public static void DropEnergy(int amount, Vector3 position) => Instance.InternalDropEnergy(amount, position);

        private IEnumerator CollectEnergy(int amount)
        {
            yield return new WaitForSeconds(1.5f);

            GameManager.MainTree.CollectEnergy(amount);
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
// [x] Root split obstacle
// [ ] Absorb tree
// [ ] Unities upgrade
// [ ] Pause / play / fast forward
// [ ] Sell unit
// [ ] Wave timer
// [ ] Unit construction time
// [ ] SFX
// [ ] Main menu
// [ ] Build & upload