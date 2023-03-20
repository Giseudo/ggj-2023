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
        public static bool IsGameOver { get; private set; }

        private static List<WaveSpawner> _waveSpawners;
        private Damageable _treeDamageable;

        public static MatchManager Instance { get; private set; }
        public static int RoundNumbers { get; private set; }
        public static int CurrentRound { get; private set; }
        public static int CurrentScore { get; private set; }
        public static List<WaveSpawner> WaveSpawners => _waveSpawners;
        public static bool HasStarted { get; private set; }
        public static Action<WaveSpawner> WaveSpawnerAdded;
        public static Action<WaveSpawner> WaveSpawnerRemoved;
        public static Action<int, Vector3> DroppedEnergy;
        public static Action LevelCompleted;
        public static Action GameCompleted;
        public static Action GameOver;
        public static Action<int> RoundStarted;
        public static Action<int> ScoreChanged;

        public void Awake()
        {
            Instance = this;

            EndedWavesCount = 0;
            CurrentRound = 0;
            IsGameOver = false;
            WaveSpawnerAdded = delegate { };
            WaveSpawnerRemoved = delegate { };
            DroppedEnergy = delegate { };
            LevelCompleted = delegate { };
            GameCompleted = delegate { };
            RoundStarted = delegate { };
            ScoreChanged = delegate { };
            GameOver = delegate { };
            HasStarted = false;

            _waveSpawners = new List<WaveSpawner>();
        }

        public void Start()
        {
            GameManager.Scenes.loadedLevel += OnLoadLevel;
            OnLoadLevel(0);

            // LevelCompleted.Invoke();
        }

        public void OnDestroy()
        {
            GameManager.Scenes.loadedLevel -= OnLoadLevel;
        }

        private void OnLoadLevel(int level)
        {
            HasStarted = false;
            CurrentRound = 0;
            EndedWavesCount = 0;
            IsGameOver = false;

            if (_treeDamageable)
                _treeDamageable.died -= OnTreeDeath;

            if (!GameManager.MainTree.TryGetComponent<Damageable>(out _treeDamageable)) return;

            _treeDamageable.died += OnTreeDeath;
        }

        private void OnTreeDeath(Damageable damageable)
        {
            IsGameOver = true;
            GameOver.Invoke();
        }

        public static void StartRound()
        {
            HasStarted = true;
            RoundStarted.Invoke(CurrentRound);

            for (int i = 0; i < _waveSpawners.Count; i++)
            {
                WaveSpawner waveSpawner = _waveSpawners[i];

                waveSpawner.StartRound();
            }
        }

        public static void NextRound()
        {
            CurrentRound++;
            RoundStarted.Invoke(CurrentRound);

            for (int i = 0; i < _waveSpawners.Count; i++)
            {
                WaveSpawner waveSpawner = _waveSpawners[i];

                waveSpawner.NextRound(CurrentRound);
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
                IsGameOver = true;

                if (GameManager.Scenes.CurrentLevel >= GameManager.Scenes.LevelScenes.Count - 1)
                {
                    GameCompleted.Invoke();
                    return;
                }

                LevelCompleted.Invoke();
            }
        }

        private void OnRoundOver()
        { }

        public void OnCreepDeath(Creep creep)
        {
            if (creep == null) return;

            InternalDropEnergy(creep.EnergyDropAmount, creep.transform.position);
        }

        private void InternalDropEnergy(int amount, Vector3 position)
        {
            if (IsGameOver) return;

            DroppedEnergy.Invoke(amount, position);

            StartCoroutine(CollectEnergy(amount));
        }

        public static void DropEnergy(int amount, Vector3 position) => Instance.InternalDropEnergy(amount, position);

        private IEnumerator CollectEnergy(int amount)
        {
            yield return new WaitForSeconds(1.5f);

            GameManager.MainTree.CollectEnergy(amount);
        }

        public static void SetScore(int value)
        {
            CurrentScore = value;
            ScoreChanged.Invoke(CurrentScore);
        }

        public static void AddScore(int value)
        {
            CurrentScore += value;
            ScoreChanged.Invoke(CurrentScore);
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
// [x] SFX
// [x] Keyboard commands
// [x] Root limit
// [x] Tree upgrade
// [x] Unities upgrade
// [x] Seed hole improvements
// [x] Sell unit
// [x] Next level
// [x] Main menu
// [x] Bozo explodir
// [x] Wave timer
// [x] Day / cycle
// [x] Camera pan
// [x] Block unit creation by level
// [x] Spit & seed hole vfx
// [x] Spit audio
// [x] Improve trees look
// [x] Absorb tree indications
// [x] Area damage
// [x] Intro
// [ ] Score