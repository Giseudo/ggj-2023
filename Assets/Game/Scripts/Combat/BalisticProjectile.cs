using System;
using UnityEngine;
using DG.Tweening;
using Freya;

namespace Game.Combat
{
    [ExecuteInEditMode]
    public class BalisticProjectile : MonoBehaviour, IProjectile
    {
        [SerializeField]
        private Transform _target;

        [SerializeField]
        private float _height;

        [SerializeField]
        private float _hitTime = 2f;

        [SerializeField]
        private float _hitRadius = 2f;

        [SerializeField]
        private int _attackDamage = 1;

        [SerializeField]
        private float _targetAheadDistance = 5f;

        private float t;

        private Vector3 _initialPosition;
        private Vector3 _targetPosition;
        private Tween _tween;

        public void SetTarget(Transform target) => _target = target;
        public Action<IProjectile, Damageable> collided = delegate { };
        public Action<IProjectile, Damageable> Collided { get => collided; set => collided = value; }
        public Action<IProjectile> died = delegate { };
        public Action<IProjectile> Died { get => died; set => died = value; }
        public GameObject GameObject => gameObject;

        public void OnEnable()
        { }

        public void OnDisable()
        {
            t = 0f;

            Died.Invoke(this);
        }

        public void Fire()
        {
            _initialPosition = transform.position;
            _targetPosition = _target.position;

            t = 0f;

            _tween?.Kill();
            _tween = DOTween.To(() => t, x => t = x, 1f, _hitTime)
                .OnUpdate(() => {
                    float distance = (_targetPosition - _initialPosition).magnitude;
                    float speed = distance / t;
                    float time = distance / speed;

                    Vector3 p = Vector3.Lerp(_initialPosition, _targetPosition, time);
                    Vector3 h = Vector3.Lerp(Vector3.zero, Vector3.up * _height, Mathf.Pow(((Mathf.Cos((time + 0.5f) * Mathfs.TAU)) + 1f) * 0.5f, 0.3f));

                    transform.position = p + h;
                })
                .OnComplete(() => {
                    Collider[] colliders = Physics.OverlapSphere(transform.position, _hitRadius, 1 << LayerMask.NameToLayer("Creep"));

                    for (int i = 0; i < colliders.Length; i++)
                    {
                        Collider collider = colliders[i];

                        if (!collider.TryGetComponent<Damageable>(out Damageable damageable))
                            continue;

                        damageable.Hurt(_attackDamage);

                        Collided.Invoke(this, damageable);
                    }
                });
        }
    }
}