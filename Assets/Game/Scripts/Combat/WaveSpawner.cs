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
        private string _name;

        [SerializeField]
        private List<Wave> _waves = new List<Wave>();

        private int _endedWavesCount = 0;

        private WaveSpawner _waveSpawner;

        public Action over = delegate { };
        public Action<Creep> creepDied = delegate { };

        private void OnCreepDeath(Creep creep) => creepDied.Invoke(creep);

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

                spawner.creepDied -= OnCreepDeath;
                spawner.creepsDied -= OnCreepsDeath;
            }
        }

        public void Start()
        {
            if (_waves.Count <= 0)
            {
                over.Invoke();
                return;
            }

            for (int i = 0; i < _waves.Count; i++)
            {
                Wave wave = _waves[i];

                if (!_waveSpawner.Spawners.TryGetValue(wave.CreepData, out CreepSpawner spawner))
                    continue;

                spawner.SetInterval(wave.SpawnInterval);
                spawner.SetLimit(wave.CountLimit);

                spawner.creepDied += OnCreepDeath;
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

        private int _currentRound = 0;

        private Dictionary<CreepData, CreepSpawner> _spawners = new Dictionary<CreepData, CreepSpawner>();

        public CreepSpawner CreepSpawnerPrefab => _creepSpawnerPrefab;
        public Dictionary<CreepData, CreepSpawner> Spawners => _spawners;
        public SplineContainer Spline => _spline;

        public Action finished = delegate { };
        public Action roundOver = delegate { };
        public Action<Creep> creepDied = delegate { };

        public void OnCreepDeath(Creep creep) => creepDied.Invoke(creep);

        public void Start()
        {
            MatchManager.AddWaveSpawner(this);

            for (int i = 0; i < _rounds.Count; i++)
            {
                Round round = _rounds[i];

                round.creepDied += OnCreepDeath;
                round.over += OnRoundOver;

                round.Awake(this);
            }
        }

        public void OnDestroy()
        {
            MatchManager.RemoveWaveSpawner(this);

            for (int i = 0; i < _rounds.Count; i++)
            {
                Round round = _rounds[i];

                round.creepDied -= OnCreepDeath;
                round.over -= OnRoundOver;

                round.Destroy();
            }
        }

        public void StartRound()
        {
            Round round = _rounds[_currentRound];

            if (round == null) return;

            round.Start();
        }

        public void OnRoundOver()
        {
            if (_currentRound >= _rounds.Count - 1)
            {
                finished.Invoke();
                return;
            }

            _currentRound++;
            roundOver.Invoke();
        }
    }
}