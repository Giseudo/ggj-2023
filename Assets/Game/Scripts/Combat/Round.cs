using System;
using System.Collections.Generic;
using UnityEngine;

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
        private float _roundTime;

        public List<Wave> Waves => _waves;
        public float RoundTime => _roundTime;

        public Action over = delegate { };
        public Action<Creep> creepDied = delegate { };

        private void OnCreepDeath(Creep creep)
        {
            creepDied.Invoke(creep);
        }

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
                    // spawner.SetInterval(wave.SpawnInterval);
                    // spawner.SetLimit(wave.CountLimit);
                    spawner.SetSpline(_waveSpawner.Spline);

                    _waveSpawner.Spawners.Add(wave.CreepData, spawner);
                }
            }

            CalculateTime();
        }

        public void Destroy()
        {
            for (int i = 0; i < _waves.Count; i++)
            {
                Wave wave = _waves[i];

                if (!_waveSpawner.Spawners.TryGetValue(wave.CreepData, out CreepSpawner spawner))
                    continue;

                spawner.creepDied -= OnCreepDeath;
            }
        }

        public void Start()
        {
            Debug.Log($"{_waveSpawner.name}: started {_name}");
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

                //spawner.SetInterval(wave.SpawnInterval);
                //spawner.SetLimit(wave.CountLimit);

                if (wave.CreepData.CreepDeathSpawn)
                {
                    if (!_waveSpawner.Spawners.TryGetValue(wave.CreepData.CreepDeathSpawn, out CreepSpawner creepSpawner))
                    {
                        creepSpawner = GameObject.Instantiate<CreepSpawner>(_waveSpawner.CreepSpawnerPrefab, spawner.transform);
                        creepSpawner.SetPrefab(wave.CreepData.CreepDeathSpawn.Prefab);
                        creepSpawner.SetSpline(spawner.Spline);

                        _waveSpawner.Spawners.Add(wave.CreepData.CreepDeathSpawn, creepSpawner);
                    }
                }

                spawner.creepDied += OnCreepDeath;
 
                spawner.Play(wave.InitialDelay, wave.CountLimit, wave.SpawnInterval, OnCreepsDeath);
            }
        }

        private void OnCreepsDeath()
        {
            _endedWavesCount++;

            if (_endedWavesCount >= _waves.Count)
                over.Invoke();
        }

        private void CalculateTime()
        {
            _waves.ForEach(wave => {
                float waveTime = wave.InitialDelay + ((wave.CountLimit - 1) * wave.SpawnInterval);

                if (waveTime > _roundTime)
                    _roundTime = waveTime;
            });
        }
    }
}