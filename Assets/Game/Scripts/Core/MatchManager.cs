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
        private int _endedWavesCount = 0;
        private int _roundOverCount = 0;

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

            WaveSpawnerAdded = delegate { };
            WaveSpawnerRemoved = delegate { };
            DroppedEnergy = delegate { };
            LevelCompleted = delegate { };

            _waveSpawners = new List<WaveSpawner>();
        }

        public void Start()
        {
            StartCoroutine(StartRound());

            GameManager.Scenes.loadedLevel += OnLoadLevel;
        }

        public void OnDestroy()
        {
            GameManager.Scenes.loadedLevel -= OnLoadLevel;
        }

        private void OnLoadLevel(int level)
        {
            StartCoroutine(StartRound());
        }

        public IEnumerator StartRound()
        {
            yield return new WaitForSeconds(1f);

            for (int i = 0; i < _waveSpawners.Count; i++)
            {
                WaveSpawner waveSpawner = _waveSpawners[i];

                waveSpawner.StartRound();
            }
        }

        public static void AddWaveSpawner(WaveSpawner waveSpawner)
        {
            _waveSpawners.Add(waveSpawner);
            WaveSpawnerAdded.Invoke(waveSpawner);

            waveSpawner.finished += Instance.OnWaveFinish;
            waveSpawner.creepDied += Instance.OnCreepDeath;
            waveSpawner.roundOver += Instance.OnRoundOver;
        }

        public static void RemoveWaveSpawner(WaveSpawner waveSpawner)
        {
            _waveSpawners.Remove(waveSpawner);
            WaveSpawnerRemoved.Invoke(waveSpawner);

            waveSpawner.finished -= Instance.OnWaveFinish;
            waveSpawner.creepDied -= Instance.OnCreepDeath;
            waveSpawner.roundOver -= Instance.OnRoundOver;
        }

        private void OnWaveFinish()
        {
            _endedWavesCount++;

            if (_endedWavesCount >= _waveSpawners.Count)
                LevelCompleted.Invoke();
        }

        private void OnRoundOver()
        {
            _roundOverCount++;

            if (_roundOverCount >= _waveSpawners.Count)
            {
                _roundOverCount = 0;

                StartCoroutine(StartRound());
            }
        }

        public void OnCreepDeath(Creep creep)
        {
            if (creep == null) return;

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
// [x] Absorb tree
// [x] Pause / play / fast forward
// [x] Keyboard commands
// [x] Root limit
// [x] Tree upgrade
// [x] Unities upgrade
// [x] Seed hole improvements
// [x] Sell unit
// [x] Next level
// [ ] Main menu
// [ ] Wave timer
// [ ] Day / cycle
// [ ] Block unit creation by level
// [ ] Absorb tree indications / transition
// [ ] Score
// [x] SFX