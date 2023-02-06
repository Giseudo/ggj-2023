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
        private GameObject _prefab;

        [SerializeField]
        private SplineContainer _spline;

        [SerializeField]
        private float _spawnInterval = 2f;

        [SerializeField]
        private int _limit;

        private int _deathCount;
        private int _spawnedCount;

        private ObjectPool<GameObject> _pool;

        public ObjectPool<GameObject> Pool => _pool;
        public GameObject Prefab => _prefab;
        public SplineContainer Spline => _spline;
        public int Limit => _limit;

        public Action creepsDied = delegate { };

        public void SetPrefab(GameObject prefab) => _prefab = prefab;
        public void SetInterval(float interval) => _spawnInterval = interval;
        public void SetSpline(SplineContainer spline) => _spline = spline;
        public void SetLimit(int limit) => _limit = limit;

        public void Awake()
        {
            _pool = new ObjectPool<GameObject>(
                () => {
                    GameObject instance = GameObject.Instantiate(_prefab);

                    if (instance.TryGetComponent<Damageable>(out Damageable damageable))
                        damageable.died += OnDie;

                    return instance;
                },
                (instance) => {
                    if (!instance.TryGetComponent<SplineAnimate>(out SplineAnimate splineAnimate))
                        splineAnimate = instance.AddComponent<SplineAnimate>();

                    splineAnimate.PlayOnAwake = false;
                    splineAnimate.Container = _spline;
                    splineAnimate.AnimationMethod = SplineAnimate.Method.Speed;
                    splineAnimate.Restart(true);

                    instance.transform.localScale = Vector3.one;
                    instance.gameObject.SetActive(true);

                    _spawnedCount++;
                },
                (instance) => {
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
            damageable.transform.DOScale(Vector3.zero, .5f)
                .OnComplete(() => _pool.Release(damageable.gameObject));
            
            _deathCount++;

            Debug.Log($"{_deathCount} / {_limit}");

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