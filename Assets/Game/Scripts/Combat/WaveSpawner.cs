using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace Game.Combat
{
    [Serializable]
    public class Round
    {
        [SerializeField]
        private List<Wave> _waves = new List<Wave>();

        private int _endedWavesCount = 0;

        private WaveSpawner _waveSpawner;

        public Action over = delegate { };

        public void Awake(WaveSpawner waveSpawner)
        {
            _waveSpawner = waveSpawner;

            for (int i = 0; i < _waves.Count; i++)
            {
                Wave wave = _waves[i];

                if (!_waveSpawner.Spawners.TryGetValue(wave.CreepData, out CreepSpawner spawner))
                {
                    spawner = GameObject.Instantiate<CreepSpawner>(_waveSpawner.CreepSpawnerPrefab, _waveSpawner.transform);
                    spawner.SetPrefab(wave.CreepData.Prefab);
                    spawner.SetInterval(wave.SpawnInterval);
                    spawner.SetLimit(wave.CountLimit);
                    spawner.SetSpline(_waveSpawner.Spline);

                    _waveSpawner.Spawners.Add(wave.CreepData, spawner);
                }
            }
        }

        public void Destroy()
        {
            for (int i = 0; i < _waves.Count; i++)
            {
                Wave wave = _waves[i];

                if (!_waveSpawner.Spawners.TryGetValue(wave.CreepData, out CreepSpawner spawner))
                    continue;

                spawner.creepsDied -= OnCreepsDeath;
            }
        }

        public void Start()
        {
            Debug.Log("Round start");

            for (int i = 0; i < _waves.Count; i++)
            {
                Wave wave = _waves[i];

                if (!_waveSpawner.Spawners.TryGetValue(wave.CreepData, out CreepSpawner spawner))
                    continue;

                spawner.SetInterval(wave.SpawnInterval);
                spawner.SetLimit(wave.CountLimit);

                spawner.creepsDied += OnCreepsDeath;
 
                spawner.Play(wave.InitialDelay);
            }
        }

        private void OnCreepsDeath()
        {
            _endedWavesCount++;

            if (_endedWavesCount >= _waves.Count)
            {
                over.Invoke();

                for (int i = 0; i < _waves.Count; i++)
                {
                    Wave wave = _waves[i];

                    if (!_waveSpawner.Spawners.TryGetValue(wave.CreepData, out CreepSpawner spawner))
                        continue;

                    spawner.creepsDied -= OnCreepsDeath;
                }
            }
        }
    }

    [Serializable]
    public class Wave
    {
        [SerializeField]
        private CreepData _creepData;

        [SerializeField]
        private int _countLimit;

        [SerializeField]
        private float _spawnInterval;

        [SerializeField]
        private float _initialDelay;

        public CreepData CreepData => _creepData;
        public int CountLimit => _countLimit;
        public float SpawnInterval => _spawnInterval;
        public float InitialDelay => _initialDelay;
    }

    [Serializable]
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField]
        private List<Round> _rounds = new List<Round>();

        [SerializeField]
        private SplineContainer _spline;

        [SerializeField]
        private CreepSpawner _creepSpawnerPrefab;

        [SerializeField]
        private int _currentRound;

        private Dictionary<CreepData, CreepSpawner> _spawners = new Dictionary<CreepData, CreepSpawner>();

        public CreepSpawner CreepSpawnerPrefab => _creepSpawnerPrefab;
        public Dictionary<CreepData, CreepSpawner> Spawners => _spawners;
        public SplineContainer Spline => _spline;

        public Action finished = delegate { };

        public void OnDestroy()
        {
            MatchManager.RemoveWaveSpawner(this);

            for (int i = 0; i < _rounds.Count; i++)
            {
                Round round = _rounds[i];

                round.Destroy();
            }
        }

        public void Start()
        {
            MatchManager.AddWaveSpawner(this);

            for (int i = 0; i < _rounds.Count; i++)
            {
                Round round = _rounds[i];

                round.Awake(this);
            }

            StartRound();
        }

        public void StartRound()
        {
            Round round = _rounds[_currentRound];

            if (round == null) return;

            round.Start();

            round.over += OnRoundOver;
        }

        public void OnRoundOver()
        {
            _currentRound++;

            if (_currentRound >= _rounds.Count)
            {
                finished.Invoke();
                return;
            }

            StartRound();
        }
    }
}