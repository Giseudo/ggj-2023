using System;
using System.Collections;
using Game.Core;
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

        //[SerializeField]
        //private float _spawnInterval = 2f;

        //[SerializeField]
        //private int _limit;

        private ObjectPool<Creep> _pool;

        public ObjectPool<Creep> Pool => _pool;
        public Creep Prefab => _prefab;
        public SplineContainer Spline => _spline;
        // public int Limit => _limit;

        public Action<Creep> creepDied = delegate { };
        public Action creepsDied = delegate { };

        public void SetPrefab(Creep prefab) => _prefab = prefab;
        //public void SetInterval(float interval) => _spawnInterval = interval;
        public void SetSpline(SplineContainer spline) => _spline = spline;
        //public void SetLimit(int limit) => _limit = limit;

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

        public void Play(float delay, int limit, float interval, Action callback)
        {
            StartCoroutine(Spawn(delay, new RoundCounter { Limit = limit, Interval = interval }, callback));
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

        public IEnumerator Spawn(float delay, RoundCounter counter, Action callback)
        {
            void CheckCreepsDeath(Creep creep, Damageable damageable)
            {
                if (counter.Deaths >= counter.Limit)
                    callback();
                
                if (damageable.Health > 0) return;

                creepDied.Invoke(creep);
            }

            void SpawnChild(Creep parentCreep, Damageable parentDamageable, CreepChildDeath creepChildDeath)
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
                            counter.Deaths++;
                            CheckCreepsDeath(parentCreep, parentDamageable);
                        }

                        if (spawnedCreep.Data.CreepDeathSpawn != null)
                        {
                            SpawnChild(spawnedCreep, damageable, creepChildDeath);
                            return;
                        }

                        if (damageable.Health > 0) return;

                        spawner.OnCreepDeath(spawnedCreep);
                    }

                    damageable.died += OnSpawnDeath;
                }

                spawner.OnCreepDeath(parentCreep);
            }

            void OnDie(Damageable damageable)
            {
                damageable.died -= OnDie;
                damageable.TryGetComponent<Creep>(out Creep creep);
                damageable.transform.DOScale(Vector3.zero, .5f)
                    .OnComplete(() => _pool.Release(creep));
                
                if (creep.Data.CreepDeathSpawn != null)
                {
                    SpawnChild(creep, damageable, new CreepChildDeath { TotalCreepsToDie = creep.TotalCreepsToSpawn });
                    return;
                }

                counter.Deaths += 1;

                CheckCreepsDeath(creep, damageable);
            }

            yield return new WaitForSeconds(delay);

            while (counter.Count < counter.Limit)
            {
                Creep creep = _pool.Get();

                if (creep.TryGetComponent<Damageable>(out Damageable damageable))
                    damageable.died += OnDie;
                
                counter.Count += 1;

                yield return new WaitForSeconds(counter.Interval);
            }
        }
    }


    public class RoundCounter
    {
        public int Limit { get; set; }
        public float Interval { get; set; }
        public int Count { get; set; }
        public int Deaths { get; set; }
    }

   public class CreepChildDeath
    {
        public int count { get; set; }
        public int TotalCreepsToDie { get; set; }
        public bool ReachedLimit => count >= TotalCreepsToDie;
        public void Increase() => count += 1;
    }
}