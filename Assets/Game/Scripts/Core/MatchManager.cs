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
        [SerializeField]
        private LeaderboardData _leaderboard;

        private static List<WaveSpawner> _waveSpawners;
        private Damageable _treeDamageable;

        public static MatchManager Instance { get; private set; }
        public static int EndedWavesCount { get; private set; }
        public static int RoundOverCount { get; private set; }
        public static bool IsGameOver { get; private set; }
        public static bool IsTreeDead { get; private set; }
        public static int RoundNumbers { get; private set; }
        public static int CurrentRound { get; private set; }
        public static int CurrentScore { get; private set; }
        public static bool HasStarted { get; private set; }
        public static LeaderboardData Leaderboard { get; private set; }
        public static List<WaveSpawner> WaveSpawners => _waveSpawners;
        public static Action<WaveSpawner> WaveSpawnerAdded;
        public static Action<WaveSpawner> WaveSpawnerRemoved;
        public static Action<int, Vector3> DroppedEnergy;
        public static Action LevelCompleted;
        public static Action GameCompleted;
        public static Action GameOver;
        public static Action DrainScoreHealth;
        public static Action DrainScoreEnergy;
        public static Action ScoreFinished;
        public static Action<int> RoundStarted;
        public static Action<int> ScoreChanged;
        public static Action<int> NewHighScore;

        private void OnLevelComplete() => StartCoroutine(AbsorbHealth());

        public void Awake()
        {
            Instance = this;

            Leaderboard = _leaderboard;
            EndedWavesCount = 0;
            CurrentRound = 0;
            CurrentScore = 0;
            IsGameOver = false;
            IsTreeDead = false;
            HasStarted = false;
            WaveSpawnerAdded = delegate { };
            WaveSpawnerRemoved = delegate { };
            DroppedEnergy = delegate { };
            LevelCompleted = delegate { };
            RoundStarted = delegate { };
            ScoreChanged = delegate { };
            DrainScoreHealth = delegate { };
            DrainScoreEnergy = delegate { };
            ScoreFinished = delegate { };
            NewHighScore = delegate { };
            GameOver = delegate { };

            _waveSpawners = new List<WaveSpawner>();
        }

        public void Start()
        {
            GameManager.Scenes.loadedLevel += OnLoadLevel;
            LevelCompleted += OnLevelComplete;
            OnLoadLevel(0);

            LevelCompleted.Invoke();
            // GameCompleted.Invoke();

            // if (GameManager.MainTree.TryGetComponent<Damageable>(out Damageable damageable))
            //     damageable.Die();
        }

        public void OnDestroy()
        {
            GameManager.Scenes.loadedLevel -= OnLoadLevel;
            LevelCompleted -= OnLevelComplete;
        }

        private void OnLoadLevel(int level)
        {
            HasStarted = false;
            CurrentRound = 0;
            EndedWavesCount = 0;
            IsTreeDead = false;
            IsGameOver = false;

            if (_treeDamageable)
                _treeDamageable.died -= OnTreeDeath;
            
            if (!GameManager.MainTree) return;
            if (!GameManager.MainTree.TryGetComponent<Damageable>(out _treeDamageable)) return;

            _treeDamageable.died += OnTreeDeath;
        }

        private void OnTreeDeath(Damageable damageable)
        {
            IsTreeDead = true;
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

            if (IsGameOver) return;

            if (EndedWavesCount >= _waveSpawners.Count)
            {
                IsGameOver = true;

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

        private IEnumerator AbsorbHealth()
        {
            if (!GameManager.MainTree.TryGetComponent<Damageable>(out Damageable damageable)) yield return null;

            yield return new WaitForSeconds(1f);

            while (damageable.Health > 0)
            {
                damageable.SetHealth(damageable.Health - 1);

                MatchManager.AddScore(10000);
                DrainScoreHealth.Invoke();

                yield return new WaitForSeconds(.25f);
            }

            StartCoroutine(AbsorbEnergy());
        }

        private IEnumerator AbsorbEnergy()
        {
            while (GameManager.MainTree.EnergyAmount > 0)
            {
                int score = 10000;

                MatchManager.AddScore(score * 10);
                DrainScoreEnergy.Invoke();

                GameManager.MainTree.SetEnergy(GameManager.MainTree.EnergyAmount - score);

                yield return new WaitForSeconds(.25f);
            }

            ScoreFinished.Invoke();
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
        
        public static void EndGame()
        {
            GameCompleted.Invoke();

            for (int i = 0; i < Leaderboard.Positions.Count; i++)
            {
                LeaderboardPosition position = Leaderboard.GetPosition(i);

                if (CurrentScore >= position.Score)
                {
                    Leaderboard.AddScore(i, CurrentScore);

                    if (i < 5) NewHighScore.Invoke(i);

                    break;
                }
            }
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
// [x] Score
// [ ] Credits