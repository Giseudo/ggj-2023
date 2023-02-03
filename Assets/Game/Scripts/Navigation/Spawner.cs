using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    private GameObject _prefab;

    [SerializeField]
    private SplineContainer _spline;

    [SerializeField]
    private float _spawnInterval = 2f;

    private float _count;

    public void Start()
    {
        StartCoroutine(Spawn());
            GameObject instance = GameObject.Instantiate(_prefab);
    }

    public IEnumerator Spawn()
    {
        while (_count < 20)
        {

/*
            if (!instance.TryGetComponent<SplineAnimate>(out SplineAnimate splineAnimate))
                splineAnimate = instance.AddComponent<SplineAnimate>();

            splineAnimate.Container = _spline;
            */

            _count++;

            yield return new WaitForSeconds(_spawnInterval);
        }
    }
}
