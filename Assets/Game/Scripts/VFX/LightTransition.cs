using System;
using UnityEngine;
using DG.Tweening;
using Game.Core;

public class LightTransition : MonoBehaviour
{
    [SerializeField]
    private Color _color = Color.white;

    [SerializeField]
    private float _intensity = 1f;

    [SerializeField]
    private Vector3 _rotation = Vector3.zero;

    [SerializeField]
    private float _transitionTime = 2f;

    private Light _light;

    public Action finished = delegate { };

    public void Awake()
    {
        TryGetComponent<Light>(out _light);
    }

    public void Start()
    {
        GameManager.SetMainLight(_light);
    }

    public void StartTransition(Action callback)
    {
        _light.DOIntensity(_intensity, _transitionTime);
        _light.transform.DORotate(_rotation, _transitionTime);
        _light.DOColor(_color, _transitionTime)
            .OnComplete(() => {
                finished.Invoke();
                callback();
            });
    }
}
