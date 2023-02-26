using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Core;
using DG.Tweening;
using Game.Combat;

namespace Game.Core
{
    public class MatchManager : MonoBehaviour
    {
        public static int EndedWavesCount { get; private set; }
        public static int RoundOverCount { get; private set; }

        private static List<WaveSpawner> _waveSpawners;

        public static MatchManager Instance { get; private set; }
        public static int RoundNumbers { get; private set; }
        public static List<WaveSpawner> WaveSpawners => _waveSpawners;
        public static bool HasStarted { get; private set; }

        public static Action<WaveSpawner> WaveSpawnerAdded;
        public static Action<WaveSpawner> WaveSpawnerRemoved;
        public static Action<int, Vector3> DroppedEnergy;
        public static Action LevelCompleted;
        public static Action<int> RoundStarted;

        public void Awake()
        {
            Instance = this;

            EndedWavesCount = 0;
            RoundOverCount = 0;
            WaveSpawnerAdded = delegate { };
            WaveSpawnerRemoved = delegate { };
            DroppedEnergy = delegate { };
            LevelCompleted = delegate { };
            RoundStarted = delegate { };
            HasStarted = false;

            _waveSpawners = new List<WaveSpawner>();
        }

        public void Start()
        {
            GameManager.Scenes.loadedLevel += OnLoadLevel;
        }

        public void OnDestroy()
        {
            GameManager.Scenes.loadedLevel -= OnLoadLevel;
        }

        private void OnLoadLevel(int level)
        { }

        public static void StartRound()
        {
            HasStarted = true;
            RoundStarted.Invoke(RoundOverCount);

            for (int i = 0; i < _waveSpawners.Count; i++)
            {
                WaveSpawner waveSpawner = _waveSpawners[i];

                waveSpawner.StartRound();
            }
        }

        public static void NextRound()
        {
            RoundStarted.Invoke(RoundOverCount + 1);

            // TODO mark next round as started?

            for (int i = 0; i < _waveSpawners.Count; i++)
            {
                WaveSpawner waveSpawner = _waveSpawners[i];

                waveSpawner.NextRound();
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
            EndedWavesCount++;

            if (EndedWavesCount >= _waveSpawners.Count)
            {
                EndedWavesCount = 0;
                LevelCompleted.Invoke();
            }
        }

        private void OnRoundOver()
        {
            RoundOverCount++;

            if (RoundOverCount >= _waveSpawners.Count)
            {
                RoundOverCount = 0;

                // TODO start next round if it's not started yet
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
// [x] Main menu
// [x] Bozo explodir
// [x] Absorb tree indications / transition
// [ ] Wave timer
// [ ] Day / cycle
// [x] Block unit creation by level
// [ ] Score
// [x] SFX