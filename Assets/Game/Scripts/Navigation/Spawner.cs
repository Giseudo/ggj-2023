using System;
using System.Collections;
using System.Collections.Generic;
using Game.Combat;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Splines;
using DG.Tweening;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    private GameObject _prefab;

    [SerializeField]
    private SplineContainer _spline;

    [SerializeField]
    private float _spawnInterval = 2f;

    private ObjectPool<GameObject> _pool;

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

                splineAnimate.Container = _spline;
                splineAnimate.AnimationMethod = SplineAnimate.Method.Speed;
                splineAnimate.MaxSpeed = 4f;
                splineAnimate.Restart(true);

                instance.transform.localScale = Vector3.one;

                instance.gameObject.SetActive(true);
            },
            (instance) => {
                instance.gameObject.SetActive(false);
            },
            (instance) => {
                if (instance.TryGetComponent<Damageable>(out Damageable damageable))
                    damageable.died -= OnDie;

                Destroy(instance);
            },
            true,
            30
        );
    }

    public void OnDestroy()
    {
        _pool.Dispose();
    }

    public void Start()
    {
        StartCoroutine(Spawn());
    }

    private void OnDie(Damageable damageable)
    {
        damageable.transform.DOScale(Vector3.zero, .5f)
            .OnComplete(() => _pool.Release(damageable.gameObject));
    }

    public IEnumerator Spawn()
    {
        while (_pool.CountActive < 30)
        {
            _pool.Get();

            yield return new WaitForSeconds(_spawnInterval);
        }
    }
}
