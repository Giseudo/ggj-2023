using System;
using System.Collections;
using System.Collections.Generic;
using Game.Combat;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Splines;
using DG.Tweening;

namespace Game.Combat
{
    public class CreepSpawner : MonoBehaviour
    {
        [SerializeField]
        private Creep _prefab;

        [SerializeField]
        private SplineContainer _spline;

        [SerializeField]
        private float _spawnInterval = 2f;

        [SerializeField]
        private int _limit;

        private int _deathCount;
        private int _spawnedCount;

        private ObjectPool<Creep> _pool;

        public ObjectPool<Creep> Pool => _pool;
        public Creep Prefab => _prefab;
        public SplineContainer Spline => _spline;
        public int Limit => _limit;

        public Action<Creep> creepDied = delegate { };
        public Action creepsDied = delegate { };

        public void SetPrefab(Creep prefab) => _prefab = prefab;
        public void SetInterval(float interval) => _spawnInterval = interval;
        public void SetSpline(SplineContainer spline) => _spline = spline;
        public void SetLimit(int limit) => _limit = limit;

        public void Awake()
        {
            _pool = new ObjectPool<Creep>(
                () => {
                    Creep instance = GameObject.Instantiate<Creep>(_prefab);

                    return instance;
                },
                (instance) => {
                    instance.transform.position = _spline.EvaluatePosition(0f);

                    instance.SetSpline(_spline);
                    instance.transform.localScale = Vector3.one;
                    instance.gameObject.SetActive(true);
                },
                (instance) => {
                    instance.transform.position = _spline.EvaluatePosition(0f);

                    instance.gameObject.SetActive(false);
                },
                (instance) => {
                    if (instance == null) return;

                    Destroy(instance);
                },
                true,
                100
            );
        }

        public void OnDestroy()
        {
            _pool.Dispose();
        }

        private void OnDie(Damageable damageable)
        {
            damageable.died -= OnDie;
            damageable.TryGetComponent<Creep>(out Creep creep);
            damageable.transform.DOScale(Vector3.zero, .5f)
                .OnComplete(() => _pool.Release(creep));
            
            if (creep.Data.CreepDeathSpawn != null)
            {
                SpawnChild(creep, damageable, new CreepChildDeath { totalCreepsToDie = creep.TotalCreepsToSpawn });
                return;
            }

            _deathCount++;

            CheckCreepsDeath(creep, damageable);
        }

        private void SpawnChild(Creep parentCreep, Damageable parentDamageable, CreepChildDeath creepChildDeath)
        {
            WaveSpawner spawner = MatchManager.WaveSpawners?.Find(spawner => spawner.Spline == Spline);
            if (!spawner.Spawners.TryGetValue(parentCreep.Data.CreepDeathSpawn, out CreepSpawner creepSpawner)) return;

            for (int i = 0; i < parentCreep.Data.DeathSpawnCount; i++)
            {
                Creep spawnedCreep = creepSpawner.Spawn();
                spawnedCreep.SetSpline(Spline, parentCreep.Displacement - ((i + 1) * 3));

                if (!spawnedCreep.TryGetComponent<Damageable>(out Damageable damageable)) return;

                void OnSpawnDeath(Damageable damageable)
                {
                    damageable.died -= OnSpawnDeath;
                    creepChildDeath.Increase();

                    damageable.TryGetComponent<Creep>(out Creep creep);
                    damageable.transform.DOScale(Vector3.zero, .5f)
                        .OnComplete(() => creepSpawner.Pool.Release(spawnedCreep));

                    if (creepChildDeath.ReachedLimit)
                    {
                        _deathCount++;
                        CheckCreepsDeath(parentCreep, parentDamageable);
                    }

                    if (damageable.Health > 0) return;

                    if (spawnedCreep.Data.CreepDeathSpawn != null)
                    {
                        SpawnChild(spawnedCreep, damageable, creepChildDeath);
                        return;
                    }

                    spawner.OnCreepDeath(spawnedCreep);
                }

                damageable.died += OnSpawnDeath;
            }

            spawner.OnCreepDeath(parentCreep);
        }

        private void CheckCreepsDeath(Creep creep, Damageable damageable)
        {
            if (_deathCount >= _limit)
                creepsDied.Invoke();
            
            if (damageable.Health > 0) return;

            creepDied.Invoke(creep);
        }

        public void Play(float delay)
        {
            _deathCount = 0;
            _spawnedCount = 0;

            StartCoroutine(Spawn(delay));
        }

        public void Stop()
        { }

        public Creep Spawn()
        {
            Creep creep = _pool.Get();
            WaveSpawner spawner = MatchManager.WaveSpawners?.Find(spawner => spawner.Spline == Spline);

            if (creep.Data.CreepDeathSpawn)
            {
                if (!spawner.Spawners.TryGetValue(creep.Data.CreepDeathSpawn, out CreepSpawner creepSpawner))
                {
                    creepSpawner = GameObject.Instantiate<CreepSpawner>(spawner.CreepSpawnerPrefab, spawner.transform);
                    creepSpawner.SetPrefab(creep.Data.CreepDeathSpawn.Prefab);
                    creepSpawner.SetSpline(spawner.Spline);

                    spawner.Spawners.Add(creep.Data.CreepDeathSpawn, creepSpawner);
                }
            }

            return creep;
        }

        public IEnumerator Spawn(float delay)
        {
            yield return new WaitForSeconds(delay);

            while (_spawnedCount < _limit)
            {
                Creep creep = _pool.Get();

                if (creep.TryGetComponent<Damageable>(out Damageable damageable))
                    damageable.died += OnDie;

                _spawnedCount++;

                yield return new WaitForSeconds(_spawnInterval);
            }
        }
    }

    public class CreepChildDeath
    {
        public int count { get; set; }
        public int totalCreepsToDie { get; set; }
        public bool ReachedLimit => count >= totalCreepsToDie;
        public void Increase() => count += 1;
    }
}