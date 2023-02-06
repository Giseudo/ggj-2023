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

                    if (instance.TryGetComponent<Damageable>(out Damageable damageable))
                        damageable.died += OnDie;

                    return instance;
                },
                (instance) => {
                    instance.transform.position = _spline.EvaluatePosition(0f);

                    instance.SetSpline(_spline);
                    instance.transform.localScale = Vector3.one;
                    instance.gameObject.SetActive(true);

                    _spawnedCount++;
                },
                (instance) => {
                    instance.transform.position = _spline.EvaluatePosition(0f);

                    if (instance.TryGetComponent<Damageable>(out Damageable damageable))
                        damageable.Revive();

                    instance.gameObject.SetActive(false);
                },
                (instance) => {
                    if (instance.TryGetComponent<Damageable>(out Damageable damageable))
                        damageable.died -= OnDie;

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
            damageable.TryGetComponent<Creep>(out Creep creep);
            damageable.transform.DOScale(Vector3.zero, .5f)
                .OnComplete(() => _pool.Release(creep));
            
            _deathCount++;

            if (_deathCount >= _limit)
                creepsDied.Invoke();
        }

        public void Play(float delay)
        {
            _deathCount = 0;
            _spawnedCount = 0;

            StartCoroutine(Spawn(delay));
        }

        public void Stop()
        { }

        public IEnumerator Spawn(float delay)
        {
            yield return new WaitForSeconds(delay);

            while (_spawnedCount < _limit)
            {
                _pool.Get();

                yield return new WaitForSeconds(_spawnInterval);
            }
        }
    }
}